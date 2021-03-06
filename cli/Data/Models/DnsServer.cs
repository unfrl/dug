using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace dug.Data.Models
{
    public class DnsServer
    {
        [Required]
        public IPAddress IPAddress {get; set;}

        public string CountryCode {get; set;}

        public string City {get; set;}

        public bool? DNSSEC {get; set;} //TODO: Will no value in the server info here actually set this to null?
        
        public double Reliability {get; set;}

        public ContinentCodes ContinentCode {
            get {
                if(string.IsNullOrEmpty(CountryCode)){
                    return ContinentCodes.UNKNOWN;
                }
                return DataMaps.CountryContinentMap.ContainsKey(CountryCode) ? DataMaps.CountryContinentMap[CountryCode] : ContinentCodes.UNKNOWN;
            }
        }

        public string CountryName { 
            get {
                return CountryCode != null && DataMaps.CountryNameMap.ContainsKey(CountryCode) ? DataMaps.CountryNameMap[CountryCode] : null;
            }
        }

        public string CountryFlag {
            get {
                if(CountryCode == null){
                    return null;
                }
                return CountryCode != null && DataMaps.CountryFlagMap.ContainsKey(CountryCode) ? DataMaps.CountryFlagMap[CountryCode] : null;
            }
        }

        public string CityCountryName {
            get {
                string result = string.IsNullOrWhiteSpace(City) ? "" : $"{City}, ";
                result += string.IsNullOrEmpty(CountryName) ? "UNKNOWN COUNTRY 🤷" : CountryName;
                return result;
            }
        }

        public string CityCountryContinentName {
            get {
                string result = $"{CityCountryName}, ";
                result += ContinentCode == null ? "UNKNOWN CONTINENT" : ContinentCode.Name;
                return result;
            }
        }

        public string ToCsvString(){
            // This is using the local csvformat (defined in LocalCsvDnsServerMapping.cs) and IS being used to persist servers.
            // Apparently TinyCsvParser (true to its name) cannot also serialize. Thats fine, i wish it exposed the mappings it has registered though...
            // Keep this format in sync with the one defined in LocalCsvDnsServerMapping!
            return $"{IPAddress.ToString()},{CountryCode},{City},{DNSSEC},{Reliability}";
        }
    }

    public class DnsServerComparer : IEqualityComparer<DnsServer>
    {

        public bool Equals(DnsServer x, DnsServer y)
        {
            return x.IPAddress == y.IPAddress;
        }

        public int GetHashCode([DisallowNull] DnsServer obj)
        {
            return obj.ToCsvString().GetHashCode();
        }
    }
}