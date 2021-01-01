using System;
using System.Collections.Generic;
using dug.Data;
using dug.Data.Models;

namespace dug.Utils
{
    public class TemplateHelper
    {
        public static Dictionary<string, Func<KeyValuePair<DnsServer, DnsResponse>,object>> TemplateHeaderMap = new Dictionary<string,Func<KeyValuePair<DnsServer, DnsResponse>,object>> {
            {"IPAddress", pair => pair.Key.IPAddress.ToString()},
            {"ResponseTime", pair => pair.Value.ResponseTime},
            {"Value", pair => { return GetAnswersString(pair.Value); } },
        };

        /*
            From a DnsResponse get a string reprisenting the answers it provided with the TTLs removed.
        */
        public static string GetAnswersString(DnsResponse response){
            if(response.HasError){
                return response.Error.Code.ToString(); //TODO: Might be nice to make these prettier someday
            }

            var records = response.QueryResponse.Answers;
            if(records.Count == 0){
                return "Empty";
            }

            List<string> recordStrings = new List<string>();
            foreach(var record in records){
                var recordString = record.ToString();
                var ttlStart = recordString.IndexOf(' ');
                var ttlEnd = recordString.IndexOf(' ', ttlStart+1);
                recordString = recordString.Remove(ttlStart, ttlEnd-ttlStart);
                recordStrings.Add(recordString);
            }
            recordStrings.Sort();
            return string.Join(System.Environment.NewLine, recordStrings);
        }
    }
}