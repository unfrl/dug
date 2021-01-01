using System;
using System.Collections.Generic;
using System.Net;
using CommandLine;
using CommandLine.Text;
using dug.Data.Models;

namespace dug.Options
{
    public enum ReliabilityUpdateType
    {
        Normal,
        Prune
    }

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
                yield return new Example("Import servers", new UnParserSettings(){ PreferShortName = true}, new UpdateOptions { Servers = "8.8.8.8,2001:4860:4860::8888" });
                yield return new Example("Import and overwrite servers", new UnParserSettings(){ PreferShortName = true}, new UpdateOptions { Servers = "8.8.8.8,2001:4860:4860::8888", Overwite = true });
                yield return new Example("Import servers from remote source, then update all servers' reliability", new UnParserSettings(){ PreferShortName = true}, new UpdateOptions { Reliability = ReliabilityUpdateType.Normal });
                yield return new Example("Import servers from remote source, then update all servers' reliability. Remove servers that fail", new UnParserSettings(){ PreferShortName = true}, new UpdateOptions { Reliability = ReliabilityUpdateType.Prune });
                yield return new Example("Update server reliability, remove any that fail. Also use up to 2 retries when querying each server", new UnParserSettings(){ PreferShortName = true}, new UpdateOptions { Reliability = ReliabilityUpdateType.Prune, ReliabilityOnly = true, QueryRetries = 2 });
            }
        }

        [Option('f', "file", Required = false, HelpText = "Update DNS server list using the specified file instead of the remote source")] //TODO: At some point we need a link here to a readme showing the format the file must be in.
        public string CustomServerFile { get; set; }

        [Option('o', "overwrite", Required = false, HelpText = "Overwrite the current server list instead of updating it.")]
        public bool Overwite { get; set; }

        [Option('r', "reliability", Required = false, HelpText = "Runs a query for a very stable domain (google.com) against ALL servers. Can be set to 'normal' or 'prune'. Normal updates server reliability based on the results, Prune removes servers that failed (timeout, error, etc)")]
        public ReliabilityUpdateType? Reliability { get; set; }

        private bool _reliabilityOnly;
        [Option("reliability-only", Required = false, HelpText = "Can only be used with the (-r,--reliability) option. This will keep any new servers from other options (-s,-f,etc) or remote sources from being added. Use if you only want to update the reliability of the currently present servers")]
        public bool ReliabilityOnly {get{return _reliabilityOnly;}
            set
            {
                if(Reliability == null){
                    throw new Exception("--reliability-only cannot be used without (-r,--reliability)");
                }
                _reliabilityOnly = value;
            }
        }

        private string _servers;
        [Option('s', "servers", Required = false, HelpText = "The server IPs to import instead of the remote source. Specify a single value (\"8.8.8.8\") or multiple separated by commas (\"8.8.8.8\",\"2001:4860:4860::8888\").")]
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
    }
}