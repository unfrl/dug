using dug.Data.Models;
using TinyCsvParser;
using System.Linq;
using System.Text;
using System.IO;
using System;
using TinyCsvParser.Mapping;

namespace dug.Parsing
{
    public enum DnsServerCsvFormats{
        Remote,
        Local
    }

    public class DnsServerParser: IDnsServerParser
    {
        public static readonly ICsvMapping<DnsServer> DefaultRemoteParser = new RemoteCsvDnsServerMapping();
        public static readonly ICsvMapping<DnsServer> DefaultLocalParser = new LocalCsvDnsServerMapping();

        public DnsServerParser(){
        }

        public ParallelQuery<DnsServer> ParseServersFromStream(Stream stream, ICsvMapping<DnsServer> format, bool skipHeaders, char separator){
            var parserOptions = new CsvParserOptions(skipHeaders, separator);
            var parser = new CsvParser<DnsServer>(parserOptions, format);
            return parser.ReadFromStream(stream, Encoding.UTF8).Where(res => res.IsValid).Select(res => res.Result);
        }
    }
}