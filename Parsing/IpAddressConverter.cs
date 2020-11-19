using System;
using System.Net;
using TinyCsvParser.TypeConverter;

namespace dug.Parsing
{
    internal class IpAddressConverter : ITypeConverter<IPAddress>
    {
        public Type TargetType => typeof(IPAddress);

        public bool TryConvert(string value, out IPAddress result)
        {
            return IPAddress.TryParse(value, out result);
        }
    }
}