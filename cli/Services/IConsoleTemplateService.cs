using System.Collections.Generic;
using dug.Data;
using dug.Data.Models;
using dug.Options;

namespace dug.Services
{
    public interface IConsoleTemplateService
    {
        void DrawResults(Dictionary<DnsServer, List<DnsResponse>> results, RunOptions options);
    }
}