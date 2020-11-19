using System.Collections.Generic;
using System.Threading.Tasks;
using dug.Data.Models;

namespace dug
{
    public interface IDnsServerService{
        List<DnsServer> GetServers();

        void UpdateServersFromFile(string customFilePath);

        Task UpdateServersFromRemote();

        void EnsureServers();
    }
}