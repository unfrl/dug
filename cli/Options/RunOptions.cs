using System;
using System.Collections.Generic;
using System.Net;
using CommandLine;
using CommandLine.Text;
using DnsClient;
using dug.Data;
using dug.Data.Models;
using dug.Utils;

namespace dug.Options
{
    public enum OutputFormats
    {
        TABLES,
        CSV,
        JSON
    }

    [Verb("run", isDefault: true, HelpText = "Get DNS propagation info for a URL")]
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
                yield return new Example("Output specific fields of results as json", new RunOptions { Hostname = "git.kaijucode.com", Template="ipaddress,city,responsetime,value", OutputFormat = OutputFormats.JSON});
            }
        }


        [Value(0, Required = true, HelpText = "The Hostname you would like to see propogation for", MetaName = "Hostname")]
        public string Hostname { get; set; }

        [Option('f', "file", Required = false, HelpText = "Use the specified DNS server list for this run.")] //TODO: At some point we need a link here to a readme showing the format the file must be in.
        public string CustomServerFile { get; set; }

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
                    if(Enum.TryParse<QueryType>(queryTypeString.ToUpperInvariant(), out parsedQueryType)){
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

        [Option('m', "multiple-sources", Required = false, HelpText = "When specifying servers (-s, --servers) or (-f, --file) also use integrated servers", Default = false)]
        public bool MultipleServerSources { get; set; }

        private string _dataColumns;
        [Option("data-columns", Required = false, HelpText = "Specify the fields, and their order, in the file specified with (-f). Must be used with (-f). Options: ipaddress,countrycode,city,dnssec,reliability,ignore")]
        public string DataColumns { get{return _dataColumns;}
            set
            {
                if(string.IsNullOrEmpty(CustomServerFile)){
                    throw new Exception("--data-columns cannot be used without specifying a server file (-f)");
                }
                var columns = value.ToLowerInvariant().Split(',', StringSplitOptions.None); //Specifically DO NOT remove empty entries
                foreach(var column in columns){
                    if(!TemplateHelper.ServerSetterMap.ContainsKey(column)){
                        throw new Exception($"Unable to parse provided column header: {column}");
                    }
                }
                _dataColumns = value.ToLowerInvariant();
            }
        }

        private bool _dataHeadersPresent;
        [Option("data-headers-present", Required = false, HelpText = "Specifies whether or not headers are present in the file specified with (-f). Can only be used in conjuction with --data-columns")]
        public bool DataHeadersPresent { get{return _dataHeadersPresent;}
            set
            {
                if(string.IsNullOrEmpty(DataColumns)){
                    throw new Exception("--data-headers-present cannot be used without (--data-columns)");
                }
                _dataHeadersPresent = value;
            }
        }

        private char? _dataSeparator;
        [Option("data-separator", Required = false, HelpText = "Specifies the separator to be used when parsing servers from the file specified with (-f). Can only be used in conjuction with --data-columns. Assumes ',' if not set.")]
        public char? DataSeparator { get{return _dataSeparator;}
            set
            {
                if(string.IsNullOrEmpty(DataColumns)){
                    throw new Exception("--data-separator cannot be used without (--data-columns)");
                }
                _dataSeparator = value;
            }
        }

        private string _template;
        [Option("output-template", Required = false, HelpText = "Specify which data, and in what order, to put into out. Ignored if --output-format=TABLES. Options: ipaddress,countrycode,city,dnssec,reliability,continentcode,countryname,countryflag,citycountryname,citycountrycontinentname,responsetime,recordtype,haserror,errormessage,errorcode,value")]
        public string Template { get{return _template;}
            set
            {
                var headers = value.ToLowerInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries); //Specifically DO NOT remove empty entries
                foreach(var header in headers){
                    if(!TemplateHelper.ResponseGetterMap.ContainsKey(header)){
                        throw new Exception($"Unable to parse provided output-template header: {header}");
                    }
                }
                _template = value.ToLowerInvariant();
            }
        }

        private OutputFormats _outputFormat;
        [Option("output-format", Required = false, Default = OutputFormats.TABLES, HelpText = "Specify the output format. For formats other than the default you must also specify a template (--output-template). Options: CSV,JSON")]
        public OutputFormats OutputFormat { get{return _outputFormat;}
            set
            {
                if(value == OutputFormats.TABLES){
                    _outputFormat = value;
                    return;
                }

                if(string.IsNullOrEmpty(Template)){
                    throw new Exception($"A template (--output-template) is required when using an output-format other than the default ({default(OutputFormats)})");
                }
                _outputFormat = value;
            }
        }

        private int _tableDetailLevel;
        [Option('d', "table-detail", Default = 1, HelpText = "Specify the level of detail to show when rendering tables. Ignored if output-format is set to anything other than its default (TABLES)")]
        public int TableDetailLevel { get{return _tableDetailLevel;}
             set
             {
                 if(value < 1 || value > 2) //Currently only support 1 and 2
                 {
                    throw new Exception("The specified table-detail is out of range. Valid values are in the range [1-2]");
                 }
                 _tableDetailLevel = value;
             }
        }

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
}