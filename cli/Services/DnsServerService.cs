using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using dug.Data;
using dug.Data.Models;
using dug.Parsing;
using dug.Utils;
using TinyCsvParser.Mapping;
using TinyCsvParser.Model;

namespace dug.Services
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
                    newServers = LoadServersFromStream(reader.BaseStream, DnsServerParser.DefaultLocalParser, true, ',', false);
                    PersistServers();
                }
                DugConsole.WriteLine($"Loaded {newServers} DNS Servers from built-in source");
            }
            else {
                LoadServersFromDatastore();
            }            
        }

        // Returns the number of servers added
        // If overwrite is set the current server list is overwritten, instead of being updated with novel servers
        private int LoadServersFromStream(Stream stream, ICsvMapping<DnsServer> format, bool skipHeaders, char separator, bool overwrite)
        {
            var parsedServers = ParseServersFromStream(stream, format, skipHeaders, separator).ToList();

            return LoadServers(parsedServers, overwrite);
        }

        private int LoadServers(List<DnsServer> servers, bool overwrite = false){
            if(overwrite){
                Console.WriteLine($"Overwriting {_servers.Count()} with {servers.Count()} specified servers");

                _servers = servers;
                return servers.Count();
            }

            var novelServers = servers.Where(newServer => !_servers.Any(presentServer => presentServer.IPAddress.ToString() == newServer.IPAddress.ToString())).ToList();
            int novelServerCount = novelServers.Count();
            if(novelServerCount > 0){
                _servers.AddRange(novelServers);
            }
            return novelServerCount;
        }

        public void UpdateServers(List<DnsServer> servers, bool overwrite){
            int serversAdded = LoadServers(servers, overwrite);
            if(serversAdded == 0){
                Console.WriteLine($"Added {serversAdded} DNS Servers. The provided server(s) are already present.");
                return;
            }
            Console.WriteLine($"Added {serversAdded} DNS Servers.");
            PersistServers();
        }

        public List<DnsServer> ParseServersFromStream(Stream stream, ICsvMapping<DnsServer> format, bool skipHeaders, char separator){
            //This is almost always used in a 'using' context, so we dont want to return an IEnumerable where the actual enumeration would likely occur outside of that context
            return _serverParser.ParseServersFromStream(stream, format, skipHeaders, separator).Where(server => server.IPAddress != null).ToList();
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

        public void UpdateServersFromFile(string customFilePath, string customHeaders, char separator, bool skipHeaders, bool overwrite)
        {
            if(!File.Exists(customFilePath))
            {
                throw new FileNotFoundException($"Unable to find file at: {customFilePath}");
            }

            if(string.IsNullOrEmpty(customHeaders)){
                UpdateServersFromFileDefaultHeaders(customFilePath, overwrite);
                return;
            }

            var tokenizedLines = File
                .ReadLines(customFilePath, Encoding.UTF8)
                .Skip(skipHeaders ? 1 : 0)
                .Select((line, index) => new TokenizedRow(index, line.Split(separator, StringSplitOptions.None))); //Specifically DO NOT remove empty entries

            var customMapper = new CustomDnsServerMapping(customHeaders);
            var parsedServers = tokenizedLines.Select(line => customMapper.Map(line)).Where(res => res.IsValid).Select(res => res.Result);
            int serversAdded = LoadServers(parsedServers.ToList(), overwrite);

            Console.WriteLine($"Added {serversAdded} DNS Servers from {customFilePath}");
            PersistServers();
        }

        private void UpdateServersFromFileDefaultHeaders(string customFilePath, bool overwrite)
        {
            int serversAdded;
            using(var streamReader = File.OpenText(customFilePath)){
                serversAdded = LoadServersFromStream(streamReader.BaseStream, DnsServerParser.DefaultLocalParser, true, ',', overwrite);
            }
            Console.WriteLine($"Added {serversAdded} DNS Servers from {customFilePath}");
            PersistServers();
        }

        private void LoadServersFromDatastore(){
            int serversAdded;
            using(var streamReader = File.OpenText(Config.ServersFile)){
                serversAdded = LoadServersFromStream(streamReader.BaseStream, DnsServerParser.DefaultLocalParser, true, ',', false);
            }

            if(Config.Verbose)
                Console.WriteLine($"Loaded {serversAdded} DNS Servers from {Config.ServersFile}");
        }

        public async Task UpdateServersFromRemote(string url, char separator, string customHeaders, bool skipHeaders, bool overwrite)
        {
            var serverInfoStream = await new HttpClient().GetStreamAsync(url);
            int serversAdded = LoadServersFromStream(serverInfoStream, new CustomDnsServerMapping(customHeaders), skipHeaders, separator, overwrite);
            Console.WriteLine($"Retrieved {serversAdded} novel DNS Servers from {url}");
            PersistServers();
        }

        public async Task UpdateServersFromDefaultRemote(bool overwrite)
        {
            string remoteSourceURL = "https://public-dns.info/nameservers.csv";
            var serverInfoStream = await new HttpClient().GetStreamAsync(remoteSourceURL);
            int serversAdded = LoadServersFromStream(serverInfoStream, DnsServerParser.DefaultRemoteParser, true, ',', overwrite);
            Console.WriteLine($"Retrieved {serversAdded} novel DNS Servers from {remoteSourceURL}");
            PersistServers();
        }

        public void UpdateServerReliabilityFromResults(Dictionary<DnsServer, List<DnsResponse>> rawResults, bool prune,  double penalty = 0.1, double promotion = 0.01)
        {
            if(!prune){
                if(penalty < 0 || penalty > 1){
                    throw new ArgumentOutOfRangeException("penalty must be between 0 and 1");
                }
                if(promotion < 0 || promotion > 1){
                    throw new ArgumentOutOfRangeException("promotion must be between 0 and 1");
                }
            }
            foreach(var serverResults in rawResults){
                var server = serverResults.Key;
                var extantServer = _servers.Find(existingServer => existingServer.IPAddress.ToString() == server.IPAddress.ToString());
                if(extantServer == null){
                    continue;
                }
                foreach(var dnsResponse in serverResults.Value){
                    if(dnsResponse.HasError){
                        if(prune){
                            _servers.Remove(extantServer);
                            DugConsole.VerboseWriteLine($"PRUNING -- {extantServer.CityCountryContinentName} {extantServer.IPAddress} due to failed response");
                            continue;
                        }
                        extantServer.Reliability = extantServer.Reliability - penalty;
                    }
                    else if(dnsResponse.QueryResponse.Answers.Count() == 0){ //Dont do anything for servers with an empty response.
                        continue;
                    }
                    else{
                        extantServer.Reliability = extantServer.Reliability + promotion;
                    }
                    extantServer.Reliability = Math.Clamp(extantServer.Reliability, 0, 1);
                }
            }
            
            PersistServers();
        }
    }
}