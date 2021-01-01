using System;
using System.Threading.Tasks;
using System.Linq;
using CommandLine;
using dug.Options;
using dug.Parsing;
using dug.Utils;
using dug.Services;
using System.Collections.Generic;
using dug.Data.Models;
using System.IO;
using dug.Data;
using DnsClient;

namespace dug
{
    public class App
    {
        private ParserResult<object> _cliArgs;
        private IDnsServerParser _parser;
        private IDnsServerService _dnsServerService;
        private IDnsQueryService _dnsQueryService;
        private IConsoleTableService _consoleTableService;
        private IConsoleTemplateService _consoleTemplateService;
        private IPercentageAnimator _percentageAnimator;

        public App(ParserResult<object> cliArgs, IDnsServerParser parser, IDnsServerService dnsServerService, IDnsQueryService dnsQueryService, IConsoleTableService consoleTableService, IConsoleTemplateService consoleTemplateService, IPercentageAnimator percentageAnimator)
        {
            _cliArgs = cliArgs;
            _parser = parser;
            _dnsServerService = dnsServerService;
            _dnsQueryService = dnsQueryService;
            _consoleTableService = consoleTableService;
            _consoleTemplateService = consoleTemplateService;
            _percentageAnimator = percentageAnimator;
        }

        public async Task<int> RunAsync()
        {
            // _cliArgs.WithNotParsed(errs => DisplayHelp(_cliArgs, errs)); //TODO: Handle CLI Parsing errors (maybe)
            await _cliArgs.WithParsedAsync(ExecuteArgumentsAsync);
                
            return 0;
        }

        private async Task ExecuteArgumentsAsync(object args)
        {
            HandleGlobalOptions(args as GlobalOptions);
            
            _dnsServerService.EnsureServers();
            switch(args){
                case UpdateOptions uo:
                    if(Config.Verbose){
                        _consoleTableService.RenderInfoPanel<UpdateOptions>(args);
                    }
                    await ExecuteUpdate(uo);
                    break;
                case RunOptions ro:
                    if(Config.Verbose){
                        _consoleTableService.RenderInfoPanel<RunOptions>(args);
                    }
                    await ExecuteRun(ro);
                    break;
                default:
                    //TODO: Idk when itd get here...
                    break;
            }
        }

        private void HandleGlobalOptions(GlobalOptions options){
            Config.Verbose = options.Verbose;
            DugConsole.VerboseWriteLine("Verbose Output Enabled");
        }

        private async Task ExecuteRun(RunOptions opts)
        {
            // 0. Validate Arguments (Happening in Options.cs in the set methods)

            // 1. Determine the servers to be used
            //    - For now just get the top opts.ServerCount most "reliable" servers per continent. Eventually I'll provide cli options to refine this.
            List<DnsServer> serversToUse = new List<DnsServer>();
            if(!string.IsNullOrEmpty(opts.CustomServerFile)){
                if(!File.Exists(opts.CustomServerFile)){
                    Console.WriteLine($"Specified file does not exist: {opts.CustomServerFile}");
                    System.Environment.Exit(1);
                }
                using(var streamReader = File.OpenText(opts.CustomServerFile)){
                    serversToUse = _dnsServerService.ParseServersFromStream(streamReader.BaseStream, DnsServerCsvFormats.Local);
                }
            }

            if(opts.ParsedServers?.Count() > 0){
                //TODO: Should we 'decorate' these servers (turn them into DnsServers) before using them?
                        //If yes: We should do things like determine if they have DNSSEC, etc. Maybe this could be a static parse method off of DnsServer or something?
                // Also when we're rendering the results we shouldnt assume to have anything except the IPAddress... Maybe when you do this the rendering should be way simpler?

                serversToUse.AddRange(opts.ParsedServers);
            }

            if(opts.MultipleServerSources || serversToUse.Count == 0) {
                var serversByReliability = _dnsServerService.ServersByContinent.SelectMany(group => group.OrderByDescending(server => server.Reliability).Take(opts.ServerCount));
                serversToUse.AddRange(serversByReliability
                    .Where(server => opts.ParsedContinents.Contains(server.ContinentCode, new ContinentCodeComparer())));
            }
            DugConsole.VerboseWriteLine("Server Count: "+serversToUse.Count());

            // 2. Run the queries with any options (any records, specific records, etc)
            if(opts.OutputFormat == OutputFormats.TABLES){
                _percentageAnimator.Start("", serversToUse.Count * opts.ParsedQueryTypes.Count());
            }
            var queryResults = await _dnsQueryService.QueryServers(opts.Hostname, serversToUse, TimeSpan.FromMilliseconds(opts.Timeout), opts.ParsedQueryTypes, opts.QueryParallelism, opts.QueryRetries, _percentageAnimator.EventHandler);
            _percentageAnimator.StopIfRunning();

            // 3. Draw beautiful results in fancy table
            if(opts.OutputFormat == OutputFormats.TABLES){
                _consoleTableService.DrawResults(queryResults, opts);
            }
            else{
                _consoleTemplateService.DrawResults(queryResults, opts);
            }

            // 4. Update server reliability depending on results
            if(string.IsNullOrEmpty(opts.CustomServerFile)){
                _dnsServerService.UpdateServerReliabilityFromResults(queryResults, false);
            }
        }

        private async Task ExecuteUpdate(UpdateOptions opts)
        {
            if(!opts.ReliabilityOnly){
                if(!string.IsNullOrEmpty(opts.CustomServerFile)){
                    _dnsServerService.UpdateServersFromFile(opts.CustomServerFile, opts.Overwite);
                }

                bool hasSpecifiedServers = opts.ParsedServers != null && opts.ParsedServers.Any();
                if(hasSpecifiedServers){
                    _dnsServerService.UpdateServers(opts.ParsedServers, opts.Overwite);
                }

                if(string.IsNullOrEmpty(opts.CustomServerFile) && !hasSpecifiedServers){
                    await _dnsServerService.UpdateServersFromRemote(opts.Overwite);
                }
            }

            if(opts.Reliability != null){
                _percentageAnimator.Start($"Testing {_dnsServerService.Servers.Count} server responses for google.com", _dnsServerService.Servers.Count);
                var results = await _dnsQueryService.QueryServers("google.com", _dnsServerService.Servers, TimeSpan.FromSeconds(3), new [] { QueryType.A }, opts.QueryParallelism, opts.QueryRetries, _percentageAnimator.EventHandler);
                _percentageAnimator.StopIfRunning();
                Console.WriteLine($"\nFinished, got {results.Select(pair => pair.Value.Count(res => !res.HasError)).Sum()} good responses out of {_dnsServerService.Servers.Count() * 1} requests");
                
                _dnsServerService.UpdateServerReliabilityFromResults(results, opts.Reliability == ReliabilityUpdateType.Prune);
                return;
            }
        }
    }
}