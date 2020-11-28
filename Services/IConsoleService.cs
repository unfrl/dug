using System.Collections.Generic;
using DnsClient;
using dug.Data.Models;
using dug.Options;

namespace dug.Services
{
    public interface IConsoleService
    {
        void DrawResults(Dictionary<DnsServer, IDnsQueryResponse> results, RunOptions options);
    }
}