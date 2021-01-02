using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using Terminal.Gui;

namespace VU.Server
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            #region Argument Parsing

            // Get path to VU from arguments, along with the path to the server instance and Battlefield 3 installation
            string vuPath = string.Empty;
            string gamePath = string.Empty;
            string instancePath = string.Empty;
            string remoteAdminPort = string.Empty;

            var arguments = new List<string>();
            for (int i = 0; i < args.Length - 1; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-vupath":
                        vuPath = args[i + 1];
                        i++; // Skip next arg
                        break;
                    case "-gamepath":
                        gamePath = args[i + 1];
                        i++; // Skip next arg
                        break;
                    case "-serverinstancepath":
                        instancePath = args[i + 1];
                        i++; // Skip next arg
                        break;
                    case "-remoteadminport":
                        remoteAdminPort = args[i + 1];
                        i++; // Skip next arg
                        break;
                    default:
                        arguments.Add(args[i]);
                        break;
                }
            }

            // If path to VU was not provided, assume that this executable is placed in
            // the same directory as vu.exe
            if (string.IsNullOrWhiteSpace(vuPath))
            {
                string currentExePath = Assembly.GetExecutingAssembly().Location;
                string currentPath = Path.GetDirectoryName(currentExePath);

                // Update the VU path
                vuPath = currentPath;
            }

            // Check to see if vu.exe can be located at the specified vuPath
            if (!File.Exists(Path.Combine(vuPath, "vu.exe")))
            {
                Console.WriteLine($"vu.exe was not found in the directory {vuPath}");
                return;
            }

            // Check to see if bf3.exe can be located at the specified gamePath
            if (!string.IsNullOrWhiteSpace(gamePath) && !File.Exists(Path.Combine(gamePath, "bf3.exe")))
            {
                Console.WriteLine($"bf3.exe was not found in the directory {gamePath}");
                return;
            }

            // Get the RCON port from the provided string
            if (!IPEndPoint.TryParse(remoteAdminPort, out IPEndPoint remoteEndPoint))
                remoteEndPoint = new IPEndPoint(IPAddress.Loopback, 47200);

            // Read RCON configuration
            string configPath = Path.Combine(instancePath, "Admin", "Startup.txt");
            if (!File.Exists(configPath))
            {
                Console.WriteLine($"Startup.txt was not found at {configPath}");
                return;
            }

            #endregion

            // Create and run application
            Application.Init();

            // Get rid of that horrible blue background
            Application.Top.ColorScheme.Normal = new Terminal.Gui.Attribute(Color.White, Color.Black);

            var window = new ServerWindow(vuPath, gamePath, instancePath, configPath, remoteEndPoint.Port, arguments);
            Application.Top.Add(window);

            AppDomain.CurrentDomain.ProcessExit += delegate
            {
                // Dispose of the window and all child processes/handles
                window.Dispose();
            };

            Application.Run();
        }
    }
}
