using CommandLine;

namespace VU.Server
{
    internal sealed class Options
    {
        [Option("path", Required = true, HelpText = "Path to Venice Unleashed")]
        public string VeniceUnleashedPath { get; set; }


        [Option("instance", Required = true, HelpText = "Path to VU server instance")]
        public string InstancePath { get; set; }
        [Option("gamepath", Required = false, HelpText = "Path to Battlefield 3 game files")]
        public string GamePath { get; set; }

        [Option("gameport", Required = false, Default = 25200, HelpText = "Set VU server game port")]
        public int GamePort { get; set; }
        [Option("harmonyport", Required = false, Default = 7948, HelpText = "Set VU server harmony port")]
        public int HarmonyPort { get; set; }
        [Option("remoteport", Required = false, Default = 47200, HelpText = "Set VU server remote port")]
        public int RemotePort { get; set; }

        [Option("frequency", Required = false, Default = Frequency.Default30, HelpText = "Set VU server frequency")]
        public Frequency Frequency { get; set; }

        [Option("unlisted", Required = false, Default = false, HelpText = "Prevent VU server from being visible on the server list")]
        public bool Unlisted { get; set; }
        [Option("noupdate", Required = false, Default = false, HelpText = "Prevent automatic updates from restarting the server")]
        public bool NoUpdate { get; set; }

        [Option("highresterrain", Required = false, Default = false, HelpText = "Enables high resolution terrain. Useful for extending maps beyond their original play area")]
        public bool HighResTerrain { get; set; }
        [Option("disableterraininterop", Required = false, Default = false, HelpText = "Disables interpolation between different terrain LODs")]
        public bool DisableTerrainInterop { get; set; }
        [Option("skipchecksum", Required = false, Default = false, HelpText = "Disables level checksum validation on client connection")]
        public bool SkipChecksum { get; set; }

        [Option("perftrace", Required = false, Default = false, HelpText = "Writes a performance profile to perftrace-server.csv")]
        public bool PerfTrace { get; set; }
        [Option("env", Required = false, Default = "prod", HelpText = "Specifies the Zeus environment to connect to. Defaults to prod")]
        public string Environment { get; set; }
        [Option("tracedc", Required = false, Default = false, HelpText = "Traces DataContainer usage in VEXT and prints any dangling DCs during level destruction")]
        public bool TraceDC { get; set; }
        [Option("trace", Required = false, Default = false, HelpText = "Enables verbose logging")]
        public bool Trace { get; set; }

        /*
        [Option('p', "processor", Required = false, Default = 0, HelpText = "Set Processor to run VU server instance on")]
        public int Processor { get; set; }

        [Option('c', "core", Required = false, Default = 0, HelpText = "Set Processor Core to run VU server instance on")]
        public int Core { get; set; }


        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages")]
        public bool Verbose { get; set; }
        */
    }
}
