using System.Collections.Generic;
using DnsClient;
using dug.Data;
using dug.Data.Models;
using dug.Options;

namespace dug.Services
{
    public interface IConsoleService
    {
        void DrawResults(Dictionary<DnsServer, DnsResponse> results, RunOptions options);
    }
}