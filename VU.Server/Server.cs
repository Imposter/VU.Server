using Rcon.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace VU.Server
{
    internal sealed class Server : IDisposable
    {
        #region Constants and Variables

        private const int REFRESH_FREQUENCY = 1; // Hz

        // Command line options
        private readonly Options _options;

        // Server configuration (Startup.txt)
        private Dictionary<string, string> _config;

        // Server process and RCON client
        private Process _process;
        private Thread _refreshThread;
        private Client _rconClient;
        private string _rconPassword;

        private float _cpuUsage;

        #endregion

        #region Properties

        public bool Running => _process != null && !_process.HasExited;
        public int ExitCode => _process != null && _process.HasExited ? _process.ExitCode : 0;
        public bool CanSendCommands => _rconClient != null && _rconClient.IsOpen;
        public float CPUUsage => _cpuUsage;
        public long MemoryUsage => _process != null ? _process.VirtualMemorySize64 : 0;
        public TimeSpan UpTime => _process != null ? DateTime.Now - _process.StartTime : TimeSpan.Zero;

        // Game state
        public int Fps { get; private set; }
        public string State { get; private set; }
        public int PlayerCount { get; private set; }
        public int PlayerLimit { get; private set; }
        public string Map { get; private set; }
        public string Mode { get; private set; }

        #endregion

        #region Events

        public event ActionDelegate<string> LogOutput;
        public event ActionDelegate<IList<string>> DataReceived;
        public event ActionDelegate Refreshed;

        #endregion

        public Server(Options options)
        {
            _options = options;
        }

        public void Dispose()
        {
            // Shutdown server if it is running
            if (_process != null && !_process.HasExited)
                Stop();
        }

        public void Start()
        {
            if (_process != null && !_process.HasExited)
                throw new InvalidOperationException("Server is already running");

            // Load the new config
            LoadConfig();

            // Create RCON client
            _rconClient = new Client();
            _rconClient.WordsReceived += RconClient_WordsReceived;
            _rconPassword = _config["admin.password"];

            // TODO: Run WINE for Linux

            // Start the server in headless mode and redirect it's output
            _process = new Process();
#if WINDOWS
            _process.StartInfo.FileName = Path.Combine(_options.VeniceUnleashedPath, "vu.exe");
#elif LINUX
            // If we're running under linux, we need to use wine
            _process.StartInfo.FileName = "wine";
#else
#error Unsupported platform!
#endif
            _process.StartInfo.WorkingDirectory = _options.VeniceUnleashedPath;
            _process.StartInfo.CreateNoWindow = false;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;

#if LINUX
            // Set path to VU
            _process.StartInfo.Arguments = Path.Combine(_options.VeniceUnleashedPath, "vu.com");
#endif

            // Set required console flags
            _process.StartInfo.Arguments += " -server -dedicated -headless";

            // Set instance path
            _process.StartInfo.Arguments += $" -serverInstancePath \"{_options.InstancePath}\"";

            // Set game path if one was provided
            if (!string.IsNullOrWhiteSpace(_options.GamePath))
                _process.StartInfo.Arguments += $" -gamepath \"{_options.GamePath}\"";

            // Set ports
            _process.StartInfo.Arguments += $" -listen 0.0.0.0:{_options.GamePort}";
            _process.StartInfo.Arguments += $" -mHarmonyPort {_options.HarmonyPort}";
            _process.StartInfo.Arguments += $" -RemoteAdminPort 0.0.0.0:{_options.RemotePort}";

            // Server-specific arguments
            if (_options.Unlisted)
                _process.StartInfo.Arguments += " -unlisted";
            if (_options.NoUpdate)
                _process.StartInfo.Arguments += " -noUpdate";
            if (_options.HighResTerrain)
                _process.StartInfo.Arguments += " -highResTerrain";
            if (_options.DisableTerrainInterop)
                _process.StartInfo.Arguments += " -disableTerrainInterop";
            if (_options.SkipChecksum)
                _process.StartInfo.Arguments += " -skipChecksum";

            // VU arguments
            _process.StartInfo.Arguments += $" -env {_options.Environment}";
            if (_options.PerfTrace)
                _process.StartInfo.Arguments += " -perftrace";
            if (_options.TraceDC)
                _process.StartInfo.Arguments += " -tracedc";
            if (_options.Trace)
                _process.StartInfo.Arguments += " -trace";

            // Server frequency
            switch (_options.Frequency)
            {
                case Frequency.High60:
                    _process.StartInfo.Arguments += " -high60";
                    break;
                case Frequency.High120:
                    _process.StartInfo.Arguments += " -high120";
                    break;
            }

            _process.Start();

            _process.OutputDataReceived += Process_OutputDataReceived;
            _process.BeginOutputReadLine();

            // Start the refresh thread
            _refreshThread = new Thread(RefreshThreadWorker);
            _refreshThread.Name = "Server::UpdateThread";
            _refreshThread.IsBackground = true;
            _refreshThread.Start();
        }

        public void Stop()
        {
            if (_process == null || _process.HasExited)
                throw new InvalidOperationException("Server is not running");

            if (_rconClient.IsOpen)
                _rconClient.Close();

            _process.Kill();

            _process = null;
            _rconClient = null;
        }

        private void RefreshThreadWorker()
        {
            var lastTime = DateTime.Now;
            var lastProcessorTime = _process.TotalProcessorTime;

            while (_process != null && !_process.HasExited)
            {
                // Update the process
                _process.Refresh();

                // Update CPU usage
                var currentTime = DateTime.Now;
                var currentProcessorTime = _process.TotalProcessorTime;

                _cpuUsage = (float)((currentProcessorTime.TotalMilliseconds - lastProcessorTime.TotalMilliseconds) 
                    / currentTime.Subtract(lastTime).TotalMilliseconds 
                    / Environment.ProcessorCount) * 100f;

                Refreshed?.Invoke();

                Thread.Sleep(1000 / REFRESH_FREQUENCY);
            }

            // Reset vars
            Fps = 0;
            State = string.Empty;
            PlayerCount = 0;
            PlayerLimit = 0;
            Map = string.Empty;
            Mode = string.Empty;

            Refreshed?.Invoke();
        }

        public async Task<IList<string>> SendCommandAsync(IList<string> words)
        {
            if (_rconClient == null || !_rconClient.IsOpen)
                return null;

            return await _rconClient.SendMessageAsync(words);
        }

        private void LoadConfig()
        {
            _config = new Dictionary<string, string>();
            var lines = File.ReadAllLines(Path.Combine(_options.InstancePath, "Admin", "Startup.txt"));
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

        private void RconClient_WordsReceived(IList<string> words)
        {
            // Handle event based on command
            switch (words[0])
            {
                case "player.onJoin":
                    PlayerCount++;
                    break;

                case "player.onLeave":
                    PlayerCount--;
                    break;

                case "server.onMaxPlayerCountChange":
                    PlayerLimit = int.Parse(words[1]);
                    break;

                case "server.onLevelLoaded":
                    Map = words[1];
                    Mode = words[2];
                    break;

                default:
                    break;
            }

            // Pass event args
            DataReceived?.Invoke(words);
        }

        private async void Process_OutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            var line = args.Data;
            if (string.IsNullOrWhiteSpace(line))
                return;

            // Invoke logging event with the line
            LogOutput?.Invoke(line);

            // Check if the RCON client should be started by matching regex to see if RCON has started on the server
            if (Regex.Match(line, @"\[.+\] \[.+\] Remote Administration interface is listening on port 0.0.0.0:\d+").Success)
            {
                // Create connection to the RCON port on the running server so we can send commands to it from the inbuilt console
                _rconClient.Open(IPAddress.Loopback, _options.RemotePort);

                // Login with admin credentials
                await _rconClient.SendMessageAsync("login.plainText", _rconPassword);
            }
        }
    }
}
