using System;
using dug.Data.Models;
using dug.Utils;
using TinyCsvParser.Mapping;
using TinyCsvParser.Model;
using System.Linq;

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
                
                string headerValue = headers.Tokens.ElementAtOrDefault(headerIndex);
                if(headerValue == null){
                    throw new Exception(string.Format(i18n.dug.ER_Unable_To_Get_Value_From_Header_X_Expected_At_Index_X, headerName, headerIndex));
                }
                var setterFunction = TemplateHelper.ServerSetterMap[headerName]; // This should always be present, its validation in UpdateOptions.cs

                try{
                    setterFunction(server, headerValue);
                }
                catch{
                    throw new Exception(string.Format(i18n.dug.ER_Unable_Set_Field_X_To_Value_X, headerName, headerValue));
                }
            }

            return true;
        }
    }
}