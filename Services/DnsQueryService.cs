using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DnsClient;
using dug.Data.Models;
using dug.Utils;

namespace dug.Services
{
    public class DnsQueryService : IDnsQueryService
    {
        private async Task<IDnsQueryResponse> QueryDnsServer(DnsServer server, string url, TimeSpan timeout, QueryType queryType = QueryType.ANY, int retries = 0){
            LookupClientOptions options = new LookupClientOptions(new IPAddress[] {server.IPAddress}) {
                    Timeout = timeout,
                    Retries = retries
                };

                options.ContinueOnDnsError = false;
                
                var client = new LookupClient(options);
                
                return await client.QueryAsync(url, QueryType.ANY);
        }

        public async Task<Dictionary<DnsServer, IDnsQueryResponse>> QueryServers(string url, IEnumerable<DnsServer> dnsServers, TimeSpan timeout, QueryType queryType = QueryType.ANY, int retries = 0)
        {
            ConcurrentDictionary<DnsServer, IDnsQueryResponse> results = new ConcurrentDictionary<DnsServer, IDnsQueryResponse>();

            var queryTaskList = dnsServers.Select(async server => {
                Stopwatch clock = new Stopwatch();
                
                try{
                    DugConsole.VerboseWriteLine($"START -- {server.IPAddress}");
                    clock.Start();
                    var queryResult = await QueryDnsServer(server, url, timeout, queryType, retries);
                    DugConsole.VerboseWriteLine($"FINISH -- {server.IPAddress} -- {clock.ElapsedMilliseconds}");
                    results.TryAdd(server, queryResult);
                }
                catch (DnsResponseException dnsException){
                    if(dnsException.Code == DnsResponseCode.ConnectionTimeout){
                        DugConsole.VerboseWriteLine($"TIMEOUT -- {server.IPAddress} -- {clock.ElapsedMilliseconds}");
                        return;
                    }
                    DugConsole.VerboseWriteLine($"ERROR -- {server.IPAddress} -- {clock.ElapsedMilliseconds}");
                }
                catch{
                    DugConsole.VerboseWriteLine($"UNHANDLED ERROR -- {server.IPAddress} -- {clock.ElapsedMilliseconds}");
                }
            });

            await Task.WhenAll(queryTaskList);

            Console.WriteLine($"Done, got {results.Count()} good responses out of {dnsServers.Count()} servers");

            return new Dictionary<DnsServer, IDnsQueryResponse>(results);
        }
    }
}