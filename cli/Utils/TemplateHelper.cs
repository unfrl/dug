using System;
using System.Collections.Generic;
using System.Net;
using dug.Data;
using dug.Data.Models;

namespace dug.Utils
{
    public class TemplateHelper
    {
        // Contains headers mapped to a function that can retrieve their property from a DnsServer
        public static Dictionary<string, Action<DnsServer, string>> ServerSetterMap = new Dictionary<string, Action<DnsServer, string>>{
            {"ipaddress", (server, value) => server.IPAddress = IPAddress.Parse(value)},
            {"countrycode", (server, value) => server.CountryCode = value},
            {"city", (server, value) => server.City = value},
            {"dnssec", (server, value) => server.DNSSEC = bool.Parse(value)},
            {"reliability", (server, value) => server.Reliability = double.Parse(value)},
            {"ignore", (server, value) => {}}
        };

        // Contains headers mapped to a function that can retrieve their value from a KeyValuePair<DnsServer, DnsResponse>
        public static Dictionary<string, Func<KeyValuePair<DnsServer, DnsResponse>,object>> ResponseGetterMap = new Dictionary<string,Func<KeyValuePair<DnsServer, DnsResponse>,object>> {
            {"ipaddress", pair => pair.Key.IPAddress.ToString()},
            {"countrycode", pair => pair.Key.CountryCode},
            {"city", pair => pair.Key.City},
            {"dnssec", pair => pair.Key.DNSSEC},
            {"reliability", pair => pair.Key.Reliability},
            {"continentcode", pair => pair.Key.ContinentCode},
            {"countryname", pair => pair.Key.CountryName},
            {"countryflag", pair => pair.Key.CountryFlag},
            {"citycountryname", pair => pair.Key.CityCountryName},
            {"citycountrycontinentname", pair => pair.Key.CityCountryContinentName},
            {"responsetime", pair => pair.Value.ResponseTime},
            {"recordtype", pair => pair.Value.RecordType.ToString()},
            {"haserror", pair => pair.Value.HasError},
            {"errormessage", pair => pair.Value.Error.Message},
            {"errorcode", pair => pair.Value.Error.Code.ToString()},
            {"value", pair => { return GetAnswersString(pair.Value); } },
        };

        /*
            From a DnsResponse get a string representing the answers it provided with the TTLs removed.
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