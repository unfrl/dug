using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DnsClient;
using dug.Data;
using dug.Data.Models;

namespace dug.Services
{
    public interface IDnsQueryService
    {
        Task<Dictionary<DnsServer, List<DnsResponse>>> QueryServers(string url, IEnumerable<DnsServer> dnsServers, TimeSpan timeout, IEnumerable<QueryType> queryTypes, int retries = 0);
    }
}