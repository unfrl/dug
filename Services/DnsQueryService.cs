using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DnsClient;
using dug.Data;
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

        public async Task<Dictionary<DnsServer, DnsResponse>> QueryServers(string url, IEnumerable<DnsServer> dnsServers, TimeSpan timeout, QueryType queryType = QueryType.ANY, int retries = 0)
        {
            ConcurrentDictionary<DnsServer, DnsResponse> results = new ConcurrentDictionary<DnsServer, DnsResponse>();

            var queryTaskList = dnsServers.Select(async server => {
                Stopwatch clock = new Stopwatch();
                
                try{
                    DugConsole.VerboseWriteLine($"START -- {server.IPAddress}");
                    clock.Start();
                    var queryResult = await QueryDnsServer(server, url, timeout, queryType, retries);
                    long responseTime = clock.ElapsedMilliseconds;
                    DugConsole.VerboseWriteLine($"FINISH -- {server.IPAddress} -- {responseTime}");
                    results.TryAdd(server, new DnsResponse(queryResult, responseTime));
                }
                catch (DnsResponseException dnsException){
                    long responseTime = clock.ElapsedMilliseconds;
                    results.TryAdd(server, new DnsResponse(dnsException, responseTime));
                    if(dnsException.Code == DnsResponseCode.ConnectionTimeout){
                        DugConsole.VerboseWriteLine($"TIMEOUT -- {server.IPAddress} -- {responseTime}");
                        return;
                    }
                    DugConsole.VerboseWriteLine($"ERROR -- {server.IPAddress} -- {responseTime}");
                }
                catch{
                    DugConsole.VerboseWriteLine($"UNHANDLED ERROR -- {server.IPAddress} -- {clock.ElapsedMilliseconds}");
                }
            });

            await Task.WhenAll(queryTaskList);

            Console.WriteLine($"Done, got {results.Count()} good responses out of {dnsServers.Count()} servers");

            return new Dictionary<DnsServer, DnsResponse>(results);
        }
    }
}