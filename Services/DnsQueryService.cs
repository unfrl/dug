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
                    Console.WriteLine($"START -- {server.IPAddress}");
                    clock.Start();
                    var queryResult = await QueryDnsServer(server, url, timeout, queryType, retries);
                    Console.WriteLine($"FINISH -- {server.IPAddress} -- {clock.ElapsedMilliseconds}");
                    results.TryAdd(server, queryResult);
                }
                catch (DnsResponseException dnsException){
                    if(dnsException.Code == DnsResponseCode.ConnectionTimeout){
                        Console.WriteLine($"TIMEOUT -- {server.IPAddress} -- {clock.ElapsedMilliseconds}");
                        return;
                    }
                    Console.WriteLine($"ERROR -- {server.IPAddress} -- {clock.ElapsedMilliseconds}");
                }
                catch{
                    Console.WriteLine($"UNHANDLED ERROR -- {server.IPAddress} -- {clock.ElapsedMilliseconds}");
                }
            });

            await Task.WhenAll(queryTaskList);

            Console.WriteLine($"Done, got {results.Count()} good responses out of {dnsServers.Count()} servers");

            return new Dictionary<DnsServer, IDnsQueryResponse>(results);
        }
    }
}