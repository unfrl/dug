using System;
using System.Net;
using dug.Data.Models;
using TinyCsvParser.Mapping;
using TinyCsvParser.TypeConverter;

namespace dug
{
    public class CsvDnsServerMapping : CsvMapping<DnsServer>
    {
        public CsvDnsServerMapping() : base()
        {
            MapProperty(0, x => x.IPAddress, new IpAddressConverter());
            MapProperty(4, x => x.CountryCode);
            MapProperty(5, x => x.City);
            MapProperty(8, x => x.DNSSEC);
            MapProperty(9, x => x.Reliability);
        }
    }

    internal class IpAddressConverter : ITypeConverter<IPAddress>
    {
        public Type TargetType => typeof(IPAddress);

        public bool TryConvert(string value, out IPAddress result)
        {
            return IPAddress.TryParse(value, out result);
        }
    }
}