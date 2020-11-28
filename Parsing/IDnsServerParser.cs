using System.IO;
using System.Linq;
using dug.Data.Models;

namespace dug.Services.Parsing
{
    public interface IDnsServerParser{
        ParallelQuery<DnsServer> ParseServersFromStream(Stream stream, DnsServerCsvFormats format);
    }
}