using System;
using System.Collections.Generic;
using System.Text;

namespace IMS.EMR.Core.Model.Enumerations
{
    [Serializable]
    public class HL7Enum<T> : Enumeration<T> where T : HL7Enum<T>
    {        
        public HL7Enum() { }
        public HL7Enum(int value, string name) : base(value, name) { }
        public HL7Enum(int value, string name, string mnemonic, string definition, int sortOrder = 0)
            : base(value, name, sortOrder)
        {
            Mnemonic = mnemonic;
            Definition = definition;
        }

        public string Mnemonic { get; protected set; }
        public string Definition {get; protected set; }
        public static string OID { get; protected set; }
    }
}
