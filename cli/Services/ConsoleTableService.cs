using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using DnsClient;
using dug.Data;
using dug.Data.Models;
using dug.Options;
using Spectre.Console;
using dug.Utils;
using CommandLine;

namespace dug.Services
{
    public class ConsoleTableService : IConsoleTableService
    {
        public void DrawResults(Dictionary<DnsServer, List<DnsResponse>> results, RunOptions options)
        {
            DrawUrlHeader(options);
            if(options.TableDetailLevel == 1){
                DrawConciseTable(results, options);
            }
            else {
                DrawVerboseTable(results, options);
            }
        }

        private void DrawVerboseTable(Dictionary<DnsServer, List<DnsResponse>> results, RunOptions options)
        {
            var resultsByContinent = results.OrderBy(pair => pair.Key.ContinentCode.ToString());

            var parentTable = new Table()
                .Border(TableBorder.MinimalHeavyHead)
                .BorderColor(Color.White)
                .AddColumn(new TableColumn($"[green][u]{i18n.dug.Table_Server_Info}[/][/]").Centered())
                .AddColumn(new TableColumn($"[green][u]{i18n.dug.Table_Results}[/][/]").Centered());

            ContinentCodes currentContinent = null;
            foreach(var resultPair in resultsByContinent){
                var server = resultPair.Key;

                //Check if we need to render a continent name
                if(server.ContinentCode.Code != currentContinent?.Code){
                    currentContinent = server.ContinentCode;
                    parentTable.AddRow(new Markup($"[bold underline blue] {currentContinent.Name} [/]"));
                    parentTable.AddEmptyRow();
                }

                //Create Server info for left column
                var serverInfoGrid = new Grid();
                serverInfoGrid.AddColumn(new GridColumn().NoWrap());
                serverInfoGrid.AddRow(server.IPAddress.ToString());
                // serverInfoGrid.AddRow(string.IsNullOrEmpty(server.CountryFlag) ? "" : server.CountryFlag); //I would really like to use these flag emojis but it seems like it has very little terminal support, most render them incorrectly...
                serverInfoGrid.AddRow(server.CityCountryName);
                serverInfoGrid.AddRow("DNSSEC: " + (server.DNSSEC == null ? "❓" : ((bool)server.DNSSEC ? "🔒" : "🔓")));
                serverInfoGrid.AddRow($"{i18n.dug.Table_Reliability}: {server.Reliability * 100}%");
                serverInfoGrid.AddEmptyRow();

                var resultTable = new Table().AddColumns("","",""); //Currently you must declare the column headers, even when you dont want to render them.
                resultTable.ShowHeaders = false;
                foreach(var result in resultPair.Value.OrderBy(res => res.RecordType.ToString())){
                    var answerString = TemplateHelper.GetAnswersString(result);
                    answerString = MarkupHelper.FormatDnsResponseMarkup(answerString, options.Hostname);
                    resultTable.AddRow(result.RecordType.ToString(), answerString, $"{result.ResponseTime}ms");
                }

                parentTable.AddRow(serverInfoGrid, resultTable);
            }

            AnsiConsole.Render(parentTable);
        }
        
        private void DrawConciseTable(Dictionary<DnsServer, List<DnsResponse>> results, RunOptions options){
            var table = new Table()
                .Border(TableBorder.MinimalHeavyHead)
                .BorderColor(Color.White)
                .AddColumn(new TableColumn($"[green][u]{i18n.dug.Table_Record_Type}[/][/]").Centered())
                .AddColumn(new TableColumn($"[green][u]{i18n.dug.Table_Value}[/][/]").Centered())
                .AddColumn(new TableColumn($"[green][u]{i18n.dug.Table_Continent_Consensus}[/][/]").LeftAligned());

            
            foreach(var queryType in options.ParsedQueryTypes){
                var resultsWithContinentCounts = new Dictionary<string, Dictionary<ContinentCodes, int>>();
                foreach(var result in results){
                    var server = result.Key;
                    var responses = result.Value;

                    var relevantResponse = responses.Single(res => (QueryType)res.RecordType == queryType);
                    var answerString = TemplateHelper.GetAnswersString(relevantResponse);
                    if(resultsWithContinentCounts.ContainsKey(answerString)){
                        if(resultsWithContinentCounts[answerString].ContainsKey(server.ContinentCode)){
                            resultsWithContinentCounts[answerString][server.ContinentCode]++;
                        }
                        else{
                            resultsWithContinentCounts[answerString][server.ContinentCode] = 1;
                        }
                    }
                    else{
                        resultsWithContinentCounts[answerString] = new Dictionary<ContinentCodes, int>(new ContinentCodeComparer()){{server.ContinentCode, 1}};
                    }
                }
                
                var continentTotals = new Dictionary<ContinentCodes, int>(new ContinentCodeComparer());
                foreach(ContinentCodes continent in ContinentCodes.Continents){
                    var totalContinentInstances = resultsWithContinentCounts.Sum(res => res.Value.GetValueOrDefault(continent));
                    continentTotals[continent] = totalContinentInstances;
                }

                foreach(var groupedResult in resultsWithContinentCounts){
                    table.AddRow(
                        new Text(queryType.ToString()),
                        new Markup(MarkupHelper.FormatDnsResponseMarkup(groupedResult.Key, options.Hostname)),
                        new Markup(MarkupHelper.FormatConsensusMarkup(groupedResult.Value, continentTotals))
                        );
                        table.AddEmptyRow();
                }

                table.AddRow(new Rule().HeavyBorder(), new Rule().HeavyBorder(), new Rule().HeavyBorder());
            }
            AnsiConsole.Render(table);
        }

        private void DrawUrlHeader(RunOptions options)
        {
            AnsiConsole.Render(new Rule($"[green]{string.Join(',', options.ParsedQueryTypes)} {i18n.dug.Table_Records_For} {options.Hostname}[/]").RuleStyle(Style.Parse("blue")).DoubleBorder().LeftAligned());
        }

        public void RenderInfoPanel<T>(object args)
        {
            var table = new Table()
                .Border(TableBorder.MinimalHeavyHead)
                .BorderColor(Color.White)
                .AddColumn(new TableColumn($"[green][u]{i18n.dug.Table_Argument}[/][/]").Centered())
                .AddColumn(new TableColumn($"[green][u]{i18n.dug.Table_Value}[/][/]").Centered())
                .AddColumn(new TableColumn($"[green][u]{i18n.dug.Table_Description}[/][/]").LeftAligned());
            var optionProperties = typeof(T).GetProperties().Where(prop => prop.GetCustomAttribute<OptionAttribute>() != null);
            foreach(var property in optionProperties){
                
                var optionAttribute = property.GetCustomAttribute<OptionAttribute>();
                var valueString = property.GetValue(args)?.ToString() ?? "";

                if(valueString != (optionAttribute?.Default?.ToString() ?? ReflectionHelper.GetDefaultValueAsString(property.PropertyType))){
                    valueString = $"[green]{valueString}[/]";
                }
                else{
                    continue;
                }

                string shortArgString = string.IsNullOrEmpty(optionAttribute?.ShortName)? string.Empty : $"-{optionAttribute.ShortName} , ";
                string longArgString = string.IsNullOrEmpty(optionAttribute?.LongName) ? string.Empty : $"--{optionAttribute.LongName}";
                var argumentString = shortArgString + longArgString;
                var descriptionString = optionAttribute?.HelpText ?? string.Empty;
                table.AddRow(
                    new Markup(argumentString),
                    new Markup(valueString),
                    new Markup(descriptionString)
                );
            }
            AnsiConsole.Render(table);
        }
    }
}