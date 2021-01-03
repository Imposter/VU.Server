using System;
using System.Collections.Generic;
using Terminal.Gui;

namespace VU.Server
{
    internal sealed partial class ServerWindow : Window
    {
        private const int PROCESS_DETECTION_MS = 30000; // Every 30 seconds

        // Command line options
        private readonly Options _options;

        // Server process
        private Server _server;

        public ServerWindow(Options options)
            : base($"VU server: {options.InstancePath}")
        {
            _options = options;

            // Create user interface
            CreateUserInterface();

            // Load configuration and start server
            _server = new Server(options);
            _server.LogOutput += Server_LogOutput;
            _server.Refreshed += Server_Refreshed;
            _server.Start();

            // Add input field handler
            _inputTextField.KeyDown += InputTextField_KeyDown;

            // Add process exit detection
            Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(PROCESS_DETECTION_MS), _ =>
            {
                if (!_server.Running)
                {
                    WriteToLog($"Server exited unexpectedly with code {_server.ExitCode}, restarting...");

                    // Load configuration and start server
                    _server.Start();
                }

                return true; // Prevent removal of idle function
            });
        }

        protected override void Dispose(bool disposing)
        {
            // Dispose of any resources (including the child server process)
            _server.Dispose();

            base.Dispose(disposing);
        }

        private void Server_LogOutput(string line)
        {
            // Push the line to the list view
            WriteToLog(line);
        }

        private void Server_Refreshed()
        {
            // Update labels
            _upTimeLabel.Text = $"Uptime: {_server.UpTime:hh\\:mm\\:ss}";
            _fpsLabel.Text = $"FPS: {_server.Fps}";
            _stateLabel.Text = $"State: {_server.State}";
            _playersLabel.Text = $"Players: {_server.PlayerCount}/{_server.PlayerLimit}";
            _mapLabel.Text = $"Map: {_server.Map}";
            _modeLabel.Text = $"Mode: {_server.Mode}";
        }

        private void Command_StartServer()
        {
            WriteToLog("Starting server...");
            if (!_server.Running)
                _server.Start();
        }

        private void Command_StopServer()
        {
            WriteToLog("Stopped server");
            if (_server.Running)
                _server.Stop();
        }

        private void Command_RestartServer()
        {
            WriteToLog("Stopping server...");
            if (_server.Running)
                _server.Stop();

            WriteToLog("Starting server...");
            _server.Start();
        }

        private void Command_Exit()
        {
            if (_server.Running)
                _server.Stop();

            // Dispose of current resources and close window
            Dispose();

            // Request to stop the application
            Application.RequestStop();
        }

        private async void Command_SendRCONCommand(List<string> words)
        {
            try
            {
                if (!_server.CanSendCommands)
                {
                    WriteToLog("[Console] RCON unavailable!");
                    return;
                }

                var responseWords = await _server.SendCommandAsync(words);
                WriteToLog($"[Server] RCON: {string.Join(' ', responseWords)}");
            }
            catch (Exception ex)
            {
                WriteToLog($"[Console] RCON: {ex.Message}");

                // TODO: Write verbose details to log file
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
