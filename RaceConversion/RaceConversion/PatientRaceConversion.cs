using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IMS.EMR.Core.Model.Enumerations;
using IMS.Updater.Tools;
using MySql.Data.MySqlClient;
using Tools.Database;

namespace RaceConversion
{
    public class PatientRaceConversion : IUpdateModule
    {
        public DbSettings Settings;

        public bool Go(AppUpdateConfiguration appConfig)
        {
            Settings = new DbSettings()
            {
                DatabaseServer = appConfig.Server,
                Password = appConfig.Password,
                Schema = appConfig.Database,
                User = appConfig.User
            };

            if (!appConfig.UpdatePeripheral)
            {
                var eskimoPatients = new List<ConversionPatient>();
                var hispanicPatients = new List<ConversionPatient>();
                OnProgressChanged("Patient Race Conversion Started");
                using (var db = new Database(Settings))
                {
                    const string sql = "Select id,race_bit_code, EthnicityID, Ethnicity from patients ";
                    using (var dr = db.ExecuteReader(sql))
                    {
                        OnProgressChanged("Getting All Patients With Eskimo Race");
                        while (dr.Read())
                        {
                            var patient = new ConversionPatient
                            {
                                Id = dr.GetString("id"),
                                BitCode = dr.GetInt32("race_bit_code"),
                                Ethnicity = dr.GetString("Ethnicity"),
                                EthnicityId = dr.GetInt32("EthnicityID")
                            };
                            patient.PatientRaces = Race.GetRaces(patient.BitCode);

                            if (patient.PatientRaces.Contains(Race.Eskimo))
                            {
                                eskimoPatients.Add(patient);
                            }

                            else if (patient.PatientRaces.Contains(Race.Hispanic))
                            {
                                hispanicPatients.Add(patient);
                            }

                        }
                    }
                    OnProgressChanged("Done Getting Patients");
                }
                OnProgressChanged("Start Updates");
                if (eskimoPatients.Any())
                {
                    OnProgressChanged("Updating Eskimo Patients Race");
                    UpdateEskimoPatients(eskimoPatients);
                    OnProgressChanged("Updating Eskimo Patients Race Complete");

                    OnProgressChanged("Inserting New Patient Granular Rows");
                    CreateInsertToPatientGranularRaceTableForEskimos(eskimoPatients);
                    OnProgressChanged("Inserting New Patient Granular Rows Complete");

                }

                if (hispanicPatients.Any())
                {
                    OnProgressChanged("Updating Hispanic Patients Race");
                    UpdateHispanicPatients(hispanicPatients);
                    OnProgressChanged("Updating Hispanic Patients Race Complete");

                    OnProgressChanged("Updating Hispanic Patients Ethnicity");
                    UpdateHispanicPatientsEthnicity(hispanicPatients);
                    OnProgressChanged("Updating Hispanic Patients Ethnicity Complete");
                }




                OnProgressChanged("Patient Eskimo Race Conversion Complete" + Environment.NewLine +
                                  "Total Number Of Patients Converted: " + eskimoPatients.Count.ToString());
                OnProgressChanged("Patient Latino Race Conversion Complete" + Environment.NewLine +
                                  "Total Number Of Patients Converted: " + hispanicPatients.Count.ToString());


                return true;
            }

            return false;
        }

        private void UpdateHispanicPatientsEthnicity(List<ConversionPatient> hispanicPatients)
        {
            foreach (var pat in hispanicPatients.Where(pat => pat.EthnicityId == 0 || pat.EthnicityId == 4))
            {
                pat.EthnicityId = 1;
                pat.Ethnicity = "Hispanic Or Latino";
                UpdatePatientEthnicity(pat);
            }
        }

        private void UpdateHispanicPatients(List<ConversionPatient> hispanicPatients)
        {
            foreach (var patient in hispanicPatients)
            {
                patient.BitCode = patient.BitCode - 16;
                if (patient.BitCode == 0)
                {
                    patient.BitCode = 128;
                }
                UpdatePatientRace(patient);
            }
        }

        private void UpdateEskimoPatients(List<ConversionPatient> eskimoPatients)
        {
            foreach (var patient in eskimoPatients)
            {
                patient.BitCode = patient.BitCode - 8;
                if (!patient.PatientRaces.Contains(Race.AmericanIndian))
                {
                    patient.BitCode += 4;
                }
                if (patient.BitCode == 0)
                {
                    patient.BitCode = 4;
                }
                UpdatePatientRace(patient);
            }
        }

        private void UpdatePatientRace(ConversionPatient patient)
        {
            var sql = "Update patients set race_bit_code ='" + patient.BitCode.ToString() + "' Where id ='" + patient.Id + "';";
            using (var db = new Database(Settings))
            {
                db.ExecuteNonQuery(sql);
            }
        }

        private void UpdatePatientEthnicity(ConversionPatient patient)
        {
            var sql = "Update patients set EthnicityID ='1', Ethnicity ='" + patient.Ethnicity + "' Where id ='" + patient.Id + "';";
            using (var db = new Database(Settings))
            {
                db.ExecuteNonQuery(sql);
            }
        }

        private void CreateInsertToPatientGranularRaceTableForEskimos(List<ConversionPatient> eskimoPatients)
        {
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = new StringBuilder();
            sql.Append(
                "INSERT INTO `patient_granular_race` (`granular_race_concept_code`, `race_name`, `race_common_code_concept_code`,`patientID`,`created_by`,`created_on`,`modified_by`,`modified_on`, `Deleted`)" +
                " VALUES");


            foreach (var patient in eskimoPatients)
            {
                using (var db = new Database(Settings))
                {
                    var temp = @"SELECT EXISTS(SELECT 1 FROM patient_granular_race WHERE patientID='" + patient.Id + "' and granular_race_concept_code ='1840-8' LIMIT 1)";
                    var result = db.ExecuteScalar(temp);

                    if (result.ToString() == 0.ToString())
                    {
                        sql.Append("('1840-8','Eskimo','1002-5','" + patient.Id + "','103572','" + time + "','103572','" + time + "','0'),");
                    }
                }
            }

            sql.Length -= 1;
            sql.Append(";");

            using (var db = new Database(Settings))
            {
                db.ExecuteNonQuery(sql.ToString());
            }

        }

        private void OnProgressChanged(string message)
        {
            if (ModuleProgressChanged != null)
                ModuleProgressChanged(message);
        }

        public event ModuleProgressChangedEventHandler ModuleProgressChanged;
    }
}
