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

    [Verb("run", isDefault: true, HelpText = "HT_Run", ResourceType = typeof(i18n.dug))]
    public class RunOptions : GlobalOptions
    {
        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example(i18n.dug.Default, new RunOptions { Hostname = "git.kaijucode.com"});
                yield return new Example(i18n.dug.EX_Specify_Query_Types, new UnParserSettings(){ PreferShortName = true}, new RunOptions { Hostname = "git.kaijucode.com", QueryTypes= "A,MX" });
                yield return new Example(i18n.dug.EX_Specify_Continents, new UnParserSettings(){ PreferShortName = true}, new RunOptions { Hostname = "git.kaijucode.com", Continents= "AF,NA,SA" });
                yield return new Example(i18n.dug.EX_Query_Reliable_On_Continents, new UnParserSettings(){ PreferShortName = true}, new RunOptions { Hostname = "git.kaijucode.com", Continents= "AF,NA", Servers = "8.8.8.8", ServerCount = 3, MultipleServerSources = true });
                yield return new Example(i18n.dug.EX_Json_Fields_Output, new RunOptions { Hostname = "git.kaijucode.com", Template="ipaddress,city,responsetime,value", OutputFormat = OutputFormats.JSON});
            }
        }


        [Value(0, Required = true, HelpText = "HT_Run_Hostname", ResourceType = typeof(i18n.dug), MetaName = "Hostname")]
        public string Hostname { get; set; }

        private int? _watch;
        [Option('w', "watch", Required = false, HelpText = "HT_Run_Watch", ResourceType = typeof(i18n.dug))]
        public int? Watch { get {return _watch;}
            set {
                if(value.HasValue && value.Value < 0){
                    throw new ArgumentOutOfRangeException(nameof(RunOptions.Watch), i18n.dug.ER_Run_Watch_Out_Of_Range);
                }
                
                _watch = value;
            }
        }

        [Option('f', "file", Required = false, HelpText = "HT_Run_Custom_Server_File", ResourceType = typeof(i18n.dug))] //TODO: At some point we need a link here to a readme showing the format the file must be in.
        public string CustomServerFile { get; set; }

        private string _servers;
        [Option('s', "servers", Required = false, HelpText = "HT_Run_Servers", ResourceType = typeof(i18n.dug))]
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
                        throw new Exception($"{i18n.dug.ER_Run_Server_Parse} {addressString}");
                    }
                }
            }
        }
        public List<DnsServer> ParsedServers { get; set; }

        private int _serverCount;
        [Option("server-count", Required = false, HelpText = "HT_Run_Server_Count", ResourceType = typeof(i18n.dug), Default = 6)]
        public int ServerCount { get {return _serverCount;}
            set{
                if(value < 1){
                    throw new Exception(i18n.dug.ER_Server_Count_Out_Of_Range);
                }
                _serverCount = value;
            }
        }
        
        private string _continents;
        [Option("continents", Required = false, HelpText = "HT_Run_Continents", ResourceType = typeof(i18n.dug), Default = "AF,SA,NA,OC,AS,EU,AN")]
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
                        throw new Exception($"{i18n.dug.ER_Run_Inavlid_Continent} {continentString}");
                    }
                }
            }
        }

        public List<ContinentCodes> ParsedContinents { get; set; }

        private string _queryTypes;
        [Option('q', "query-types", Required = false, HelpText = "HT_Run_Query_Types", ResourceType = typeof(i18n.dug), Default = "A")]
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
                        throw new Exception($"{i18n.dug.ER_Run_Invalid_Query_Type} {queryTypeString}");
                    }
                    
                }
            }
        }
        // NOTE: This is because of a really annoying issue that almost makes using IEnumerables with a separator as commandline options useless.
        // Any [Option] with an IEnumerable is very 'greedy' see: https://github.com/commandlineparser/commandline/issues/687
        // Supposedly this will be fixed in version 2.9.0 but hasnt yet and this is on 2.9.0-preview1
        public List<QueryType> ParsedQueryTypes { get; set; }

        [Option('m', "multiple-sources", Required = false, HelpText = "HT_Run_Multiple_Server_Sources", ResourceType = typeof(i18n.dug), Default = false)]
        public bool MultipleServerSources { get; set; }

        private string _dataColumns;
        [Option("data-columns", Required = false, HelpText = "HT_Run_Data_Columns", ResourceType = typeof(i18n.dug))]
        public string DataColumns { get{return _dataColumns;}
            set
            {
                if(string.IsNullOrEmpty(CustomServerFile)){
                    throw new Exception(i18n.dug.ER_Run_Data_Columns_Without_Server_File);
                }
                var columns = value.ToLowerInvariant().Split(',', StringSplitOptions.None); //Specifically DO NOT remove empty entries
                foreach(var column in columns){
                    if(!TemplateHelper.ServerSetterMap.ContainsKey(column)){
                        throw new Exception($"{i18n.dug.ER_Run_Unable_Parse_Data_Column_Header} {column}");
                    }
                }
                _dataColumns = value.ToLowerInvariant();
            }
        }

        private bool _dataHeadersPresent;
        [Option("data-headers-present", Required = false, HelpText = "HT_Run_Data_Headers_Present", ResourceType = typeof(i18n.dug))]
        public bool DataHeadersPresent { get{return _dataHeadersPresent;}
            set
            {
                if(string.IsNullOrEmpty(DataColumns)){
                    throw new Exception(i18n.dug.ER_Run_Data_Headers_Present_Requires_Data_Columns);
                }
                _dataHeadersPresent = value;
            }
        }

        private char? _dataSeparator;
        [Option("data-separator", Required = false, HelpText = "HT_Run_Data_Separator", ResourceType = typeof(i18n.dug))]
        public char? DataSeparator { get{return _dataSeparator;}
            set
            {
                if(string.IsNullOrEmpty(DataColumns)){
                    throw new Exception(i18n.dug.ER_Run_Data_Separator_Requires_Data_Columns);
                }
                _dataSeparator = value;
            }
        }

        private string _template;
        [Option("output-template", Required = false, HelpText = "HT_Run_Template", ResourceType = typeof(i18n.dug))]
        public string Template { get{return _template;}
            set
            {
                var headers = value.ToLowerInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries); //Specifically DO NOT remove empty entries
                foreach(var header in headers){
                    if(!TemplateHelper.ResponseGetterMap.ContainsKey(header)){
                        throw new Exception($"{i18n.dug.ER_Run_Unable_Parse_Template_Header} {header}");
                    }
                }
                _template = value.ToLowerInvariant();
            }
        }

        private OutputFormats _outputFormat;
        [Option("output-format", Required = false, Default = OutputFormats.TABLES, HelpText = "HT_Run_Output_Format", ResourceType = typeof(i18n.dug))]
        public OutputFormats OutputFormat { get{return _outputFormat;}
            set
            {
                if(value == OutputFormats.TABLES){
                    _outputFormat = value;
                    return;
                }

                if(Watch.HasValue){
                    throw new Exception(i18n.dug.ER_Run_Output_Format_Cannot_Be_Used_With_Watch);
                }

                if(string.IsNullOrEmpty(Template)){
                    throw new Exception($"{i18n.dug.ER_Run_Output_Format_Requires_Template} ({default(OutputFormats)})");
                }
                _outputFormat = value;
            }
        }

        private int _tableDetailLevel;
        [Option('d', "table-detail", Default = 1, HelpText = "HT_Run_Table_Detail_Level", ResourceType = typeof(i18n.dug))]
        public int TableDetailLevel { get{return _tableDetailLevel;}
             set
             {
                 if(value < 1 || value > 2) //Currently only support 1 and 2
                 {
                    throw new Exception(i18n.dug.ER_Run_Table_Detail_Out_Of_Range);
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