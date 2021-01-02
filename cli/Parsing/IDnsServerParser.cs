using System.IO;
using System.Linq;
using dug.Data.Models;
using TinyCsvParser.Mapping;

namespace dug.Parsing
{
    public interface IDnsServerParser{
        ParallelQuery<DnsServer> ParseServersFromStream(Stream stream, ICsvMapping<DnsServer> format, bool skipHeaders, char separator);
    }
}