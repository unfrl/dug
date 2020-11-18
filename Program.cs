using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using dug.Data;
using dug.Data.Models;
using Microsoft.EntityFrameworkCore;
using TinyCsvParser;

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

        public static async Task<int> Main(string[] args)
        {
            SetupDatabase();
            // Parsing args
            await Parser.Default.ParseArguments<UpdateOptions>(args)
                   .WithParsedAsync<UpdateOptions>(ExecuteUpdateActions);
            
            // If there are no servers in the db populate it from the built in list. I do this after the update so i dont load them before then just have them updated right away.
            // Theoretically the update command could be the first one they run :)
            await EnsureServers();
            return 1;
        }

        // Loads the default DNS Servers if there are no servers in the database
        private static async Task EnsureServers(){
            bool serversPresent;
            using (var db = new DugContext())
            {
                serversPresent = await db.DnsServers.AnyAsync();
            }

            if(!serversPresent){
                var assembly = typeof(dug.Program).GetTypeInfo().Assembly;
                Stream resource = assembly.GetManifestResourceStream("dug.Resources.default_servers.csv");
                using (var reader = new StreamReader(resource))
                {
                    await LoadServers(reader);
                }
            }
        }

        private static async Task LoadServers(StreamReader reader)
        {
            //TODO: Ensure the headers are correct
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
            CsvParser<DnsServer> csvParser = new CsvParser<DnsServer>(csvParserOptions, new CsvDnsServerMapping());
            var result = csvParser.ReadFromStream(reader.BaseStream, Encoding.UTF8).Where(res => res.IsValid).Select(res => res.Result).ToList();
            using (var db = new DugContext())
            {
                await db.DnsServers.AddRangeAsync(result);
                db.SaveChanges();
            }
            Console.WriteLine("Loaded DNS Servers");
        }

        private static async Task ExecuteUpdateActions(UpdateOptions options)
        {
            if(!string.IsNullOrEmpty(options.CustomServerFile)){
                //Ensure the file exists
                var stream = File.OpenText(options.CustomServerFile);
                await LoadServers(stream);
                return;
            }

            throw new NotImplementedException("Havent implemented updating from the remote source yet ;)");
            //Do the update from the remote source
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
