using dug.Services.Data.Models;
using TinyCsvParser;
using System.Linq;
using System.Text;
using System.IO;
using System;

namespace dug.Services.Parsing
{
    public enum DnsServerCsvFormats{
        Remote,
        Local
    }

    public class DnsServerParser: IDnsServerParser
    {
        private CsvParser<DnsServer> _remoteParser;
        private CsvParser<DnsServer> _localParser;

        public DnsServerParser(){
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
            _remoteParser = new CsvParser<DnsServer>(csvParserOptions, new RemoteCsvDnsServerMapping());
            _localParser = new CsvParser<DnsServer>(csvParserOptions, new LocalCsvDnsServerMapping());
        }

        public ParallelQuery<DnsServer> ParseServersFromStream(Stream stream, DnsServerCsvFormats format){
            switch(format){
                case DnsServerCsvFormats.Remote:
                    return _remoteParser.ReadFromStream(stream, Encoding.UTF8).Where(res => res.IsValid).Select(res => res.Result);
                case DnsServerCsvFormats.Local:
                    return _localParser.ReadFromStream(stream, Encoding.UTF8).Where(res => res.IsValid).Select(res => res.Result);
                default:
                    throw new NotImplementedException(); //TODO: Handle this?
            }
            
        }
    }
}