using System.Threading.Tasks;
using CommandLine;
using dug.Services;
using dug.Options;
using dug.Parsing;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace dug
{
    class Program
    {

        static async Task<int> Main(string[] args)
        {
            var services = ConfigureServices(args);

            var serviceProvider = services.BuildServiceProvider();

            int exitCode = 1;
            try{
                // calls the Run method in App, which is replacing Main
                exitCode = await serviceProvider.GetService<App>().RunAsync();
            }
            catch(Exception ex){
                if(ex is AggregateException ag){
                    Console.Error.WriteLine(ag.InnerExceptions[0].Message);
                }
                else{
                    Console.Error.WriteLine(ex.Message);
                }
            }

            return exitCode;
        }

        private static IServiceCollection ConfigureServices(string[] args)
        {
            IServiceCollection services = new ServiceCollection();

            var parser = new CommandLine.Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<RunOptions, UpdateOptions>(args);

            services.AddSingleton(parserResult);
            services.AddTransient<IDnsServerParser, DnsServerParser>();
            services.AddTransient<IDnsServerService, DnsServerService>();
            services.AddTransient<IDnsQueryService, DnsQueryService>();
            services.AddTransient<IConsoleTableService, ConsoleTableService>();
            services.AddTransient<IConsoleTemplateService, ConsoleTemplateService>();
            services.AddSingleton<IPercentageAnimator>(new PercentageAnimator());

            // required to run the application
            services.AddTransient<App>();

            return services;
        }
    }
}
