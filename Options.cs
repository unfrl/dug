using System;
using System.Collections.Generic;
using System.Net;
using CommandLine;
using CommandLine.Text;
using DnsClient;
using dug.Data;
using dug.Data.Models;

namespace dug.Options
{
    [Verb("run", isDefault: true, HelpText = "Get DNS propogation info for a URL")]
    public class RunOptions : GlobalOptions
    {
        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Default", new RunOptions { Hostname = "git.kaijucode.com"});
                yield return new Example("Specify query type(s)", new UnParserSettings(){ PreferShortName = true}, new RunOptions { Hostname = "git.kaijucode.com", QueryTypes= "A,MX" });
                yield return new Example("Specify continents to query on", new UnParserSettings(){ PreferShortName = true}, new RunOptions { Hostname = "git.kaijucode.com", Continents= "AF,NA,SA" });
                yield return new Example("Query the top 3 most reliable servers in Africa and North America as well as 8.8.8.8", new UnParserSettings(){ PreferShortName = true}, new RunOptions { Hostname = "git.kaijucode.com", Continents= "AF,NA", Servers = "8.8.8.8", ServerCount = 3, MultipleServerSources = true });
            }
        }

        [Option('t', "timeout", Required = false, HelpText = "The timeout (in ms) to be used when querying the DNS Server(s). If there are multiple it will apply to each server", Default = 3000)]
        public int Timeout { get; set; }

        [Option("server-count", Required = false, HelpText = "dug runs queries against the top servers, ranked by reliability, per continent. This allows you to set how many servers from each continent to use.", Default = 6)]
        public int ServerCount { get; set; }
        
        private string _continents;
        [Option("continents", Required = false, HelpText = "The continents on which servers will be queried. Defaults to all.", Default = "AF,SA,NA,OC,AS,EU,AN")]
        public string Continents { get {return _continents;}
            set{
                _continents = value;
                ParsedContinents = new List<ContinentCodes>();
                foreach(string continentString in Continents.Split(",")){
                    ContinentCodes parsedContinentCode;
                    if(ContinentCodes.TryParse(continentString, out parsedContinentCode)){
                        ParsedContinents.Add(parsedContinentCode);
                    }
                    else{
                        throw new Exception($"Invalid Continent value: {continentString}");
                    }
                }
            }
        }

        public List<ContinentCodes> ParsedContinents { get; set; }

        private string _queryTypes;
        [Option('q', "query-types", Required = false, HelpText = "The query type(s) to run against each server. Specify a single value (A) or multiple separated by commas (A,MX).", Default = "A")]
        public string QueryTypes { get{return _queryTypes;} 
            set{
                _queryTypes = value;
                ParsedQueryTypes = new List<QueryType>();
                foreach(string queryTypeString in QueryTypes.Split(",")){
                    QueryType parsedQueryType;
                    if(Enum.TryParse<QueryType>(queryTypeString, out parsedQueryType)){
                        ParsedQueryTypes.Add(parsedQueryType);
                    }
                    else{
                        throw new Exception($"Invalid Query Type value: {queryTypeString}");
                    }
                    
                }
            }
        }

        // NOTE: This is because of a really annoying issue that almost makes using IEnumerables with a separator as commandline options useless.
        // Any [Option] with an IEnumerable is very 'greedy' see: https://github.com/commandlineparser/commandline/issues/687
        // SUpposedly this will be fixed in version 2.9.0 but hasnt yet and this in on 2.9.0-preview1
        public List<QueryType> ParsedQueryTypes { get; set; }

        [Option('m', "multiple-sources", Required = false, HelpText = "When specifying servers (-s or --servers) also use integrated servers", Default = false)]
        public bool MultipleServerSources { get; set; }

        private string _servers;
        [Option('s', "servers", Required = false, HelpText = "The servers to query against instead of the integrated servers. Specify a single value (\"8.8.8.8\") or multiple separated by commas (\"8.8.8.8\",\"2001:4860:4860::8888\").")]
        public string Servers { get {return _servers;}
            set {
                _servers = value;
                ParsedServers = new List<DnsServer>();
                foreach(string addressString in Servers?.Split(",")){
                    IPAddress parsedAddress;
                    if(IPAddress.TryParse(addressString, out parsedAddress)){
                        ParsedServers.Add(new DnsServer() {IPAddress = parsedAddress});
                    }
                    else{
                        throw new Exception($"Unable to parse provided Server: {addressString}");
                    }
                }
            }
        }
        public List<DnsServer> ParsedServers { get; set; }

        [Option('f', "file", Required = false, HelpText = "Use the specified DNS server list for this run.")] //TODO: At some point we need a link here to a readme showing the format the file must be in.
        public string CustomServerFile { get; set; }

        [Value(0, Required = true, HelpText = "The Hostname you would like to see propogation for", MetaName = "Hostname")]
        public string Hostname { get; set; }
    }

    //This will likely be needed later
    // public class ParseableIPAddress : IPAddress
    // {
    //     public ParseableIPAddress(string address) : base(parseIPFromString(address))
    //     {
    //     }

    //     private static byte[] parseIPFromString(string address){
    //         IPAddress result;
    //         if(IPAddress.TryParse(address, out result)){
    //             return result.GetAddressBytes();
    //         }
    //         throw new ArgumentException($"Unable to parse provided IPAddress: {address}");
    //     }
    // }

    [Verb("update", HelpText = "Update DNS server list with any new unique servers. Uses remote server to get list by default.")]
    public class UpdateOptions : GlobalOptions
    {

        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Default", new UpdateOptions());
                yield return new Example("Overwrite current servers", new UnParserSettings(){ PreferShortName = true}, new UpdateOptions { Overwite = true });
            }
        }

        [Option('f', "file", Required = false, HelpText = "Update DNS server list using the specified file instead of the remote source")] //TODO: At some point we need a link here to a readme showing the format the file must be in.
        public string CustomServerFile { get; set; }

        [Option('o', "overwrite", Required = false, HelpText = "Overwrite the current server list instead of updating it.")]
        public bool Overwite { get; set; }

        [Option('r', "reliability", Required = false, HelpText = "Runs a query for a very stable domain (google.com) against ALL servers and updates server reliability based on the results")]
        public bool Reliability { get; set; }
    }


    

    public class GlobalOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Enable Verbose Output")]
        public bool Verbose { get; set; }

        [Option('p', "parallel", Required = false, HelpText = "The number of servers to perform queries against in parralel", Default = 100)]
        public int QueryParallelism { get; set; }

        [Option("retries", Required = false, HelpText = "The number of times to retry queries one servers that error (or timeout)", Default = 0)]
        public int QueryRetries { get; set; }
    }
}