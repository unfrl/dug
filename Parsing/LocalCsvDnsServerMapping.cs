using System;
using System.Net;
using dug.Services.Data.Models;
using TinyCsvParser.Mapping;
using TinyCsvParser.TypeConverter;

namespace dug.Services.Parsing
{
    public class LocalCsvDnsServerMapping : CsvMapping<DnsServer>
    {
        public LocalCsvDnsServerMapping() : base()
        {
            MapProperty(0, x => x.IPAddress, new IpAddressConverter());
            MapProperty(1, x => x.CountryCode);
            MapProperty(2, x => x.City);
            MapProperty(3, x => x.DNSSEC);
            MapProperty(4, x => x.Reliability);
        }
    }
}