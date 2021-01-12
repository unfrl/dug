using System;
using System.IO;

namespace dug
{
    public static class Config {
        public static string ConfigDirectory = Path.Join(getConfigBaseDirectory(), ".dug");
        public static string ServersFile = Path.Join(ConfigDirectory, "servers.csv");
        public static string ServersTempFile = Path.Join(ConfigDirectory, "servers.tmp.csv");
        public static bool Verbose { get; set; }
        //This value is used so that we avoid writing to the console when the output is templated, in which can we dont want random messages polluting it.
        public static bool CanWrite { get; set; } = true;

        // Returns the User's home directory, platform agnostic.
        private static string getConfigBaseDirectory(){
            return Environment.OSVersion.Platform == PlatformID.Unix ?
            Environment.GetEnvironmentVariable("HOME") :
            Environment.GetEnvironmentVariable("%HOMEDRIVE%%HOMEPATH%");
        }
    }
}