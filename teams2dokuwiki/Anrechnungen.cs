using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace teams2dokuwiki
{
    public class Anrechnungs : List<Anrechnung>
    {
        public Anrechnungs(int periode)
        {
            using (SqlConnection odbcConnection = new SqlConnection(Global.ConnectionStringUntis))
            {
                Beschreibungs beschreibungs = new Beschreibungs();

                CVReasons cvreasons = new CVReasons();

                try
                {
                    string queryString = @"
SELECT 
CV_REASON_ID, 
Name, 
Longname
FROM CV_Reason
WHERE (SCHOOLYEAR_ID=" + Global.AktSj[0] + Global.AktSj[1] + @");";

                    SqlCommand odbcCommand = new SqlCommand(queryString, odbcConnection);
                    odbcConnection.Open();
                    SqlDataReader sqlDataReader = odbcCommand.ExecuteReader();

                    while (sqlDataReader.Read())
                    {
                        CVReason cvreason = new CVReason()
                        {
                            Id = sqlDataReader.GetInt32(0),
                            Name = Global.SafeGetString(sqlDataReader, 1),
                            Langname = Global.SafeGetString(sqlDataReader, 2)
                        };

                        cvreasons.Add(cvreason);

                    };
                    sqlDataReader.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    odbcConnection.Close();
                }


                try
                {
                    string queryString = @"SELECT 
DESCRIPTION_ID, 
Name, 
Longname
FROM Description
WHERE (SCHOOLYEAR_ID=" + Global.AktSj[0] + Global.AktSj[1] + @");
";
                    SqlCommand odbcCommand = new SqlCommand(queryString, odbcConnection);
                    odbcConnection.Open();
                    SqlDataReader sqlDataReader = odbcCommand.ExecuteReader();

                    while (sqlDataReader.Read())
                    {
                        Beschreibung beschreibung = new Beschreibung()
                        {
                            BeschreibungId = sqlDataReader.GetInt32(0),
                            Name = Global.SafeGetString(sqlDataReader, 1),
                            Langname = Global.SafeGetString(sqlDataReader, 2)                            
                        };
                                                
                        beschreibungs.Add(beschreibung);
                        
                    };
                    sqlDataReader.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    odbcConnection.Close();
                }


                try
                {
                    string queryString = @"SELECT 
CountValue.TEACHER_ID,  
DESCRIPTION_ID, 
CountValue.Text,
CountValue.Value,
CountValue.DateFrom,
CountValue.DateTo,
CountValue.CV_REASON_ID

FROM CountValue
WHERE (((CountValue.SCHOOLYEAR_ID)=" + Global.AktSj[0] + Global.AktSj[1] + @") AND ((CountValue.Deleted)='false') AND ((CountValue.Deleted)='false'))
ORDER BY CountValue.TEACHER_ID;
";

                    SqlCommand odbcCommand = new SqlCommand(queryString, odbcConnection);
                    odbcConnection.Open();
                    SqlDataReader sqlDataReader = odbcCommand.ExecuteReader();

                    while (sqlDataReader.Read())
                    {

                        var sss = sqlDataReader.GetInt32(6);


                        Anrechnung anrechnung = new Anrechnung()
                        {
                            TeacherIdUntis = sqlDataReader.GetInt32(0),
                            Beschr = (from b in beschreibungs where b.BeschreibungId == sqlDataReader.GetInt32(1) select b.Name).FirstOrDefault() == null ? "" : (from b in beschreibungs where b.BeschreibungId == sqlDataReader.GetInt32(1) select b.Name).FirstOrDefault(),  // Wiki-URL                            
                            Text = Global.SafeGetString(sqlDataReader, 2) == null ? "" : Global.SafeGetString(sqlDataReader, 2), // Vorsitz etc.                            
                            Wert = Convert.ToDouble(sqlDataReader.GetInt32(3)) / 100000,
                            Von = sqlDataReader.GetInt32(4) > 0 ? DateTime.ParseExact((sqlDataReader.GetInt32(4)).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture) : new DateTime(),
                            Bis = sqlDataReader.GetInt32(5) > 0 ? DateTime.ParseExact((sqlDataReader.GetInt32(5)).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture) : new DateTime(),
                            Grund = Convert.ToInt32((from c in cvreasons where c.Id == sqlDataReader.GetInt32(6) select c.Name).FirstOrDefault())
                        };

                        if (anrechnung.Text.Contains("("))
                        {
                            anrechnung.TextGekürzt = anrechnung.Text.Substring(0, anrechnung.Text.IndexOf('(')).Trim();
                        }
                        else
                        {
                            anrechnung.TextGekürzt = anrechnung.Text.Trim();
                        }
                        
                        if (anrechnung.TeacherIdUntis != 0 && !(from t in this                              
                             where t.TeacherIdUntis == anrechnung.TeacherIdUntis                              
                             where t.Text == anrechnung.Text                             where t.Beschr == anrechnung.Beschr
                             select t).Any())
                        {                            
                            if (!(anrechnung.Text.Contains("euergruppe") && anrechnung.Beschr.Contains("ildungsgangl")))
                            {
                                if (anrechnung.Grund > 210 || anrechnung.Grund == 200) // Schwerbehinderung etc. nicht einlesen
                                {   
                                    this.Add(anrechnung);
                                }                                
                            }                            
                        }                            
                    };
                    sqlDataReader.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    odbcConnection.Close();
                }
            }
        }
    }
}