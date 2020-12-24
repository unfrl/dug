using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dug.Data;

namespace dug.Utils
{
    public static class MarkupHelper
    {
        /*
            Takes in a string from a DNS Response and decorates it with markup.
            Ex: Messages that are related to a status, like "Empty" or "ConnectionTimeout" are colored
        */
        public static string FormatDnsResponseMarkup(string dnsResponse, string url){
            switch(dnsResponse){
                case "Unassigned":
                case "Empty":
                    return $"[yellow]{dnsResponse}[/]";
                case "ConnectionTimeout":
                    return "[red]Connection Timeout[/]";
                default:
                    return UrlMarkupInPlace(dnsResponse, url);
            }
        }

        /*
            Takes in a string and replaces all occurences of the specified url with markup making it green.
        */
        public static string UrlMarkupInPlace(string value, string url){
            return value.Replace(url, $"[green]{url}[/]");
        }

        /*
            Used for rendering consensus.
            Takes 2 dictionaries both containing keys of continents and values associated with them.
            Returns multiline markup showing each continent with the value from the first dictionary as well as a percentage (colored) showing the value
            for that continent divided by the value in the second dictionary.
        */
        public static string FormatConsensusMarkup(Dictionary<ContinentCodes, int> continentCounts, Dictionary<ContinentCodes, int> continentTotals){
            StringBuilder sb = new StringBuilder();
            int serverCountWithValue = continentCounts.Sum(pair => pair.Value);
            int serverCountOverall = continentTotals.Sum(pair => pair.Value);
            foreach(ContinentCodes continent in continentCounts.Keys){

                float continentConsensusPercentage = (float)continentCounts[continent]/(float)continentTotals[continent];
                sb.AppendLine($"{continent.Name} ({continentCounts[continent]}) {FormatPercentageMarkup(continentConsensusPercentage)}");
            }

            sb.AppendLine($"[b][u]Overall[/] ({serverCountWithValue} out of {serverCountOverall}) {FormatPercentageMarkup((float)serverCountWithValue/(float)serverCountOverall)}[/]");
            return sb.ToString();
        }

        /*
            Returns markup coloring the provided percentage.
            Anything <= 0.4 is red
            Anything between 0.4-0.8 is orange
            Anything between 0.8-1.0 is yellow
            Anything >= 1.0 (100%) is green
        */
        public static string FormatPercentageMarkup(double percentage){
            switch(percentage){
                case double p when (p >= 1.0):
                    return $"[green]{p.ToString("P0")}[/]";
                case double p when (p >= 0.8 && p < 1.0):
                    return $"[yellow]{p.ToString("P0")}[/]";
                case double p when (p >= 0.4 && p < 0.8):
                    return $"[#FFA500]{p.ToString("P0")}[/]";
                default:
                    return $"[red]{percentage.ToString("P0")}[/]";
            }
        }
    }
}