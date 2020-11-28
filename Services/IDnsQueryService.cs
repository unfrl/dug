using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DnsClient;
using dug.Data.Models;

namespace dug.Services
{
    public interface IDnsQueryService
    {
        Task<Dictionary<DnsServer, IDnsQueryResponse>> QueryServers(string url, IEnumerable<DnsServer> dnsServers, TimeSpan timeout, QueryType queryType = QueryType.ANY, int retries = 0);
    }
}