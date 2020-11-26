using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using dug.Data.Models;
using dug.Parsing;

namespace dug
{
    public class DnsServerService : IDnsServerService
    {
        private List<DnsServer> _servers = new List<DnsServer>();

        private IDnsServerParser _serverParser;

        public List<DnsServer> Servers => _servers;

        public ILookup<ContinentCodes, DnsServer> ServersByContinent => Servers.ToLookup(server => server.ContinentCode, new ContinentCodeComparer());

        public DnsServerService(IDnsServerParser serverParser){
            _serverParser = serverParser;
        }

        // Loads the default DNS Servers if there are no servers in the database
        public void EnsureServers()
        {
            if(!File.Exists(Config.ServersFile)){
                Directory.CreateDirectory(Config.ConfigDirectory);
                var assembly = typeof(dug.Program).GetTypeInfo().Assembly;
                Stream resource = assembly.GetManifestResourceStream("dug.Resources.default_servers.csv");
                int newServers = 0;
                using (var reader = new StreamReader(resource))
                {
                    newServers = LoadServersFromStream(reader.BaseStream, DnsServerCsvFormats.Local);
                    PersistServers();
                }
                Console.WriteLine($"Loaded {newServers} DNS Servers from built-in source");
            }
            else {
                LoadServersFromDatastore();
            }            
        }

        // Returns the number of servers added
        // If overwrite is set the current server list is overwritten, instead of being updated with novel servers
        private int LoadServersFromStream(Stream stream, DnsServerCsvFormats format, bool overwrite = false)
        {
            //TODO: Ensure the headers are correct
            var parsedServers = _serverParser.ParseServersFromStream(stream, format).Where(server => !string.IsNullOrEmpty(server.CountryCode)).ToList();

            if(overwrite){
                if(Config.Verbose)
                    Console.WriteLine($"Overwriting {_servers.Count()} with {parsedServers.Count()} specified servers");

                _servers = parsedServers;
                return parsedServers.Count();
            }

            var novelServers = parsedServers.Where(newServer => !_servers.Any(presentServer => presentServer.IPAddress.ToString() == newServer.IPAddress.ToString())).ToList();
            int novelServerCount = novelServers.Count();
            if(novelServerCount > 0){
                _servers.AddRange(novelServers);
            }
            return novelServerCount;
        }

        private void PersistServers(){
            StringBuilder builder = new StringBuilder("ip_address,country_code,city,dnssec,reliability\n"); //The first line should be the headers
            foreach(DnsServer server in _servers){
                builder.AppendLine(server.ToCsvString());
            }
            if(!File.Exists(Config.ServersFile)){
                File.WriteAllText(Config.ServersFile, builder.ToString());
                return;
            }
            
            File.WriteAllText(Config.ServersTempFile, builder.ToString());
            File.Replace(Config.ServersTempFile, Config.ServersFile, null);
        }

        public void UpdateServersFromFile(string customFilePath, bool overwrite)
        {
            //TODO: Ensure the file exists
            if(!File.Exists(customFilePath))
            {
                throw new FileNotFoundException($"Unable to find file at: {customFilePath}");
            }
            int serversAdded;
            using(var streamReader = File.OpenText(customFilePath)){
                serversAdded = LoadServersFromStream(streamReader.BaseStream, DnsServerCsvFormats.Local, overwrite);
            }
            Console.WriteLine($"Retrieved {serversAdded} DNS Servers from {customFilePath}");
            PersistServers();
        }

        private void LoadServersFromDatastore(){
            int serversAdded;
            using(var streamReader = File.OpenText(Config.ServersFile)){
                serversAdded = LoadServersFromStream(streamReader.BaseStream, DnsServerCsvFormats.Local);
            }

            if(Config.Verbose)
                Console.WriteLine($"Loaded {serversAdded} DNS Servers from {Config.ServersFile}");
        }

        public async Task UpdateServersFromRemote(bool overwrite)
        {
            string remoteSourceURL = "https://public-dns.info/nameservers.csv";
            var serverInfoStream = await new HttpClient().GetStreamAsync(remoteSourceURL);
            int serversAdded = LoadServersFromStream(serverInfoStream, DnsServerCsvFormats.Remote, overwrite);
            Console.WriteLine($"Retrieved {serversAdded} DNS Servers from {remoteSourceURL}");
            PersistServers();
        }
    }
}