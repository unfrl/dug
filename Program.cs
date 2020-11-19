using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        [Verb("run", isDefault: true)]
        public class RunOptions
        {
            [Value(0)]
            public string Url {get; set;}
        }

        [Verb("update", HelpText = "Update DNS server list. Uses remote server list by default.")]
        public class UpdateOptions
        {
            [Option('f', "file", Required = false, HelpText = "Update DNS server list using the specified file")] //TODO: At some point we need a link here to a readme showing the format the file must be in.
            public string CustomServerFile { get; set; }
        }

        public static async Task<int> Main(string[] args)
        {
            SetupDatabase();
            // Parse args and execute
            await Parser.Default.ParseArguments<RunOptions, UpdateOptions>(args)
                        .WithParsedAsync(ExecuteArguments);
            return 0;
        }

        private static async Task ExecuteArguments(object args)
        {
            switch(args){
                case UpdateOptions uo:
                    await ExecuteUpdate(uo);
                    break;
                case RunOptions ro:
                    // If there are no servers in the db populate it from the built in list. I do this after the update so i dont load them before then just have them updated right away.
                    // Theoretically the update command could be the first one they run :)
                    await EnsureServers();
                    ExecuteRun(ro);
                    break;
                default:
                    //TODO: Idk when itd get here...
                    break;
            }
        }

        private static int ExecuteRun(RunOptions opts)
        {
            Console.WriteLine("URL: "+opts.Url);
            return 0;
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
                    LoadServersFromStream(reader.BaseStream);
                }
                Console.WriteLine("Loaded DNS Servers from built-in source");
            }
        }

        private static void LoadServersFromStream(Stream stream)
        {
            //TODO: Ensure the headers are correct
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
            CsvParser<DnsServer> csvParser = new CsvParser<DnsServer>(csvParserOptions, new CsvDnsServerMapping());
            var parsedServers = csvParser.ReadFromStream(stream, Encoding.UTF8).Where(res => res.IsValid).Select(res => res.Result).ToList();
            using (var db = new DugContext())
            {
                var novelServers = parsedServers.Where(newServer => !db.DnsServers.Any(presentServer => presentServer.IPAddress != newServer.IPAddress));
                int novelServerCount = novelServers.Count();
                if(novelServerCount > 0){
                    db.DnsServers.AddRange(novelServers);
                    db.SaveChanges();
                }
                Console.WriteLine($"Added {novelServerCount} new DNS Servers");
            }
        }

        private static async Task ExecuteUpdate(UpdateOptions options)
        {
            if(!string.IsNullOrEmpty(options.CustomServerFile)){
                //Ensure the file exists
                var stream = File.OpenText(options.CustomServerFile);
                LoadServersFromStream(stream.BaseStream);
                return;
            }

            var serverInfoStream = await new HttpClient().GetStreamAsync("https://public-dns.info/nameservers.csv");
            LoadServersFromStream(serverInfoStream);
            
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
            }
        }
    }
}
