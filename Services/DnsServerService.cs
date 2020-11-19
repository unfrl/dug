using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using dug.Data.Models;
using dug.Parsing;

namespace dug
{
    public class DnsServerService : IDnsServerService
    {
        private List<DnsServer> _servers = new List<DnsServer>();

        private IDnsServerParser _serverParser;

        public DnsServerService(IDnsServerParser serverParser){
            _serverParser = serverParser;
        }

        public List<DnsServer> GetServers(){
            return _servers;
        }

        // Loads the default DNS Servers if there are no servers in the database
        public void EnsureServers()
        {
            LoadServersFromDatastore();
            if (_servers.Any())
            {
                //TODO: If there is a verbose output maybe make it say something like "servers already present, no need to use internal source"
                return;
            }

            var assembly = typeof(dug.Program).GetTypeInfo().Assembly;
            Stream resource = assembly.GetManifestResourceStream("dug.Resources.default_servers.csv");
            int newServers = 0;
            using (var reader = new StreamReader(resource))
            {
                var firstline = reader.ReadLine();
                newServers = LoadServersFromStream(reader.BaseStream, DnsServerCsvFormats.Local, true);
            }
            Console.WriteLine($"Loaded {newServers} DNS Servers from built-in source");
        }

        //Returns the number of servers added
        private int LoadServersFromStream(Stream stream, DnsServerCsvFormats format, bool updateFile = false)
        {
            //TODO: Ensure the headers are correct
            var parsedServers = _serverParser.ParseServersFromStream(stream, format).ToList();
            var novelServers = parsedServers.Where(newServer => !_servers.Any(presentServer => presentServer.IPAddress.ToString() == newServer.IPAddress.ToString())).ToList();
            int novelServerCount = novelServers.Count();
            if(novelServerCount > 0){
                _servers.AddRange(novelServers);
                if(updateFile){
                    //TODO: Update server.csv atomically
                }
            }
            return novelServerCount;
        }

        public void UpdateServersFromFile(string customFilePath)
        {
            //TODO: Ensure the file exists
            if(!File.Exists(customFilePath))
            {
                throw new FileNotFoundException($"Unable to find file at: {customFilePath}");
            }
            var stream = File.OpenText(customFilePath);
            int serversAdded = LoadServersFromStream(stream.BaseStream, DnsServerCsvFormats.Local, true);
            Console.WriteLine($"Added {serversAdded} DNS Servers from {customFilePath}");
        }

        private void LoadServersFromDatastore(){
            var stream = File.OpenText(Config.ServersFile);
            int serversAdded = LoadServersFromStream(stream.BaseStream, DnsServerCsvFormats.Local);
            Console.WriteLine($"Loaded {serversAdded} DNS Servers from {Config.ServersFile}");
        }

        public async Task UpdateServersFromRemote()
        {
            var serverInfoStream = await new HttpClient().GetStreamAsync("https://public-dns.info/nameservers.csv");
            int serversAdded = LoadServersFromStream(serverInfoStream, DnsServerCsvFormats.Remote, true);
            Console.WriteLine($"Added {serversAdded} DNS Servers from remote source");
        }
    }
}