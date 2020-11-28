using System;
using System.IO;

namespace dug.Services
{
    public static class Config {
        public static string ConfigDirectory = Path.Join(getConfigBaseDirectory(), ".dug");
        public static string ServersFile = Path.Join(ConfigDirectory, "servers.csv");
        public static string ServersTempFile = Path.Join(ConfigDirectory, "servers.tmp.csv");

        public static bool Verbose { get; set; }

        // Returns the User's home directory, platform agnostic.
        private static string getConfigBaseDirectory(){
            return Environment.OSVersion.Platform == PlatformID.Unix ?
            Environment.GetEnvironmentVariable("HOME") :
            Environment.GetEnvironmentVariable("%HOMEDRIVE%%HOMEPATH%");
        }
    }
}