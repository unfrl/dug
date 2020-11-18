using System;
using System.IO;
using System.Net;
using System.Reflection;
using CommandLine;
using dug.Data;
using dug.Data.Models;
using Microsoft.EntityFrameworkCore;

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
            // Parsing args
            // Parser.Default.ParseArguments<Options>(args)
            //        .WithParsed<Options>(o =>
            //        {
            //            if (o.Verbose)
            //            {
            //                Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
            //                Console.WriteLine("Quick Start Example! App is in Verbose mode!");
            //            }
            //            else
            //            {
            //                Console.WriteLine($"Current Arguments: -v {o.Verbose}");
            //                Console.WriteLine("Quick Start Example!");
            //            }
            //        });
            
            // Reading embedded resources
            // var assembly = typeof(dug.Program).GetTypeInfo().Assembly;
            // Stream resource = assembly.GetManifestResourceStream("dug.Resources.default_servers.csv");
            // using (var reader = new StreamReader(resource))
            // {
            //     var resContent = reader.ReadLine();
            //     Console.WriteLine($"resContent: {resContent}");
            // }

            // Console.WriteLine("Home: " + Environment.GetEnvironmentVariable("HOME"));
            SetupDatabase();
        }

        private static void SetupDatabase(){
            if(!File.Exists(Config.SqliteFile)){
                Directory.CreateDirectory(Config.ConfigDirectory);
                File.Create(Config.SqliteFile);
            }
            using (var db = new DugContext())
            {
                db.Database.Migrate();
                // Create
                Console.WriteLine("Inserting a new server");
                db.Add(new DnsServer { IPAddress = IPAddress.Parse("82.146.26.2") });
                db.SaveChanges();
            }
        }
    }
}
