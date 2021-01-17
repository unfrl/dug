using System;
using System.Collections.Generic;
using dug.Data;
using dug.Data.Models;
using dug.Options;
using dug.Utils;
using System.Text.Json;

namespace dug.Services
{
    public class ConsoleTemplateService : IConsoleTemplateService
    {
        public void DrawResults(Dictionary<DnsServer, List<DnsResponse>> results, RunOptions options)
        {
            switch(options.OutputFormat){
                case OutputFormats.CSV:
                    DrawCsvResults(results, options);
                    break;
                case OutputFormats.JSON:
                    DrawJsonResults(results, options);
                    break;
                default:
                    throw new Exception($"Unable to render results in specified format {options.OutputFormat}");
            }
        }

        private void DrawJsonResults(Dictionary<DnsServer, List<DnsResponse>> results, RunOptions options)
        {
            var headers = options.Template.ToLowerInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries);

            var dynamicResults = new List<dynamic>();

            foreach(var pair in results){
                var server = pair.Key;
                var responses = pair.Value;
                foreach(var response in responses){
                    dynamic expando = new System.Dynamic.ExpandoObject();
                    
                    foreach(string header in headers)
                    {
                        if(!TemplateHelper.ResponseGetterMap.ContainsKey(header)){
                            throw new Exception($"Unable to determine how to resolved specified header: {header}");
                        }
                        
                        try{
                            object data = TemplateHelper.ResponseGetterMap[header](KeyValuePair.Create(server, response));
                            ((IDictionary<String, Object>)expando).Add(header, data);
                        }
                        catch{
                            // Most of the time this means the data isnt present (like if they want errorcode but there was no error)
                        }
                    }
                    dynamicResults.Add(expando);
                }
            }

            var jsonResult = JsonSerializer.Serialize(dynamicResults, new JsonSerializerOptions {WriteIndented = true});
            Console.WriteLine(jsonResult);
        }

        private void DrawCsvResults(Dictionary<DnsServer, List<DnsResponse>> results, RunOptions options)
        {
            var headers = options.Template.ToLowerInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries);

            var csvResults = new List<string>();

            foreach(var pair in results){
                var server = pair.Key;
                var responses = pair.Value;
                foreach(var response in responses){
                    List<string> responseResults = new List<string>();
                    foreach(string header in headers)
                    {
                        if(!TemplateHelper.ResponseGetterMap.ContainsKey(header)){
                            throw new Exception($"Unable to determine how to resolved specified header: {header}");
                        }
                        try{
                        string dataString = TemplateHelper.ResponseGetterMap[header](KeyValuePair.Create(server, response)).ToString();
                        dataString = dataString.Replace(Environment.NewLine, "\\n").Replace(",",string.Empty); //Cant have real newlines or rogue commas in the csv output...
                        responseResults.Add(dataString);
                        }
                        catch{
                            responseResults.Add(string.Empty);
                            // Most of the time this means the data isnt present (like if they want errorcode but there was no error)
                        }
                    }
                    csvResults.Add(string.Join(',' , responseResults));
                }
            }

            foreach(var result in csvResults){
                Console.WriteLine(result);
            }
        }
    }
}