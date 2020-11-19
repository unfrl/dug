using System;
using System.ComponentModel.DataAnnotations;
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

        public string ToCsvString(){
            return $"{IPAddress.ToString()},{CountryCode},{City},{DNSSEC},{Reliability}";
        }
    }
}