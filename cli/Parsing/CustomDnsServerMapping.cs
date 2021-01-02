using System;
using dug.Data.Models;
using dug.Utils;
using TinyCsvParser.Mapping;
using TinyCsvParser.Model;

namespace dug.Parsing
{
    public class CustomDnsServerMapping : CsvMapping<DnsServer>
    {
        private readonly string[] _customHeaders;
        public CustomDnsServerMapping(string customHeaders) : base()
        {
            _customHeaders = customHeaders.Split(',', StringSplitOptions.None);//Specifically DO NOT remove empty entries
            MapUsing(MapCustomHeaders);
        }

        private bool MapCustomHeaders(DnsServer server, TokenizedRow headers)
        {
            for(int headerIndex = 0; headerIndex < _customHeaders.Length; headerIndex++){
                string headerName = _customHeaders[headerIndex];
                string headerValue = headers.Tokens[headerIndex]; //TODO: Handle if this is missing!
                var setterFunction = TemplateHelper.ServerSetterMap[headerName]; //TODO: Handle if this is missing!

                setterFunction(server, headerValue);
            }

            return true;
        }
    }
}