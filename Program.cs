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
        [Verb("update", HelpText = "Update DNS server list. Uses remote server list by default.")]
        public class UpdateOptions
        {
            [Option('f', "file", Required = false, HelpText = "Update DNS server list using the specified file")] //TODO: At some point we need a link here to a readme showing the format the file must be in.
            public string CustomServerFile { get; set; }
        }
        
        static void Main(string[] args)
        {
            SetupDatabase();
            // Parsing args
            Parser.Default.ParseArguments<UpdateOptions>(args)
                   .WithParsed<UpdateOptions>(ExecuteUpdateActions);
            
            // If there are no servers in the db populate it from the built in list. I do this after the update so i dont load them before then just have them updated right away.
            // Theoretically the update command could be the first one they run :)


            // Reading embedded resources
            // var assembly = typeof(dug.Program).GetTypeInfo().Assembly;
            // Stream resource = assembly.GetManifestResourceStream("dug.Resources.default_servers.csv");
            // using (var reader = new StreamReader(resource))
            // {
            //     var resContent = reader.ReadLine();
            //     Console.WriteLine($"resContent: {resContent}");
            // }

            // Console.WriteLine("Home: " + Environment.GetEnvironmentVariable("HOME"));
        }

        private static void ExecuteUpdateActions(UpdateOptions options)
        {
            if(!string.IsNullOrEmpty(options.CustomServerFile)){
                throw new NotImplementedException("Havent made this feature yet ;)");
                //Do stuff, return
            }

            //Do the update
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
                // Console.WriteLine("Inserting a new server");
                // db.Add(new DnsServer { IPAddress = IPAddress.Parse("82.146.26.2") });
                // db.SaveChanges();
            }
        }
    }
}
