using System;
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

        public static ContinentCodes AF   { get { return new ContinentCodes("AF", "Africa"); } }
        public static ContinentCodes SA   { get { return new ContinentCodes("SA", "South America"); } }
        public static ContinentCodes NA   { get { return new ContinentCodes("NA", "North America"); } }
        public static ContinentCodes OC    { get { return new ContinentCodes("OC", "Oceania"); } }
        public static ContinentCodes AS { get { return new ContinentCodes("AS", "Asia"); } }
        public static ContinentCodes EU   { get { return new ContinentCodes("EU", "Europe"); } }
        public static ContinentCodes AN   { get { return new ContinentCodes("AN", "Antarctica"); } }
        public static List<ContinentCodes> Continents { get { return new List<ContinentCodes>() {AF, SA, NA, OC, AS, EU, AN}; } }
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