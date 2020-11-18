using System;
using System.IO;

namespace dug
{
    public static class Config {
        public static string ConfigDirectory = Path.Join(getConfigBaseDirectory(), ".dug");
        public static string SqliteFile = Path.Join(ConfigDirectory, "dug.db");

        // Returns the User's home directory, platform agnostic.
        private static string getConfigBaseDirectory(){
            return Environment.OSVersion.Platform == PlatformID.Unix ?
            Environment.GetEnvironmentVariable("HOME") :
            Environment.GetEnvironmentVariable("%HOMEDRIVE%%HOMEPATH%");
        }
    }
}