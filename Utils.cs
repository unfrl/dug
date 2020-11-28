using System;

namespace dug.Services.Utils
{
    public static class DugConsole
    {
        public static void VerboseWrite(string content){
            if(Config.Verbose)
                Console.Write(content);
        }

        public static void VerboseWriteLine(string content){
            if(Config.Verbose)
                Console.WriteLine(content);
        }
    }
}