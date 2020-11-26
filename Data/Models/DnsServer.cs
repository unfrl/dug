using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace dug.Data.Models
{
    public class DnsServer
    {
        public Guid DnsServerId {get; set;}

        [Required]
        public IPAddress IPAddress {get; set;}

        public string CountryCode {get; set;}

        public string City {get; set;}

        public bool DNSSEC {get; set;}
        
        public double Reliability {get; set;}

        public ContinentCodes ContinentCode { get { return DataMaps.CountryContinentMap[CountryCode]; } }

        public string ToCsvString(){
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