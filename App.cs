using System;
using System.Threading.Tasks;
using System.Linq;
using CommandLine;
using dug.Options;
using dug.Parsing;
using DnsClient;
using dug.Utils;
using dug.Services;
using System.Collections.Generic;
using dug.Data.Models;
using System.IO;

namespace dug
{
    public class App
    {
        private ParserResult<object> _cliArgs;
        private IDnsServerParser _parser;
        private IDnsServerService _dnsServerService;
        private IDnsQueryService _dnsQueryService;
        private IConsoleService _consoleService;

        public App(ParserResult<object> cliArgs, IDnsServerParser parser, IDnsServerService dnsServerService, IDnsQueryService dnsQueryService, IConsoleService consoleService)
        {
            _cliArgs = cliArgs;
            _parser = parser;
            _dnsServerService = dnsServerService;
            _dnsQueryService = dnsQueryService;
            _consoleService = consoleService;
        }

        public async Task<int> RunAsync()
        {
            // await _cliArgs.WithNotParsedAsync(HandleCliErrorsAsync); //TODO: Handle CLI Parsing errors (maybe)
            
            await _cliArgs.WithParsedAsync(ExecuteArgumentsAsync);
                
            return 0;
        }

        private async Task ExecuteArgumentsAsync(object args)
        {
            HandleGlobalOptions(args as GlobalOptions);
            _dnsServerService.EnsureServers();
            switch(args){
                case UpdateOptions uo:
                    await ExecuteUpdate(uo);
                    break;
                case RunOptions ro:
                    // If there are no servers in the db populate it from the built in list. I do this after the update so i dont load them before then just have them updated right away.
                    // Theoretically the update command could be the first one they run :)
                    // await _dnsServerService.EnsureServers();
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
            // 1. Determine the servers to be used
            //    - For now just get the top 3 most "reliable" servers per continent. Eventually I'll provide cli options to refine this.
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

            if(opts.Servers.Count() > 0){
                throw new NotImplementedException("Specifying individual servers in a run is not supported yet");
                //TODO: Should we 'decorate' these servers (turn them into DnsServers) before using them?
                        //If yes: We should do things like determine if they have DNSSEC, etc. Maybe this could be a static parse method off of DnsServer or something?
                // Also when we're rendering the results we shouldnt assume to have anything except the IPAddress... Maybe when you do this the rendering should be way simpler?

                //serversToUse.AddRange(opts.Servers);
            }

            if(serversToUse.Count == 0) {
                serversToUse = _dnsServerService.ServersByContinent.SelectMany(group => group.OrderByDescending(server => server.Reliability).Take(6)).ToList();
            }
            DugConsole.VerboseWriteLine("Server Count: "+serversToUse.Count());

            // 2. Run the queries with any options (any records, specific records, etc)            
            Console.WriteLine("URL: " + opts.Url); //Print pretty query info panel here
            var queryResults = await _dnsQueryService.QueryServers(opts.Url, serversToUse, TimeSpan.FromMilliseconds(opts.Timeout), opts.QueryTypes);

            // 3. Draw beautiful results in fancy table
            _consoleService.DrawResults(queryResults, opts);

            // 4. Update server reliability depending on results?
            // if(string.IsNullOrEmpty(opts.CustomServerFile)){
            //     _dnsServerService.UpdateServerReliabilityFromResults(queryResults);
            // }
        }

        private async Task ExecuteUpdate(UpdateOptions options)
        {
            if(!string.IsNullOrEmpty(options.CustomServerFile)){
                _dnsServerService.UpdateServersFromFile(options.CustomServerFile, options.Overwite);
                return;
            }
            await _dnsServerService.UpdateServersFromRemote(options.Overwite);
        }

        // private async Task HandleCliErrorsAsync(IEnumerable<Error> errs){
        //     //throw new NotImplementedException("Not handling cli parse errors yet!");
        // }

    }
}