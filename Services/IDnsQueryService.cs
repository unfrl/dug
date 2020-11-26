using System.Collections.Generic;
using System.Threading.Tasks;
using DnsClient;
using dug.Data.Models;

namespace dug
{
    public interface IDnsQueryService
    {
        Task<Dictionary<DnsServer, IDnsQueryResponse>> QueryServer(string url, IEnumerable<DnsServer> dnsServers);
    }
}