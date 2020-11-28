using System;
using System.Threading.Tasks;
using System.Linq;
using CommandLine;
using dug.Options;
using dug.Parsing;
using DnsClient;
using dug.Utils;

namespace dug
{
    public class App
    {
        private ParserResult<object> _cliArgs;
        private IDnsServerParser _parser;
        private IDnsServerService _dnsServerService;
        private IDnsQueryService _dnsQueryService;

        public App(ParserResult<object> cliArgs, IDnsServerParser parser, IDnsServerService dnsServerService, IDnsQueryService dnsQueryService)
        {
            _cliArgs = cliArgs;
            _parser = parser;
            _dnsServerService = dnsServerService;
            _dnsQueryService = dnsQueryService;
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
            //    - For now just get the top 1 most reliable servers per continent. Eventually I'll provide cli options to refine this.
            var topServersByContinent = _dnsServerService.ServersByContinent.ToList().SelectMany(group => group.OrderByDescending(server => server.Reliability).Take(1));
            DugConsole.VerboseWriteLine("Server Count: "+topServersByContinent.Count());

            // 2. Run the queries with any options (any records, specific records, etc)
            QueryType queryType = opts.QueryTypes.First();
            if(opts.QueryTypes.Count()>1){
                foreach(QueryType qt in opts.QueryTypes.Skip(1)){
                    queryType = (queryType & qt);
                }
            }
            //Querytype sanity check
            // Console.WriteLine("HAS NS: "+QueryType.NS.HasFlag(queryType));
            // Console.WriteLine("HAS MX: "+QueryType.MX.HasFlag(queryType));
            // Console.WriteLine("HAS CNAME: "+QueryType.CNAME.HasFlag(queryType));
            
            Console.WriteLine("URL: " + opts.Url);
            var queryResults = await _dnsQueryService.QueryServers(opts.Url, topServersByContinent, TimeSpan.FromMilliseconds(opts.Timeout), queryType);

            // 3. Draw beautiful results in fancy table
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