using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DnsClient;
using dug.Data.Models;

namespace dug
{
    public class DnsQueryService : IDnsQueryService
    {
        public async Task<Dictionary<DnsServer, IDnsQueryResponse>> QueryServer(string url, IEnumerable<DnsServer> dnsServers)
        {
            ConcurrentDictionary<DnsServer, IDnsQueryResponse> results = new ConcurrentDictionary<DnsServer, IDnsQueryResponse>();

            var queryTaskList = dnsServers.Select(async server => {
                var dnsServer = new IPEndPoint(IPAddress.Parse(server.IPAddress.ToString()), 53);
                LookupClientOptions options = new LookupClientOptions(new IPAddress[] {server.IPAddress});
                options.Timeout = TimeSpan.FromSeconds(5); //Should be configurable
                options.Retries = 0; //Should be configurable
                options.ThrowDnsErrors = false; //Things like timeouts shouldnt throw an error, instead the results will show that they have an error
                options.ContinueOnDnsError = false;
                Stopwatch clock = new Stopwatch();
                var client = new LookupClient(options);
                try{
                    Console.WriteLine($"START -- {dnsServer.Address}");
                    clock.Start();
                    var queryResult = await client.QueryAsync(url, QueryType.ANY);
                    Console.WriteLine($"FINISH -- {dnsServer.Address} -- {clock.ElapsedMilliseconds}");
                    results.TryAdd(server, queryResult);
                }
                catch (DnsResponseException dnsException){
                    if(dnsException.Code == DnsResponseCode.ConnectionTimeout){
                        Console.WriteLine($"TIMEOUT -- {dnsServer.Address} -- {clock.ElapsedMilliseconds}");
                        return;
                    }
                    Console.WriteLine($"ERROR -- {dnsServer.Address} -- {clock.ElapsedMilliseconds}");
                    // Console.WriteLine($"Failed to retrieve data from DNS Server @ ({dnsServer.Address})");
                }
                catch{
                    Console.WriteLine($"UNHANDLED ERROR -- {dnsServer.Address} -- {clock.ElapsedMilliseconds}");
                    // Console.WriteLine($"Failed to retrieve data from DNS Server @ ({dnsServer.Address})");
                }
            });

            await Task.WhenAll(queryTaskList);

            Console.WriteLine($"Done, got {results.Count()} good responses out of {dnsServers.Count()} servers");

            return new Dictionary<DnsServer, IDnsQueryResponse>(results);
        }
    }
}