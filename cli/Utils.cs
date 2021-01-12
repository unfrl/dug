using System;

namespace dug.Utils
{
    public static class DugConsole
    {
        public static void Write(string content){
            if(Config.CanWrite)
                Console.Write(content);
        }

        public static void WriteLine(string content){
            Write(content + Environment.NewLine);
        }

        public static void VerboseWrite(string content){
            if(Config.Verbose)
                Write(content);
        }

        public static void VerboseWriteLine(string content){
                VerboseWrite(content + Environment.NewLine);
        }
    }
}