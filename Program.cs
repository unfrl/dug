using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using dug.Services;
using dug.Options;
using dug.Parsing;
using Microsoft.Extensions.DependencyInjection;

namespace dug
{
    class Program
    {

        static async Task<int> Main(string[] args)
        {
            var services = ConfigureServices(args);

            var serviceProvider = services.BuildServiceProvider();

            // calls the Run method in App, which is replacing Main
            return await serviceProvider.GetService<App>().RunAsync();
        }

        private static IServiceCollection ConfigureServices(string[] args)
        {
            IServiceCollection services = new ServiceCollection();

            var parsedArgs = Parser.Default.ParseArguments<RunOptions, UpdateOptions>(args);

            services.AddSingleton(parsedArgs);
            services.AddTransient<IDnsServerParser, DnsServerParser>();
            services.AddTransient<IDnsServerService, DnsServerService>();
            services.AddTransient<IDnsQueryService, DnsQueryService>();
            services.AddTransient<IConsoleService, ConsoleService>();

            // required to run the application
            services.AddTransient<App>();

            return services;
        }
    }
}
