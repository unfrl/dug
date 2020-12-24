using System;
using System.Collections.Generic;
using System.Net;
using CommandLine;
using CommandLine.Text;
using DnsClient;

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
                yield return new Example("Default", new RunOptions { Url = "git.kaijucode.com"});
                yield return new Example("Specify query type(s)", new UnParserSettings(){ PreferShortName = true}, new RunOptions { Url = "git.kaijucode.com", QueryTypes= "A,MX" });
            }
        }

        [Option('t', "timeout", Required = false, HelpText = "The timeout (in ms) to be used when querying the DNS Server(s). If there are multiple it will apply to each server", Default = 3000)]
        public int Timeout { get; set; }

        [Option('q', "query-types", Required = false, HelpText = "The query type(s) to run against each server. Specify a single value (A) or multiple separated by commas (A,MX).", Default = "A")]
        public string QueryTypes { get; set; }

        // [Option('q', "query-types", Required = false, HelpText = "The query type(s) to run against each server. Specify a single value (\"A\") or multiple separated by commas (\"A,MX\").", Separator = ',', Default = new [] { QueryType.A })]
        // public IEnumerable<QueryType> QueryTypes { get; set; }

        private List<QueryType> _parsedQueryTypes;
        // NOTE: This is because of a really annoying issue that almost makes using IEnumerables with a separator as commandline options useless.
        // Any [Option] with an IEnumerable is very 'greedy' see: https://github.com/commandlineparser/commandline/issues/687
        // SUpposedly this will be fixed in version 2.9.0 but hasnt yet and this in on 2.9.0-preview1
        public IEnumerable<QueryType> ParsedQueryTypes { 
            get {
                if(_parsedQueryTypes != null){
                    return _parsedQueryTypes;
                }
                _parsedQueryTypes = new List<QueryType>();
                foreach(string qt in QueryTypes.Split(",")){
                    QueryType parsedQueryType;
                    if(Enum.TryParse<QueryType>(qt, out parsedQueryType)){
                        _parsedQueryTypes.Add(parsedQueryType);
                    }
                }
                return _parsedQueryTypes;
            }
        }

        [Option('s', "servers", Separator = ',', HelpText = "TODO: NOT IMPLEMENTED")]
        public IEnumerable<ParseableIPAddress> Servers { get; set; }

        [Option('f', "file", Required = false, HelpText = "Use the specified DNS server list for this run.")] //TODO: At some point we need a link here to a readme showing the format the file must be in.
        public string CustomServerFile { get; set; }

        [Value(0, Required = true, HelpText = "The URL you would like to see propogation for", MetaName = "URL")]
        public string Url { get; set; }
    }

    public class ParseableIPAddress : IPAddress
    {
        public ParseableIPAddress(string address) : base(parseIPFromString(address))
        {
        }

        private static byte[] parseIPFromString(string address){
            IPAddress result;
            if(IPAddress.TryParse(address, out result)){
                return result.GetAddressBytes();
            }
            throw new ArgumentException($"Unable to parse provided IPAddress: {address}");
        }

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
            }
        }

        [Option('f', "file", Required = false, HelpText = "Update DNS server list using the specified file.")] //TODO: At some point we need a link here to a readme showing the format the file must be in.
        public string CustomServerFile { get; set; }

        [Option('o', "overwrite", Required = false, HelpText = "Overwrite the current server list instead of updating it.")]
        public bool Overwite { get; set; }
    }

    public class GlobalOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Enable Verbose Output")]
        public bool Verbose { get; set; }
    }
}