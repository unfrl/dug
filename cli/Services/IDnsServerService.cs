using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using dug.Data;
using dug.Data.Models;
using TinyCsvParser.Mapping;

namespace dug.Services
{
    public interface IDnsServerService{
        List<DnsServer> Servers { get; }

        ILookup<ContinentCodes, DnsServer> ServersByContinent { get; }

        // Specify the source file to load servers from and update the current server file (Config.ServersFile) with any novel servers found.
        // If overwrite is set the current server file (Config.ServersFile) will be overwritten, not just updated
        void UpdateServersFromFile(string customFilePath, string customHeaders, bool skipHeaders, bool overwrite);

        // Update the current server file (Config.ServersFile) with any novel servers found from the remote source (https://public-dns.info/nameservers.csv).
        // If overwrite is set the current server file (Config.ServersFile) will be overwritten, not just updated
        Task UpdateServersFromDefaultRemote(bool overwrite);

        // Update the current server file (Config.ServersFile) with any novel servers found from the remote specified source url
        // Attempt to parse the data using the specified customHeaders
        // Ignore the first line if skipHeaders is true
        // If overwrite is set the current server file (Config.ServersFile) will be overwritten, not just updated
        Task UpdateServersFromRemote(string url, string customHeaders, bool skipHeaders, bool overwrite);

        // Update the current server file (Config.ServersFile) with any novel servers provided
        // If overwrite is set the current server file (Config.ServersFile) will be overwritten, not just updated
        void UpdateServers(List<DnsServer> servers, bool overwrite);

        List<DnsServer> ParseServersFromStream(Stream stream, ICsvMapping<DnsServer> format, bool skipHeaders);

        // This will walk through the results and reduce the reliability of the servers that either gave an error or timed out.
        // If prune is set to true servers that failed are removed.
        // The reduction is specified by 'penalty' and can range from 0.0 to 1.0, it defaults to 0.1.
        // It also slightly improves the reliability of servers (up to 1.0) that responded
        // The improvement is specified by 'promotion' and can range from 0.0 to 1.0, it defaults to 0.01.
        // NOTE: The numbers specified are NOT percentages, even though they usually are when we import the servers. When you specify a penalty of 0.1 and a server that had a reliability of 0.5 fails, it will now have a reliability of 0.4.
        void UpdateServerReliabilityFromResults(Dictionary<DnsServer, List<DnsResponse>> results, bool prune, double penalty = 0.1, double promotion = 0.01);

        void EnsureServers();
    }
}