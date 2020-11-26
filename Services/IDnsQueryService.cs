using System.Collections.Generic;
using System.Threading.Tasks;
using dug.Data.Models;

namespace dug
{
    public interface IDnsQueryService
    {
        Task QueryServer(string url, IEnumerable<DnsServer> dnsServers);
    }
}