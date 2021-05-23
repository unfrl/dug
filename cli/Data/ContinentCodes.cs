using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace dug.Data
{
    public class ContinentCodes
    {
        private ContinentCodes(string code, string name) { Code = code; Name = name; }

        public string Code { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Code;
        }

        public static ContinentCodes AF   { get { return new ContinentCodes("AF", i18n.dug.Africa); } }
        public static ContinentCodes SA   { get { return new ContinentCodes("SA", i18n.dug.South_America); } }
        public static ContinentCodes NA   { get { return new ContinentCodes("NA", i18n.dug.North_America); } }
        public static ContinentCodes OC    { get { return new ContinentCodes("OC", i18n.dug.Oceania); } }
        public static ContinentCodes AS { get { return new ContinentCodes("AS", i18n.dug.Asia); } }
        public static ContinentCodes EU   { get { return new ContinentCodes("EU", i18n.dug.Europe); } }
        public static ContinentCodes AN   { get { return new ContinentCodes("AN", i18n.dug.Antarctica); } }
        public static ContinentCodes UNKNOWN   { get { return new ContinentCodes("UNKNOWN", i18n.dug.Unknown); } }
        public static List<ContinentCodes> Continents { get { return new List<ContinentCodes>() {AF, SA, NA, OC, AS, EU, AN, UNKNOWN}; } }

        public static bool TryParse(string value, out ContinentCodes result){
            ContinentCodes parsedContinent = Continents.FirstOrDefault(c => c.Code == value || c.Name == value);
            if(parsedContinent != null){
                result = parsedContinent;
                return true;
            }

            result = null;
            return false;
        }
    }

    public class ContinentCodeComparer : IEqualityComparer<ContinentCodes>
    {
        public bool Equals(ContinentCodes x, ContinentCodes y)
        {
            return x.Code == y.Code;
        }

        public int GetHashCode([DisallowNull] ContinentCodes obj)
        {
            return obj.Code.GetHashCode();
        }
    }
}