using System;
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
        public void DrawConciseResults(Dictionary<DnsServer, DnsResponse> results, RunOptions options)
        {
            DrawUrlHeader(options);
            // DrawConciseTable(results);
        }

        public void DrawResults(Dictionary<DnsServer, DnsResponse> results, RunOptions options)
        {
            DrawUrlHeader(options);
            // DrawVerboseTable(results);
        }

        // private void DrawConciseTable(Dictionary<DnsServer, DnsResponse> results)
        // {
        //     var parentTable = new Table()
        //         .Border(TableBorder.Rounded)
        //         .BorderColor(Color.White)
        //         .AddColumn(new TableColumn("[u]Servers[/]").Centered())
        //         .AddColumn(new TableColumn("[u]Record Type[/]").Centered())
        //         .AddColumn(new TableColumn("[u]Answer[/]").Centered());

        //         var successfulServersByResult = new Dictionary<Tuple<QueryType,string>, HashSet<DnsServer>>();
                
        //         foreach (var pair in results.Where(res => !res.Value.HasError)){
        //             foreach(var answer in pair.Value.FilteredAnswers){
        //                 var resultKey = Tuple.Create<QueryType, string>((QueryType)answer.RecordType, answer.DomainName.Original);
        //                 if(successfulServersByResult.ContainsKey(resultKey)){
        //                     successfulServersByResult[resultKey].Add(pair.Key);
        //                 }
        //                 else{
        //                     successfulServersByResult[resultKey] = new HashSet<DnsServer>() {pair.Key};
        //                 }
        //             }
        //         }

        //         var failedQueries = results.Where(res => res.Value.HasError);
        //         Console.WriteLine("successfulServersByResult: "+successfulServersByResult.Count);
        //         Console.WriteLine("failedQueries: " + failedQueries.Count());

        //         foreach(var successfulPair in successfulServersByResult){
        //             var serversByContinent = successfulPair.Value.OrderByDescending(server => server.ContinentCode.Name).ToList();
        //             ContinentCodes currentContinent = null;
        //             for(int i = 0; i < serversByContinent.Count(); i++){
        //                 var server = serversByContinent[i];
        //                 if(server.ContinentCode.Code != currentContinent?.Code){
        //                     currentContinent = server.ContinentCode;
        //                     parentTable.AddEmptyRow();
        //                     parentTable.AddRow(new Markup($"[bold underline blue] {currentContinent.Name} [/]"));
        //                 }
        //                 if(i == 0){
        //                     parentTable.AddRow(new Text(server.CityCountryName + $" ({server.IPAddress})"), new Text(successfulPair.Key.Item1.ToString()), new Text(successfulPair.Key.Item2));
        //                 }
        //                 else{
        //                     parentTable.AddRow(new Text(server.CityCountryName + $" ({server.IPAddress})"));
        //                 }
        //             }

        //             parentTable.AddEmptyRow();
        //         }

        //         AnsiConsole.Render(parentTable);
        // }

        // private void DrawVerboseTable(Dictionary<DnsServer, DnsResponse> results)
        // {
        //     var parentTable = new Table()
        //         .Border(TableBorder.Rounded)
        //         .BorderColor(Color.White)
        //         .AddColumn(new TableColumn("[u]DNS Server[/]").Centered())
        //         .AddColumn(new TableColumn("[u]Answers[/]").Centered());
        //     ContinentCodes currentContinent = null;
        //     foreach(var result in results.OrderBy(pair => pair.Key.ContinentCode.ToString())){
        //         //Create Server info for left column
        //         var server = result.Key;
        //         var requestTimedOut = result.Value.HasError && result.Value.Error.Code == DnsResponseCode.ConnectionTimeout;

        //         var serverInfoGrid = new Grid();
        //         serverInfoGrid.AddColumn(new GridColumn().NoWrap());
        //         serverInfoGrid.AddRow(server.IPAddress.ToString());
        //         // string countryInfo = string.IsNullOrEmpty(server.CountryFlag) ? "" : server.CountryFlag; //I would really like to use these flag emojis but it seems like it has very little terminal support, most render them incorrectly...
        //         serverInfoGrid.AddRow(server.CityCountryName);
        //         serverInfoGrid.AddRow("DNSSEC: " + (server.DNSSEC == null ? "❓" : ((bool)server.DNSSEC ? "🔒" : "🔓")));
        //         serverInfoGrid.AddRow("Response (ms): "+ result.Value.ResponseTime + (requestTimedOut ? " ⏲️ (Timed out)" : ""));
        //         serverInfoGrid.AddEmptyRow();

        //         //Create result sub-table for right column
        //         IRenderable resultsInfo;
        //         if(requestTimedOut){
        //             resultsInfo = new Panel("[b]No Answers Received Due to Timeout[/]").BorderColor(Color.Red);
        //         }
        //         else if(result.Value.HasError){
        //             resultsInfo = new Panel($"[b]Error: {result.Value.Error.DnsError}[/]").BorderColor(Color.Red);
        //         }
        //         else if(result.Value.FilteredAnswers.Count() < 1){
        //             resultsInfo = new Panel("[b]No Answers Received[/]").BorderColor(Color.Orange1);
        //         }
        //         else{
        //             resultsInfo = new Table()
        //             .Border(TableBorder.Rounded)
        //             .BorderColor(Color.Green)
        //             .AddColumn(new TableColumn("[u]Record Type[/]").Centered())
        //             .AddColumn(new TableColumn("[u]Result[/]").Centered());

        //             foreach(var answer in result.Value.FilteredAnswers){
        //                 ((Table)resultsInfo).AddRow(answer.RecordType.ToString(), answer.DomainName.Original);
        //             }
        //         }
                
        //         if(server.ContinentCode.Code != currentContinent?.Code){
        //             currentContinent = server.ContinentCode;
        //             parentTable.AddRow(new Markup($"[bold underline blue] {currentContinent.Name} [/]"));
        //             parentTable.AddEmptyRow();
        //         }
        //         parentTable.AddRow(serverInfoGrid, resultsInfo);
        //     }

        //     AnsiConsole.Render(parentTable);
        // }

        private void DrawUrlHeader(RunOptions options)
        {
            AnsiConsole.Render(new Rule($"[green]{string.Join(',', options.QueryTypes)} records for {options.Url}[/]").RuleStyle(Style.Parse("blue")).DoubleBorder().LeftAligned());
        }
    }
}