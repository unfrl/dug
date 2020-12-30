using System.Collections.Generic;
using DnsClient;
using dug.Data;
using dug.Data.Models;
using dug.Options;

namespace dug.Services
{
    public interface IConsoleService
    {
        void DrawResults(Dictionary<DnsServer, List<DnsResponse>> results, RunOptions options);

        /*
            Used during verbose output to allow the user to see what arguments were parsed by the application
        */
        void RenderInfoPanel<T>(object args);
    }
}