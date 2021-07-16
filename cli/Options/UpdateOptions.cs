using System;
using System.Collections.Generic;
using System.Net;
using CommandLine;
using CommandLine.Text;
using dug.Data.Models;
using dug.Utils;

namespace dug.Options
{
    public enum ReliabilityUpdateType
    {
        Normal,
        Prune
    }

    [Verb("update", HelpText = "HT_Update", ResourceType = typeof(i18n.dug))]
    public class UpdateOptions : GlobalOptions
    {

        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example(i18n.dug.Default, new UpdateOptions());
                yield return new Example(i18n.dug.EX_Overwrite, new UnParserSettings(){ PreferShortName = true}, new UpdateOptions { Overwite = true });
                yield return new Example(i18n.dug.EX_Import_Servers, new UnParserSettings(){ PreferShortName = true}, new UpdateOptions { Servers = "8.8.8.8,2001:4860:4860::8888" });
                yield return new Example(i18n.dug.EX_Import_And_Overwrite_Servers, new UnParserSettings(){ PreferShortName = true}, new UpdateOptions { Servers = "8.8.8.8,2001:4860:4860::8888", Overwite = true });
                yield return new Example(i18n.dug.EX_Remote_Import_And_Update, new UnParserSettings(){ PreferShortName = true}, new UpdateOptions { Reliability = ReliabilityUpdateType.Normal });
                yield return new Example(i18n.dug.EX_Remote_Import_And_Update_Prune, new UnParserSettings(){ PreferShortName = true}, new UpdateOptions { Reliability = ReliabilityUpdateType.Prune });
                yield return new Example(i18n.dug.EX_Update_Reliability_Prune_2_Retries, new UnParserSettings(){ PreferShortName = true}, new UpdateOptions { Reliability = ReliabilityUpdateType.Prune, ReliabilityOnly = true, QueryRetries = 2 });
            }
        }

        [Option('f', "file", Required = false, HelpText = "HT_Update_Custom_Server_File", ResourceType = typeof(i18n.dug))] //TODO: At some point we need a link here to a readme showing the format the file must be in.
        public string CustomServerFile { get; set; }

        private string _servers;
        [Option('s', "servers", Required = false, HelpText = "HT_Update_Servers", ResourceType = typeof(i18n.dug))]
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
                        throw new Exception($"{i18n.dug.ER_Update_Server_Parse} {addressString}");
                    }
                }
            }
        }
        public List<DnsServer> ParsedServers { get; set; }

        [Option('o', "overwrite", Required = false, HelpText = "HT_Update_Overwrite", ResourceType = typeof(i18n.dug))]
        public bool Overwite { get; set; }

        [Option('r', "reliability", Required = false, HelpText = "HT_Update_Reliability", ResourceType = typeof(i18n.dug))]
        public ReliabilityUpdateType? Reliability { get; set; }

        private bool _reliabilityOnly;
        [Option("reliability-only", Required = false, HelpText = "HT_Update_Reliability_Only", ResourceType = typeof(i18n.dug))]
        public bool ReliabilityOnly {get{return _reliabilityOnly;}
            set
            {
                if(Reliability == null){
                    throw new Exception(i18n.dug.ER_Update_Reliability_Only_Requires_Reliability);
                }
                _reliabilityOnly = value;
            }
        }

        private string _updateURL;
        [Option("update-url", Required = false, HelpText = "HT_Update_Update_URL", ResourceType = typeof(i18n.dug))]
        public string UpdateURL { get{return _updateURL;}
            set
            {
                if(!string.IsNullOrEmpty(CustomServerFile) || !string.IsNullOrEmpty(Servers)){
                    throw new Exception(i18n.dug.ER_Update_Update_URL_Requires_File_Or_Servers);
                }
                else if(string.IsNullOrEmpty(DataColumns)){
                    throw new Exception(i18n.dug.ER_Update_Update_URL_Requires_Data_Columns);
                }
                Uri parseResult;
                var validURL = Uri.TryCreate(value, UriKind.Absolute, out parseResult) && (parseResult.Scheme == Uri.UriSchemeHttp || parseResult.Scheme == Uri.UriSchemeHttps);
                if(!validURL){
                    throw new Exception($"{i18n.dug.ER_Update_Unable_Parse_Update_URL} {value}");
                }
                _updateURL = value;
            }
        }

        private string _dataColumns;
        [Option("data-columns", Required = false, HelpText = "HT_Update_Data_Columns", ResourceType = typeof(i18n.dug))]
        public string DataColumns { get{return _dataColumns;}
            set
            {
                var columns = value.ToLowerInvariant().Split(',', StringSplitOptions.None); //Specifically DO NOT remove empty entries
                foreach(var column in columns){
                    if(!TemplateHelper.ServerSetterMap.ContainsKey(column)){
                        throw new Exception($"{i18n.dug.ER_Update_Unable_Parse_Data_Column_Header} {column}");
                    }
                }
                _dataColumns = value.ToLowerInvariant();
            }
        }

        private bool _dataHeadersPresent;
        [Option("data-headers-present", Required = false, HelpText = "HT_Update_Data_Headers_Present", ResourceType = typeof(i18n.dug))]
        public bool DataHeadersPresent { get{return _dataHeadersPresent;}
            set
            {
                if(string.IsNullOrEmpty(DataColumns)){
                    throw new Exception(i18n.dug.ER_Update_Data_Headers_Present_Requires_Data_Columns);
                }
                _dataHeadersPresent = value;
            }
        }

        private char? _dataSeparator;
        [Option("data-separator", Required = false, HelpText = "HT_Update_Data_Separator", ResourceType = typeof(i18n.dug))]
        public char? DataSeparator { get{return _dataSeparator;}
            set
            {
                if(string.IsNullOrEmpty(DataColumns)){
                    throw new Exception(i18n.dug.ER_Update_Data_Separator_Requires_Data_Columns);
                }
                _dataSeparator = value;
            }
        }
    }
}