using Rcon.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Terminal.Gui;

namespace VU.Server
{
    internal sealed partial class ServerWindow : Window
    {
        private const int PROCESS_DETECTION_MS = 30000; // Every 30 seconds

        // Paths
        private readonly string _vuPath;
        private readonly string _gamePath;
        private readonly string _instancePath;
        private readonly string _configPath;
        private readonly List<string> _arguments;

        // Server configuration (Startup.txt)
        private Dictionary<string, string> _config;

        // Server process and RCON client
        private Process _process;
        private Client _rconClient;
        private int _rconPort;
        private string _rconPassword;

        public ServerWindow(string vuPath,
            string gamePath,
            string instancePath,
            string configPath,
            int rconPort,
            List<string> arguments)
            : base($"VU server: {instancePath}")
        {
            _vuPath = vuPath;
            _gamePath = gamePath;
            _instancePath = instancePath;
            _configPath = configPath;
            _rconPort = rconPort;
            _arguments = arguments;

            // Create user interface
            CreateUserInterface();

            // Load configuration and start server
            LoadConfig();
            StartServer();

            // Add input field handler
            _inputTextField.KeyDown += InputTextField_KeyDown;

            // Add process exit detection
            Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(PROCESS_DETECTION_MS), _ =>
            {
                if (_process != null && _process.HasExited)
                {
                    WriteToLog($"Server exited unexpectedly with code {_process.ExitCode}, restarting...");

                    // Load configuration and start server
                    LoadConfig();
                    StartServer();
                }

                return true; // Prevent removal of idle function
            });
        }

        protected override void Dispose(bool disposing)
        {
            // Shutdown server if it is running
            if (_process != null && !_process.HasExited)
                StopServer();

            base.Dispose(disposing);
        }

        private void LoadConfig()
        {
            _config = new Dictionary<string, string>();
            var lines = File.ReadAllLines(_configPath);
            foreach (var line in lines)
            {
                if (line.StartsWith("#"))
                    continue;

                // Split into parts, this also works for strings that are enclosed in quotes
                var parts = Utility.SplitStringBySpace(line);
                if (parts.Count >= 2)
                    _config.Add(parts[0], parts[1].Replace("\"", string.Empty));
            }
        }

        private void StartServer()
        {
            if (_process != null && !_process.HasExited)
                throw new InvalidOperationException("Server is already running");

            // Create RCON client
            _rconClient = new Client();
            _rconPassword = _config["admin.password"];

            // Start the server in headless mode and redirect it's output
            _process = new Process();
            _process.StartInfo.FileName = Path.Combine(_vuPath, "vu.exe");
            _process.StartInfo.WorkingDirectory = _vuPath;
            _process.StartInfo.CreateNoWindow = false;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.Arguments = "-server -dedicated -headless " + string.Join(' ', _arguments);
            _process.StartInfo.Arguments += $" -serverInstancePath \"{_instancePath}\"";
            if (!string.IsNullOrWhiteSpace(_gamePath)) _process.StartInfo.Arguments += $" -gamepath \"{_gamePath}\"";

            _process.Start();

            _process.OutputDataReceived += Process_OutputDataReceived;
            _process.BeginOutputReadLine();
        }

        private void StopServer()
        {
            if (_process == null || _process.HasExited)
                throw new InvalidOperationException("Server is not running");

            if (_rconClient.IsOpen)
                _rconClient.Close();

            _process.Kill();

            _process = null;
            _rconClient = null;
        }

        private async void Process_OutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            var line = args.Data;
            if (string.IsNullOrWhiteSpace(line))
                return;

            // Push the line to the list view
            WriteToLog(line);

            // Check if the RCON client should be started by matching regex to see if RCON has started on the server
            if (Regex.Match(line, @"\[.+\] \[.+\] Remote Administration interface is listening on port 0.0.0.0:\d+").Success)
            {
                // Create connection to the RCON port on the running server so we can send commands to it from the inbuilt console
                _rconClient.Open(IPAddress.Loopback, _rconPort);

                // Login with admin credentials
                await _rconClient.SendMessageAsync("login.plainText", _rconPassword);
            }
        }

        private void Command_StartServer()
        {
            WriteToLog("Starting server...");
            if (_process == null)
                StartServer();
        }

        private void Command_StopServer()
        {
            WriteToLog("Stopped server");
            if (_process != null && !_process.HasExited)
                StopServer();
        }

        private void Command_RestartServer()
        {
            WriteToLog("Stopping server...");
            if (_process != null && !_process.HasExited)
                StopServer();

            WriteToLog("Starting server...");
            LoadConfig();
            StartServer();
        }

        private void Command_Exit()
        {
            if (_process != null && !_process.HasExited)
                StopServer();

            // Dispose of current resources and close window, this will stop the application automatically
            Dispose();
        }

        private async void Command_SendRCONCommand(List<string> words)
        {
            try
            {
                if (!_rconClient.IsOpen)
                {
                    WriteToLog("[Console] RCON unavailable!");
                    return;
                }

                var responseWords = await _rconClient.SendMessageAsync(words);
                WriteToLog($"[Server] RCON: {string.Join(' ', responseWords)}");
            }
            catch (ArgumentException ex)
            {
                // The command we sent was invalid and returned a bad status
                WriteToLog($"[Server] RCON: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                // Thrown when a connection to the server is not open and a message request is sent,
                // this can happen if the server unexpectedly shut down
                WriteToLog($"[Console] RCON: {ex}");
                WriteToLog($"[Console] RCON: Server might be offline!");
            }
            catch (Exception ex)
            {
                // General sending failure
                WriteToLog($"[Console] RCON: {ex}");
            }
        }

        private void InputTextField_KeyDown(KeyEventEventArgs args)
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                var words = Utility.SplitStringBySpace(_inputTextField.Text.ToString());
                if (words.Count == 0) return;

                switch (words[0])
                {
                    case "start":
                        Command_StartServer();
                        break;
                    case "stop":
                        Command_StopServer();
                        break;
                    case "restart":
                        Command_RestartServer();
                        break;
                    case "exit":
                    case "quit":
                        Command_Exit();
                        break;
                    default:
                        Command_SendRCONCommand(words);
                        break;
                }

                _inputTextField.Text = string.Empty;
            }
        }
    }
}
