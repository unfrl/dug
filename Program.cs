using System;
using System.IO;
using System.Reflection;
using CommandLine;

namespace dug
{
    class Program
    {
        public class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }
        }
        
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       if (o.Verbose)
                       {
                           Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
                           Console.WriteLine("Quick Start Example! App is in Verbose mode!");
                       }
                       else
                       {
                           Console.WriteLine($"Current Arguments: -v {o.Verbose}");
                           Console.WriteLine("Quick Start Example!");
                       }
                   });
            
            var assembly = typeof(dug.Program).GetTypeInfo().Assembly;
            Stream resource = assembly.GetManifestResourceStream("dug.Resources.testfile.txt");
            using (var reader = new StreamReader(resource))
            {
                var resContent = reader.ReadLine();
                Console.WriteLine($"resContent: {resContent}");
            }

            Console.WriteLine("Home: " +Environment.GetEnvironmentVariable("HOME"));
        }
    }
}
