using CommandLine;
using System;
using System.IO;
using Terminal.Gui;

namespace VU.Server
{
    internal static class Program
    {
        private static ServerWindow _window;

        private static void SetupApplication(Options options)
        {
            // Get rid of that horrible blue background
            Application.Top.ColorScheme.Normal = new Terminal.Gui.Attribute(Color.White, Color.Black);

            // Set up process shutdown on application exit
            AppDomain.CurrentDomain.ProcessExit += OnApplicationExit;

            // Create interface
            _window = new ServerWindow(options);
            Application.Top.Add(_window);
        }

        private static void OnApplicationExit(object sender, EventArgs e)
        {
            // Trigger shutdown method in our application
            _window.Dispose();
        }

        private static void RunOptions(Options options)
        {
            // Check to see if vu.exe can be located at the specified vuPath
            if (!File.Exists(Path.Combine(options.VeniceUnleashedPath, "vu.exe")))
            {
                Console.WriteLine($"vu.exe was not found in the directory {options.VeniceUnleashedPath}");
                return;
            }

            // Check to see if bf3.exe can be located at the specified gamePath
            if (!string.IsNullOrWhiteSpace(options.GamePath) && !File.Exists(Path.Combine(options.GamePath, "bf3.exe")))
            {
                Console.WriteLine($"bf3.exe was not found in the directory {options.GamePath}");
                return;
            }

            // Initialize Terminal.Gui
            Application.Init();

            SetupApplication(options);

            // Run application
            Application.Run();
        }

        public static void Main(string[] args)
        {
            // Read command line options
            Parser.Default.ParseArguments<Options>(args)
               .WithParsed(RunOptions);
        }
    }
}
