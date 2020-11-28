using dug.Data.Models;
using TinyCsvParser.Mapping;

namespace dug.Services.Parsing
{
    public class RemoteCsvDnsServerMapping : CsvMapping<DnsServer>
    {
        public RemoteCsvDnsServerMapping() : base()
        {
            MapProperty(0, x => x.IPAddress, new IpAddressConverter());
            MapProperty(4, x => x.CountryCode);
            MapProperty(5, x => x.City);
            MapProperty(8, x => x.DNSSEC);
            MapProperty(9, x => x.Reliability);
        }
    }
}