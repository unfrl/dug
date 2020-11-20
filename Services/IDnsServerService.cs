using System.Collections.Generic;
using System.Threading.Tasks;
using dug.Data.Models;

namespace dug
{
    public interface IDnsServerService{
        List<DnsServer> Servers { get; }

        // Specify the source file to load servers from and update the current server file (Config.ServersFile) with any novel servers found.
        // If overwrite is set the current server file (Config.ServersFile) will be overwritten, not just updated
        void UpdateServersFromFile(string customFilePath, bool overwrite);

        // Update the current server file (Config.ServersFile) with any novel servers found from the remote source (https://public-dns.info/nameservers.csv).
        // If overwrite is set the current server file (Config.ServersFile) will be overwritten, not just updated
        Task UpdateServersFromRemote(bool overwrite);

        void EnsureServers();
    }
}