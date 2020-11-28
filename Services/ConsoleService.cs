using System.Collections.Generic;
using DnsClient;
using dug.Data.Models;
using dug.Options;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace dug.Services
{
    public class ConsoleService : IConsoleService
    {
        public void DrawResults(Dictionary<DnsServer, IDnsQueryResponse> results, RunOptions options)
        {
            DrawUrlHeader(options.Url);
            DrawTable(results);
        }

        private void DrawTable(Dictionary<DnsServer, IDnsQueryResponse> results)
        {
            var parentTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.White)
                .AddColumn(new TableColumn("[u]DNS Server[/]").Centered())
                .AddColumn(new TableColumn("[u]Answers[/]").Centered());

            foreach(var result in results){
                //Create Server info for left column
                var server = result.Key;
                var serverInfoGrid = new Grid();
                serverInfoGrid.AddColumn(new GridColumn().NoWrap());
                serverInfoGrid.AddRow(server.IPAddress.ToString());
                // string countryInfo = string.IsNullOrEmpty(server.CountryFlag) ? "" : server.CountryFlag; //I would really like to use these flag emojis but it seems like it has very little terminal support, most render them incorrectly...
                serverInfoGrid.AddRow(server.LongName);
                serverInfoGrid.AddRow("DNSSEC: " + (server.DNSSEC == null ? "❓" : ((bool)server.DNSSEC ? "🔒" : "🔓")));
                serverInfoGrid.AddRow("Response (ms): "+"TIME_HERE"); //TODO: Extend IDnsQueryResponse so I can add data like response time
                serverInfoGrid.AddEmptyRow();

                //Create result sub-table for right column
                IRenderable resultsInfo;
                if(result.Value.Answers.Count < 1){
                    resultsInfo = new Panel("[b]No Answers Received[/]").BorderColor(Color.Red);
                }
                else{
                    resultsInfo = new Table()
                    .Border(TableBorder.Rounded)
                    .BorderColor(Color.Green)
                    .AddColumn(new TableColumn("[u]Record Type[/]").Centered())
                    .AddColumn(new TableColumn("[u]Result[/]").Centered());

                    
                    foreach(var answer in result.Value.Answers){
                        //TODO: Only render the requested record types!
                        ((Table)resultsInfo).AddRow(answer.RecordType.ToString(), answer.DomainName.Original);
                    }
                }


                parentTable.AddRow(serverInfoGrid, resultsInfo);
            }

            AnsiConsole.Render(parentTable);
        }

        private void DrawUrlHeader(string url)
        {
            AnsiConsole.Render(new Rule($"[green]{url}[/]").RuleStyle(Style.Parse("blue")).DoubleBorder().LeftAligned());
        }
    }
}