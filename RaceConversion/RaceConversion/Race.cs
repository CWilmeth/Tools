using System;
using System.Collections.Generic;
using System.Linq;

namespace IMS.EMR.Core.Model.Enumerations
{
    [Serializable]
    public class Race : HL7Enum<Race>
    {
        public Race() { }

        private Race(int value, string name, string mnemonic, int bitCode)
            : base(value, name, mnemonic, string.Empty)
        {
            BitCode = bitCode;
            OID = "2.16.840.1.113883.6.238";
        }

        public int BitCode { get; set; }

        internal static int GetBitCode(List<Race> races)
        {
            if (races == null)
                return 0;
            else
            {
                var b = races.Sum(x => x.BitCode);
                return b;
            }
        }

        internal static List<Race> GetRaces(int bitCode)
        {
            var r = GetAll().Where(x => (bitCode & x.BitCode) == x.BitCode);
            return r.Except(Race.None).ToList();
        }

        public static string GetRaceNames(IEnumerable<Race> races)
        {
            if (races == null)
                return string.Empty;
            else
            {
                var r = races.Where(x => x != Race.None);
                if (r.Any())
                    return string.Join(", ", r.Select(x => x.Name));
                else
                    return string.Empty;
            }
        }


        public static readonly Race None = new Race(0, "None", string.Empty, 0);
        public static readonly Race Caucasian = new Race(1, "White", "2106-3", 1);
        public static readonly Race AfricanAmerican = new Race(2, "Black or African American", "2054-5", 2);
        public static readonly Race AmericanIndian = new Race(3, "American Indian or Alaska Native", "1002-5", 4);
        public static readonly Race Eskimo = new Race(4, "Eskimo", "1840-8", 8);
        public static readonly Race Hispanic = new Race(5, "Hispanic or Latino", "2135-2", 16);
        //public static readonly Race MixedRace = new Race(6, "Mixed Race", 32);
        public static readonly Race Asian = new Race(7, "Asian", "2028-9", 64);
        public static readonly Race Unknown = new Race(8, "Unknown", string.Empty, 128);
        public static readonly Race NativeHawaiian = new Race(9, "Native Hawaiian or Other Pacific Islander", "2076-8", 256);
        public static readonly Race Blank = new Race(10, string.Empty, string.Empty, 512);
        public static readonly Race DeclinedToSpecify = new Race(11, "Declined to Specify", "dcl", 1024);
    }
}

