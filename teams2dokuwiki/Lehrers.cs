﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace teams2dokuwiki
{
    public class Lehrers : List<Lehrer>
    {   
        public Lehrers()
        {
        }

        public Lehrers(int periode, Raums raums, int aktSj)
        {
            Anrechnungs anrechnungen = new Anrechnungs(periode);

            using (SqlConnection odbcConnection = new SqlConnection(Global.ConnectionStringUntis))
            {
                try
                {
                    string queryString = @"SELECT DISTINCT 
Teacher.Teacher_ID, 
Teacher.Name,
Teacher.Longname, 
Teacher.FirstName,
Teacher.Email,
Teacher.ROOM_ID,
Teacher.Title,
 Teacher.PlannedWeek,
 Teacher.Flags,
Teacher.BirthDate
 FROM Teacher
WHERE (((SCHOOLYEAR_ID)= " + Global.AktSj[0] + Global.AktSj[1] + ") AND  ((TERM_ID)=" + periode + ") AND ((Teacher.SCHOOL_ID)=177659) AND (((Teacher.Deleted)='false'))) ORDER BY Teacher.Name;";

                    SqlCommand odbcCommand = new SqlCommand(queryString, odbcConnection);
                    odbcConnection.Open();
                    SqlDataReader sqlDataReader = odbcCommand.ExecuteReader();

                    while (sqlDataReader.Read())
                    {
                        Lehrer lehrer = new Lehrer();

                        lehrer.IdUntis = sqlDataReader.GetInt32(0);
                        lehrer.Kürzel = Global.SafeGetString(sqlDataReader, 1);
                        lehrer.Nachname = Global.SafeGetString(sqlDataReader, 2);
                        if (lehrer.Nachname != "")
                        {
                            try
                            {
                                lehrer.Vorname = Global.SafeGetString(sqlDataReader, 3);
                                lehrer.Mail = Global.SafeGetString(sqlDataReader, 4);
                                lehrer.Raum = (from r in raums where r.IdUntis == sqlDataReader.GetInt32(5) select r.Raumnummer).FirstOrDefault();
                                lehrer.Titel = Global.SafeGetString(sqlDataReader, 6);
                                lehrer.Deputat = Convert.ToDouble(sqlDataReader.GetInt32(7)) / 1000;
                                lehrer.Geschlecht = Global.SafeGetString(sqlDataReader, 8).Contains("W") ? "w" : "m";
                                lehrer.Anrechnungen = (from a in anrechnungen where a.TeacherIdUntis == sqlDataReader.GetInt32(0) select a).ToList();
                                try
                                {
                                    lehrer.Geburtsdatum = DateTime.ParseExact(sqlDataReader.GetInt32(9).ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
                                }
                                catch (Exception)
                                {
                                    throw new Exception("Kein Geburtsdatum");
                                }

                                lehrer.AlterAmErstenSchultagDiesesJahres = lehrer.GetAlterAmErstenSchultagDiesesJahres(aktSj);
                                lehrer.ProzentStelle = lehrer.GetProzentStelle();
                                lehrer.AusgeschütteteAltersermäßigung = (from a in lehrer.Anrechnungen where a.Grund == 200 select a.Wert).FirstOrDefault();
                                lehrer.CheckAltersermäßigung();
                                this.Add(lehrer);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Fehler bei Lehrer " + lehrer.Nachname + ": " + ex);  
                            }                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw new Exception(ex.ToString());
                }
                finally
                {
                    odbcConnection.Close();
                    Global.WriteLine("Lehrers", this.Count);
                }
            }
        }

        

        public List<Lehrer> Referendare()
        {
            //var members = new List<string>();
            List<Lehrer> members = new List<Lehrer>();

            foreach (var lehrer in this)
            {
                if (lehrer.Kürzel.StartsWith("Y"))
                {
                    if (!members.Contains(lehrer))
                    {
                        members.Add(lehrer);
                    }
                }
            }
            return members;
        }

        internal List<Lehrer> Kollegium()
        {
            var members = new List<Lehrer>();

            foreach (var lehrer in this.OrderBy(x => x.Nachname).ThenBy(x => x.Vorname))
            {
                //                if (!members.Contains(lehrer.Mail))
                if (!members.Contains(lehrer))
                {
                    //members.Add(lehrer.Mail);
                    members.Add(lehrer);
                }
            }
            return members;
        }

        internal void DateiAnrechnungenErzeugen(Klasses klasses)
        {
            string dateiAnrechnungen = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\\Anrechnung.txt";

            if (File.Exists(dateiAnrechnungen))
            {
                File.Delete(dateiAnrechnungen);
            }

            string anrechnungstring = "";

            File.WriteAllText(dateiAnrechnungen, "  Stand: " + DateTime.Now.ToShortDateString() + ", " + DateTime.Now.ToShortTimeString() + " Uhr" + Environment.NewLine);
            File.AppendAllText(dateiAnrechnungen, Environment.NewLine);
            File.AppendAllText(dateiAnrechnungen, "^Name^Grund^Wert^Von^Bis^" + Environment.NewLine);
            double summe = 0;

            foreach (var lehrer in this.OrderBy(x => x.Nachname))
            {
                foreach (var anrechnung in lehrer.Anrechnungen)
                {
                    if (anrechnung.Grund == 500 && anrechnung.Wert > 0)
                    {
                        string namensraum = ":" + anrechnung.Text;
                        string name = anrechnung.Text;

                        if (anrechnung.Beschr.Contains("Bildungsg"))
                        {
                            var xxx = (from k in klasses where anrechnung.Text.Contains(k.BildungsgangLangname) select k.Stufe).FirstOrDefault();

                            namensraum = "";
                            if (xxx.StartsWith("BS"))
                            {
                                namensraum = "bildungsgaenge:berufsschule";
                            }
                            if (xxx.StartsWith("AV") || xxx.StartsWith("IFK"))
                            {
                                namensraum = "bildungsgaenge:ausbildungsvorbereitung";
                            }
                            if (xxx.StartsWith("FOS"))
                            {
                                namensraum = "bildungsgaenge:fachoberschule";
                            }
                            if (xxx.StartsWith("GY"))
                            {
                                namensraum = "bildungsgaenge:berufliches_gymnasium";
                            }
                            if (xxx.StartsWith("BAB") || xxx.StartsWith("BK/HSA") || xxx.StartsWith("BFS"))
                            {
                                namensraum = "bildungsgaenge:berufsfachschule";
                            }
                        }
                        else
                        {
                            name = anrechnung.Text;
                            namensraum = ":" + anrechnung.Text;//.Replace(" ", "-");

                            if (name.Contains("("))
                            {
                                int index = name.IndexOf("(");
                                name = (name.Substring(0, index)).TrimEnd();
                            }
                            if (namensraum.Contains("("))
                            {
                                int index = namensraum.IndexOf("(");
                                namensraum = (namensraum.Substring(0, index)).TrimEnd();
                            }
                        }

                        anrechnungstring += "|[[chat>" + lehrer.Mail.Replace("@berufskolleg-borken.de", "") + "|" + lehrer.Nachname + ", " + lehrer.Vorname + "]]|[[" + namensraum + "|" + name + "]]|  " + anrechnung.Wert + "|" + (anrechnung.Von.Year == 1 ? "" : anrechnung.Von.ToShortDateString()) + " |" + (anrechnung.Bis.Year == 1 ? "" : anrechnung.Bis.ToShortDateString()) + " |" + Environment.NewLine;
                        summe += anrechnung.Wert;
                    }
                }
            }
            anrechnungstring += "| |  Summe:|  " + summe.ToString() + "| | |";

            File.AppendAllText(dateiAnrechnungen, anrechnungstring + Environment.NewLine);
            Process.Start("notepad++.exe", dateiAnrechnungen);
        }

        internal void KollegiumDateiErzeugen(Unterrichts unterrichts, Klasses klasses)
        {
            string dateiKollegium = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\\Kollegium.txt";

            if (File.Exists(dateiKollegium))
            {
                File.Delete(dateiKollegium);
            }
            File.WriteAllText(dateiKollegium, "====== Kollegium ======" + Environment.NewLine);
            File.AppendAllText(dateiKollegium, Environment.NewLine);
            File.AppendAllText(dateiKollegium, "  Stand: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + Environment.NewLine);
            File.AppendAllText(dateiKollegium, Environment.NewLine);
            File.AppendAllText(dateiKollegium, "Siehe auch [[kollegium:gruppen|Gruppen & Mitglieder]]." + Environment.NewLine);

            File.AppendAllText(dateiKollegium, Environment.NewLine);
            File.AppendAllText(dateiKollegium, "^  Foto  ^  Name  ^ ^" + Environment.NewLine);

            foreach (var lehrer in this.OrderBy(x => x.Nachname))
            {
                string aufgaben = "";
                string klassenleitungen = "";
                string faecher = "";
                string bildungsgaenge = "";

                // Auf Fächer prüfen

                var fachss = new List<string>();

                if ((from u in unterrichts where u.LehrerKürzel == lehrer.Kürzel select u).Any())
                {
                    faecher = @"**Fächer:** "; ;

                    foreach (var unterricht in unterrichts)
                    {
                        if (lehrer.Kürzel == unterricht.LehrerKürzel)
                        {
                            if (!fachss.Contains(unterricht.FachKürzel + "|" + lehrer.Kürzel))
                            {
                                fachss.Add(unterricht.FachKürzel + "|" + lehrer.Kürzel);
                                faecher += unterricht.FachKürzel + ", ";
                            }
                        }
                    }

                    faecher = faecher.TrimEnd();
                    faecher = faecher.TrimEnd(',');
                    faecher = faecher + @"\\ ";
                }

                // Auf Bildungsgänge prüfen

                if ((from u in unterrichts where u.LehrerKürzel == lehrer.Kürzel select u).Any())
                {
                    bildungsgaenge = @"**Bildungsgänge:** "; ;

                    var bgs = new List<string>();

                    foreach (var unterricht in (from u in unterrichts.OrderBy(x => x.LehrerKürzel) select u))
                    {
                        if (!(new List<string>() { "LRat", "BTeam", "EDV" }.Contains(unterricht.KlasseKürzel)))
                        {
                            if (lehrer.Kürzel == unterricht.LehrerKürzel)
                            {
                                string result = Regex.Match(unterricht.KlasseKürzel, @"^[^0-9]*").Value;

                                if (!bgs.Contains(result + "|" + lehrer.Kürzel))
                                {
                                    bgs.Add(result + "|" + lehrer.Kürzel);

                                    var text = (from k in klasses where Regex.Match(k.NameUntis, @"^[^0-9]*").Value == result select k.Text).FirstOrDefault();

                                    if (result.ToLower() == "agg" || result.ToLower() == "ags")
                                    {
                                        result = "agg-ags";
                                    }
                                    bildungsgaenge += "[[:bildungsgaenge:" + text + ":" + result + "|" + result + "]], ";
                                }
                            }
                        }
                    }

                    bildungsgaenge = bildungsgaenge.TrimEnd();
                    bildungsgaenge = bildungsgaenge.TrimEnd(',');
                    bildungsgaenge = bildungsgaenge + @"\\ ";
                }

                // Auf Klassenleitung prüfen

                if ((from k in klasses from kl in k.Klassenleitungen where kl.Kürzel == lehrer.Kürzel select kl).Any())
                {
                    klassenleitungen = @"**Klassenleitungen:** "; ;

                    foreach (var klasse in klasses)
                    {
                        if (!(new List<string>() { "LRat", "BTeam", "EDV" }.Contains(klasse.NameUntis)))
                        {
                            foreach (var klassenleitung in klasse.Klassenleitungen)
                            {
                                if (lehrer.Kürzel == klassenleitung.Kürzel)
                                {
                                    string result = Regex.Match(klasse.NameUntis, @"^[^0-9]*").Value;

                                    if (result.ToLower() == "agg" || result.ToLower() == "ags")
                                    {
                                        result = "agg-ags";
                                    }

                                    klassenleitungen += "[[:bildungsgaenge:" + klasse.Text + ":" + result + "|" + klasse.NameUntis + "]]" + ", ";
                                }
                            }
                        }
                    }

                    klassenleitungen = klassenleitungen.TrimEnd();
                    klassenleitungen = klassenleitungen.TrimEnd(',');
                    klassenleitungen = klassenleitungen + @"\\ ";
                }

                // Auf Anrechnungen prüfen

                if (lehrer.Anrechnungen.Count > 0)
                {
                    aufgaben += @"**Funktion / Aufgabenbereich:** ";

                    foreach (var anrechnung in lehrer.Anrechnungen.OrderBy(x => x.Text))
                    {
                        string wert = "";

                        if (anrechnung.Wert > 0 && anrechnung.Grund == 500)
                        {
                            wert = " (" + anrechnung.Wert.ToString() + " Anrechnungsstunde" + (anrechnung.Wert == 1 ? "" : "n");

                            if (anrechnung.Von.Year > 1)
                            {
                                wert += " ab " + anrechnung.Von.ToShortDateString();
                            }
                            if (anrechnung.Bis.Year > 1)
                            {
                                wert += " bis " + anrechnung.Bis.ToShortDateString();
                            }
                            wert += ")";
                        }

                        // Wiki-Link?

                        var text = anrechnung.TextGekürzt;

                        if (anrechnung.Beschr != "")
                        {
                            text = "[[:" + anrechnung.Beschr + "|" + anrechnung.TextGekürzt + "]]";
                        }

                        aufgaben += " " + text + wert + @",";
                    }

                    aufgaben = aufgaben.TrimEnd();
                    aufgaben = aufgaben.TrimEnd(',');

                }

                File.AppendAllText(dateiKollegium, "|{{:lul:lul-fotos:" + lehrer.Kürzel + ".jpg?nolink&100|}}| **" + (lehrer.Titel != "" ? lehrer.Titel + " " : "") + lehrer.Nachname + ", " + lehrer.Vorname + @"** (" + lehrer.Kürzel + @")\\ [[" + lehrer.Mail + @"]]\\ [[chat>" + lehrer.Mail.Replace("@berufskolleg-borken.de", "") + " | " + lehrer.Vorname + " " + lehrer.Nachname + @"]]|" + bildungsgaenge + faecher + klassenleitungen + aufgaben + "| " + Environment.NewLine);
            }

            File.AppendAllText(dateiKollegium, "{{tag>Zuständigkeiten Personal Klassenleitung}}" + Environment.NewLine);

            Process.Start("notepad++.exe", dateiKollegium);
        }

        internal List<Lehrer> Lehrerinnen()
        {
            List<Lehrer> members = new List<Lehrer>();

            foreach (var lehrer in this)
            {
                if (lehrer.Geschlecht == "w" && lehrer.Deputat > 0)
                {
                    //                    if (!members.Contains(lehrer.Mail))
                    if (!members.Contains(lehrer))
                    {
                        //members.Add(lehrer.Mail);
                        members.Add(lehrer);
                    }
                }
            }
            return members;
        }

        internal List<Lehrer> GetAnrechungenAusBeschreibung(string name)
        {
            List<Lehrer> a = new List<Lehrer>();


            foreach (var lehrer in this)
            {
                var x = lehrer.GetAnrechnungAusBeschreibung(name);
             
                if (x != null)
                {
                    //a.Add(x);
                    var zzzz = (from l in this where l.Mail == x select l).FirstOrDefault();
                    string aa = "";
                    if (!a.Contains(zzzz))
                    {
                        a.Add(zzzz);
                    }
                }
            }

            if (a.Count == 0)
            {
                //a.Add("N.N.");
                //a.Add("N.N.");
            }
            return a;
        }


        internal bool istLehrer(Lehrer istMember)
        {
            foreach (var item in this)
            {
                if (item.Mail == istMember.Mail)
                {
                    return true;
                }
            }
            return false;
        }

        internal IEnumerable<string> Teilzeitkraefte()
        {
            var members = new List<string>();

            foreach (var teilzeitkraft in (from l in this where l.Deputat < 25.5 select l))
            {
                if (!members.Contains(teilzeitkraft.Mail))
                {
                    members.Add(teilzeitkraft.Mail);
                }
            }
            return members;
        }

        internal IEnumerable<string> Vollzeitkraefte()
        {
            var members = new List<string>();

            foreach (var vollzeitkraft in (from l in this where l.Deputat >= 25.5 select l))
            {
                if (!members.Contains(vollzeitkraft.Mail))
                {
                    members.Add(vollzeitkraft.Mail);
                }
            }
            return members;
        }
    }
}
    