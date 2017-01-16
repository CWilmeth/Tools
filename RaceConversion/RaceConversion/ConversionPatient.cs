using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.EMR.Core.Model.Enumerations;

namespace RaceConversion
{
    internal class ConversionPatient
    {
        public string Id { get; set; }
        public int BitCode { get; set; }
        public string Ethnicity { get; set; }
        public int EthnicityId { get; set; }
        public List<Race> PatientRaces { get; set; } 
    }
}
