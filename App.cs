using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using dug.Options;
using dug.Parsing;

namespace dug
{
    public class App
    {
        private ParserResult<object> _cliArgs;
        private IDnsServerParser _parser;
        private IDnsServerService _dnsServerService;

        public App(ParserResult<object> cliArgs, IDnsServerParser parser, IDnsServerService dnsServerService)
        {
            _cliArgs = cliArgs;
            _parser = parser;
            _dnsServerService = dnsServerService;
        }

        public async Task<int> RunAsync()
        {
            await _cliArgs.WithNotParsedAsync(HandleCliErrorsAsync);
            await _cliArgs.WithParsedAsync(ExecuteArgumentsAsync);
                
            return 0;
        }

        private async Task ExecuteArgumentsAsync(object args)
        {
            _dnsServerService.EnsureServers();
            switch(args){
                case UpdateOptions uo:
                    await ExecuteUpdate(uo);
                    break;
                case RunOptions ro:
                    // If there are no servers in the db populate it from the built in list. I do this after the update so i dont load them before then just have them updated right away.
                    // Theoretically the update command could be the first one they run :)
                    // await _dnsServerService.EnsureServers();
                    ExecuteRun(ro);
                    break;
                default:
                    //TODO: Idk when itd get here...
                    break;
            }
        }

        private static int ExecuteRun(RunOptions opts)
        {
            Console.WriteLine("URL: "+opts.Url);
            return 0;
        }

        private async Task ExecuteUpdate(UpdateOptions options)
        {
            if(!string.IsNullOrEmpty(options.CustomServerFile)){
                _dnsServerService.UpdateServersFromFile(options.CustomServerFile);
                return;
            }
            await _dnsServerService.UpdateServersFromRemote();
            
        }

        private async Task HandleCliErrorsAsync(IEnumerable<Error> errs){
            throw new NotImplementedException("Not handling cli parse errors yet!");
        }

    }
}