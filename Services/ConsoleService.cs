using System.Collections.Generic;
using System.Linq;
using DnsClient;
using dug.Data;
using dug.Data.Models;
using dug.Options;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace dug.Services
{
    public class ConsoleService : IConsoleService
    {
        public void DrawResults(Dictionary<DnsServer, DnsResponse> results, RunOptions options)
        {
            DrawUrlHeader(options.Url);
            DrawTable(results);
        }

        private void DrawTable(Dictionary<DnsServer, DnsResponse> results)
        {
            var parentTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.White)
                .AddColumn(new TableColumn("[u]DNS Server[/]").Centered())
                .AddColumn(new TableColumn("[u]Answers[/]").Centered());

            foreach(var result in results){
                //Create Server info for left column
                var server = result.Key;
                var requestTimedOut = result.Value.HasError && result.Value.Error.Code == DnsResponseCode.ConnectionTimeout;

                var serverInfoGrid = new Grid();
                serverInfoGrid.AddColumn(new GridColumn().NoWrap());
                serverInfoGrid.AddRow(server.IPAddress.ToString());
                // string countryInfo = string.IsNullOrEmpty(server.CountryFlag) ? "" : server.CountryFlag; //I would really like to use these flag emojis but it seems like it has very little terminal support, most render them incorrectly...
                serverInfoGrid.AddRow(server.LongName);
                serverInfoGrid.AddRow("DNSSEC: " + (server.DNSSEC == null ? "❓" : ((bool)server.DNSSEC ? "🔒" : "🔓")));
                serverInfoGrid.AddRow("Response (ms): "+ result.Value.ResponseTime + (requestTimedOut ? " ⏲️ (Timed out)" : ""));
                serverInfoGrid.AddEmptyRow();

                //Create result sub-table for right column
                IRenderable resultsInfo;
                if(requestTimedOut){
                    resultsInfo = new Panel("[b]No Answers Received Due to Timeout[/]").BorderColor(Color.Red);
                }
                else if(result.Value.HasError){
                    resultsInfo = new Panel($"[b]Error: {result.Value.Error.DnsError}[/]").BorderColor(Color.Red);
                }
                else if(result.Value.FilteredAnswers.Count() < 1){
                    resultsInfo = new Panel("[b]No Answers Received[/]").BorderColor(Color.Orange1);
                }
                else{
                    resultsInfo = new Table()
                    .Border(TableBorder.Rounded)
                    .BorderColor(Color.Green)
                    .AddColumn(new TableColumn("[u]Record Type[/]").Centered())
                    .AddColumn(new TableColumn("[u]Result[/]").Centered());

                    foreach(var answer in result.Value.FilteredAnswers){
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