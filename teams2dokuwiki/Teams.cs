using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace teams2dokuwiki
{
    public class Teams : List<Team>
    {
        private Anrechnungs anrechnungen;
                
        public Teams(string pfad, Klasses klasses, Lehrers lehrers)
        {
            using (StreamReader reader = new StreamReader(pfad))
            {
                string currentLine;

                reader.ReadLine();

                while ((currentLine = reader.ReadLine()) != null)
                {
                    List<string> zeile = new List<string>();

                    zeile.AddRange(currentLine.Replace("\"", "").Replace("\\", "").Split('|'));

                    if (zeile.Count != 5)
                    {
                        Console.WriteLine("Fehler in der Datei " + pfad + ". Falsche Spaltenzahl.");
                        Console.ReadKey();
                    }

                    var x = (from t in this where t.DisplayName == zeile[1] where t.TeamId == zeile[0] select t).FirstOrDefault();

                    if (x == null)
                    {
                        Team teamIst = new Team();

                        teamIst.TeamId = zeile[0];
                        teamIst.DisplayName = zeile[1];
                        teamIst.Owners = new List<Lehrer>();
                        teamIst.Members = new List<Lehrer>();
                        teamIst.Typ = zeile[4];
                        teamIst.Kategorie = (from k in klasses where k.NameUntis + "-LuL" == teamIst.DisplayName select k).Any() ? "Klasse" : "";

                        if (zeile[3] == "Owner")
                        {
                            teamIst.Owners.Add((from l in lehrers where l.Mail == zeile[2] select l).FirstOrDefault());
                        }
                        else
                        {
                            teamIst.Members.Add((from l in lehrers where l.Mail == zeile[2] select l).FirstOrDefault());
                        }
                        this.Add(teamIst);
                    }
                    else
                    {
                        if (zeile[3] == "Owner")
                        {
                            x.Owners.Add((from l in lehrers where l.Mail == zeile[2] select l).FirstOrDefault());
                        }
                        else
                        {
                            x.Members.Add((from l in lehrers where l.Mail == zeile[2] select l).FirstOrDefault());
                        }
                    }
                }
            }

            Global.WriteLine("vorhandene Teams", this.Count);
            if (this.Count == 0)
            {
                throw new Exception("Es ist kein einziges IstTeam vorhanden. Evtl. ist die Datei mit den Gruppenzugehörigkeiten leer.");
            }
            Global.WriteLine(" davon O365", (from t in this where t.Typ == "O365" select t).Count());
            Global.WriteLine(" davon Verteilergruppen", (from t in this where t.Typ == "Distribution" select t).Count());
        }

        public Teams(Klasses klasses, Lehrers lehrers, Schuelers schuelers, Unterrichts unterrichts)
        {
            foreach (var klasse in (from k in klasses where (k.Klassenleitungen != null && k.Klassenleitungen.Count > 0 && k.Klassenleitungen[0] != null && k.NameUntis.Any(char.IsDigit)) select k))
            {
                Team klassenteamSoll = new Team(klasse.NameUntis, "Klasse");
                klassenteamSoll.BildungsgangLanganme = klasse.BildungsgangLangname;
                klassenteamSoll.BildungsgangGekürzt = klasse.BildungsgangGekürzt;
                var klassenleitungMail = (from k in klasses where k.NameUntis == klassenteamSoll.DisplayName select k.Klassenleitungen[0].Mail).FirstOrDefault();
                klassenteamSoll.Klassenleitung = (from l in lehrers where l.Mail == klassenleitungMail select l).FirstOrDefault();

                klassenteamSoll.Unterrichts.AddRange((from u in unterrichts where u.KlasseKürzel == klasse.NameUntis select u).ToList());

                Lehrers unterrL = new Lehrers();

                foreach (var unterricht in unterrichts)
                {
                    if (unterricht.KlasseKürzel == klasse.NameUntis)
                    {
                        if ((from u in unterrL where u.Kürzel == unterricht.LehrerKürzel select u).Any())
                        {
                            var ll = (from l in lehrers where l.Kürzel == unterricht.LehrerKürzel select l).FirstOrDefault();

                            unterrL.Add(ll);
                        }
                    }
                }

                var unterrichtendeLehrer = (from l in lehrers
                                            where (from u in unterrichts where u.KlasseKürzel.Split(',').Contains(klasse.NameUntis) select u.LehrerKürzel).ToList().Contains(l.Kürzel)
                                            where l.Mail != null
                                            where l.Mail != ""
                                            select l).ToList();

                foreach (var unterrichtenderLehrer in unterrichtendeLehrer)
                {
                    if (!klassenteamSoll.Members.Contains(unterrichtenderLehrer))
                    {
                        klassenteamSoll.Members.Add(unterrichtenderLehrer); // Owner müssen immer auch member sein.
                    }
                    if (!klassenteamSoll.Owners.Contains(unterrichtenderLehrer))
                    {
                        klassenteamSoll.Owners.Add(unterrichtenderLehrer);
                    }
                }

                klassenteamSoll.Schuelers = (from s in schuelers
                                             where s.Klasse == klasse.NameUntis
                                             select s).ToList();

                klassenteamSoll.Stufe = klasse.Stufe;
                klassenteamSoll.Text = klasse.Text;

                if (klassenteamSoll.Members.Count() + klassenteamSoll.Owners.Count() > 0)
                {
                    this.Add(klassenteamSoll);
                }
            }

            Global.WriteLine("Klassenteams Soll", this.Count);
            Console.WriteLine("");
        }

        internal void DateiPraktiumErzeugen()
        {
            string dateiPraktikum = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\\Praktikum.txt";

            if (File.Exists(dateiPraktikum))
            {
                File.Delete(dateiPraktikum);
            }

            string schuelerName = "";

            File.WriteAllText(dateiPraktikum, "====== Praktikumsbetreuung ======" + Environment.NewLine);

            foreach (var teamSoll in (from t in Global.TeamsSoll where t.Beschreibung != null && t.Beschreibung.Contains("assenteam") select t).ToList())
            {
                // nur relevante Klassen

                if (
teamSoll.DisplayName.EndsWith("LuL") &&
(teamSoll.DisplayName.StartsWith("BT") ||
teamSoll.DisplayName.StartsWith("BW") ||
teamSoll.DisplayName.StartsWith("BS") ||
teamSoll.DisplayName.StartsWith("IFK") ||
teamSoll.DisplayName.StartsWith("G") ||
teamSoll.DisplayName.StartsWith("HB")))
                {
                    schuelerName += "" + Environment.NewLine;
                    schuelerName += "===== Praktikumsbetreuung " + teamSoll.DisplayName.Replace("-LuL", "") + " =====" + Environment.NewLine;
                    schuelerName += "^Nr. ^Schüler*in               ^Betrieb/Kontaktdaten^Lehrkraft Name^" + Environment.NewLine;

                    int i = 1;

                    foreach (var sch in teamSoll.Schuelers)
                    {
                        schuelerName += "|" + i.ToString().PadLeft(4) + "|" + (sch.Nachname + ", " + sch.Vorname).PadRight(25) + "|                    |              |" + Environment.NewLine;
                        i++;
                    }
                }
            }

            File.AppendAllText(dateiPraktikum, schuelerName + Environment.NewLine);
            Process.Start("notepad++.exe", dateiPraktikum);
        }

        public Teams(Teams klassenTeams, string name, Lehrers lehrers)
        {
            Teams klassenTeamsTemp = new Teams();

            foreach (var klas in klassenTeams.OrderBy(x => x.BildungsgangGekürzt))
            {
                var jahr = klas.DisplayName.Substring(klas.DisplayName.Length - 3, 2);

                var t = new Team
                {
                    DisplayName = klas.DisplayName + "-LuL",
                    BildungsgangLanganme = klas.BildungsgangLanganme,
                    BildungsgangGekürzt = klas.BildungsgangGekürzt,
                    Members = klas.Members,
                    Owners = klas.Owners,
                    Schuelers = klas.Schuelers,
                    Kategorie = klas.Kategorie,
                    Stufe = klas.Stufe,
                    Text = klas.Text,
                    Typ = klas.Typ,
                    TeamId = klas.TeamId
                };
                klassenTeamsTemp.Add(t);
            }

            if (name.StartsWith("Klassenteams") && name.EndsWith("-LuL"))
            {
                foreach (var klassenTeam in klassenTeamsTemp)
                {
                    klassenTeam.IstWordTeam = true;
                    klassenTeam.Members = (from m in klassenTeam.Members where !m.Mail.Contains("student") select m).ToList();
                    klassenTeam.Owners = new List<Lehrer>(); // Distribution Groups haben nur member
                    klassenTeam.Schuelers = klassenTeam.Schuelers;
                    klassenTeam.Beschreibung = name;
                }

                this.AddRange(klassenTeamsTemp);
            }

            if (name.StartsWith("Klassenteams") && name.EndsWith("-SuS"))
            {
                foreach (var klassenTeam in klassenTeamsTemp)
                {
                    klassenTeam.DisplayName += "-SuS";
                    klassenTeam.Members = (from m in klassenTeam.Members where m.Mail.Contains("student") select m).ToList();
                    klassenTeam.Owners = (from m in klassenTeam.Owners where m.Mail.Contains("student") select m).ToList();
                }

                this.AddRange(klassenTeamsTemp);
            }

            if (name == "Bildungsgaenge-LuL")
            {
                List<string> bildungsgänge = new List<string>();

                foreach (var klassenTeam in klassenTeamsTemp)
                {
                    // Die erste Ziffer und alles danach wird abgeschnitten

                    var bildungsgang = klassenTeam.DisplayName.Substring(0, klassenTeam.DisplayName.IndexOfAny("0123456789".ToCharArray()));

                    if (!bildungsgänge.Contains(bildungsgang))
                    {
                        bildungsgänge.Add(bildungsgang);

                        Team bgSoll = new Team(bildungsgang + "-LuL", "Bildungsgänge");

                        bgSoll.IstWordTeam = true;

                        // BG-Gruppen sind immer Verteilergruppen

                        bgSoll.IstVerteilergruppe = true;
                        bgSoll.BildungsgangLanganme = klassenTeam.BildungsgangLanganme;
                        bgSoll.BildungsgangKurzname = bildungsgang;
                        bgSoll.Bildungsgangleitung = (from l in lehrers
                                                      from a in l.Anrechnungen
                                                      where (a.TextGekürzt == klassenTeam.BildungsgangGekürzt
                                                      || a.TextGekürzt.Split(',')[0] == klassenTeam.BildungsgangGekürzt.Split(',')[0])
                                                      select l.Mail).FirstOrDefault();
                        bgSoll.Beschreibung = name;
                        bgSoll.Text = klassenTeam.Text;
                        bgSoll.Stufe = klassenTeam.Stufe;

                        if (bgSoll.Bildungsgangleitung == null)
                        {
                            Console.WriteLine("Keine Bildungsgangleitung in " + bgSoll.DisplayName + ". Vermutlich stimmt die Bezeichnung in den Anrechnungen in Untis nicht mit der Eigenschaft in den Stammdaten in Untis überein.");
                            Console.ReadKey();
                        }


                        foreach (var kT in klassenTeamsTemp)
                        {
                            if (kT.DisplayName.Substring(0, klassenTeam.DisplayName.IndexOfAny("0123456789".ToCharArray())) == bildungsgang)
                            {
                                foreach (var klassenTeamMember in (from k in kT.Members where !k.Mail.Contains("students") select k).ToList())
                                {
                                    if (!bgSoll.Members.Contains(klassenTeamMember) && klassenTeamMember != null)
                                    {
                                        bgSoll.Members.Add(klassenTeamMember);
                                    }
                                }
                            }
                        }

                        if (bgSoll.Members.Count > 0)
                        {
                            this.Add(bgSoll);
                        }
                    }
                }
            }
        }

        public Teams(Teams klassenTeams, string name, Lehrers lehrers, Anrechnungs anrechnungen)
        {
            Teams klassenTeamsTemp = new Teams();

            foreach (var klas in klassenTeams.OrderBy(x => x.BildungsgangGekürzt))
            {
                var jahr = klas.DisplayName.Substring(klas.DisplayName.Length - 3, 2);

                var t = new Team
                {
                    DisplayName = klas.DisplayName + "-LuL",
                    BildungsgangLanganme = klas.BildungsgangLanganme,
                    BildungsgangGekürzt = klas.BildungsgangGekürzt,
                    Members = klas.Members,
                    Owners = klas.Owners,
                    Schuelers = klas.Schuelers,
                    Kategorie = klas.Kategorie,
                    Stufe = klas.Stufe,
                    Text = klas.Text,
                    Typ = klas.Typ,
                    TeamId = klas.TeamId
                };
                klassenTeamsTemp.Add(t);
            }


            if (name.StartsWith("Klassenteams") && name.EndsWith("-LuL"))

            {
                foreach (var klassenTeam in klassenTeamsTemp)
                {

                    klassenTeam.IstWordTeam = true;
                    klassenTeam.Members = (from m in klassenTeam.Members where !m.Mail.Contains("student") select m).ToList();
                    klassenTeam.Owners = new List<Lehrer>(); // Distribution Groups haben nur member
                    klassenTeam.Schuelers = klassenTeam.Schuelers;
                    klassenTeam.Beschreibung = name;
                }

                this.AddRange(klassenTeamsTemp);
            }
        }

        public Teams()
        {
        }

        internal void Hinzufügen(Anrechnungs anrechnungen, Lehrers lehrers, Teams klassenMitAllenSoll)
        {
            foreach (var anrechnung in (from a in anrechnungen where a.Beschr != null where a.Beschr != "" where new List<string>() { "Verteilergruppe", "Team" }.Contains(a.Beschr) select a).ToList())
            {
                Team team = new Team(anrechnung.TextGekürzt);
                team.Typ = anrechnung.Beschr;

                foreach (var aa in (from ax in anrechnungen where ax.TextGekürzt == anrechnung.TextGekürzt select ax).ToList())
                {
                    var leMail = (from l in lehrers where l.IdUntis == aa.TeacherIdUntis select l).FirstOrDefault();

                    if (leMail == null)
                    {
                        string a = "";
                    }

                    if (!(from t in team.Members where t != null where t.Mail == leMail.Mail select t).Any())
                    {
                        if (anrechnung.TextGekürzt.StartsWith("Fachschaft"))
                        {
                            team.Vorsitz = @"[[https://teams.microsoft.com/l/chat/0/0?users=" + leMail.Mail + "|" + (leMail.Titel != null ? leMail.Titel + " " : "") + leMail.Vorname + " " + leMail.Nachname + @"]]";


                            if (anrechnung.TextGekürzt.Contains("nglisch"))
                            {
                                team.Members.AddRange(klassenMitAllenSoll.Fachschaft(lehrers, new List<string>() { "E", "E FU", "E1", "E2", "E G1", "E G2", "E L1", "E L2", "E L", "EL", "EL1", "EL2" }));
                            }
                            if (anrechnung.TextGekürzt.Contains("eligion"))
                            {
                                team.Members.AddRange(klassenMitAllenSoll.Fachschaft(lehrers, new List<string>() { "KR", "KR FU", "KR1", "KR2", "KR G1", "KR G2", "ER", "ER G1" }));
                            }
                            if (anrechnung.TextGekürzt.Contains("irtschaftslehre"))
                            {
                                team.Members.AddRange(klassenMitAllenSoll.Fachschaft(lehrers, new List<string>() { "WL", "WBL" }));
                            }
                            if (anrechnung.TextGekürzt.Contains("eutsch"))
                            {
                                team.Members.AddRange(klassenMitAllenSoll.Fachschaft(lehrers, new List<string>() { "D", "D FU", "D1", "D2", "D G1", "D G2", "D L1", "D L2", "D L", "DL", "DL1", "DL2" }));
                            }
                            if (anrechnung.TextGekürzt.Contains("athematik"))
                            {
                                team.Members.AddRange(klassenMitAllenSoll.Fachschaft(lehrers, new List<string>() { "M", "M FU", "M1", "M2", "M G1", "M G2", "M L1", "M L2", "M L", "ML", "ML1", "ML2" }));
                            }
                            if (anrechnung.TextGekürzt.Contains("olitik"))
                            {
                                team.Members.AddRange(klassenMitAllenSoll.Fachschaft(lehrers, new List<string>() { "PK", "PK FU", "PK1", "PK2", "GG G1", "GG G2" }));
                            }
                            if (anrechnung.TextGekürzt.Contains("port"))
                            {
                                team.Members.AddRange(klassenMitAllenSoll.Fachschaft(lehrers, new List<string>() { "SP" }));
                            }
                        }
                        else
                        {
                            team.Members.Add(leMail);
                        }
                    }
                }

                if (!(from t in this where t.DisplayName == team.DisplayName select t).Any())
                {
                    Hinzufügen(team);
                }
            }
        }

        internal void DateiGruppenUndMitgliederErzeugen()
        {
            string dateiDokuwiki = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\\Gruppen-und-Mitglieder.txt";

            if (File.Exists(dateiDokuwiki))
            {
                File.Delete(dateiDokuwiki);
            }

            File.WriteAllText(dateiDokuwiki, "====== Gruppen & Mitglieder ======" + Environment.NewLine);
            File.AppendAllText(dateiDokuwiki, "  Stand: " + DateTime.Now.ToShortDateString() + ", " + DateTime.Now.ToShortTimeString() + " Uhr; Bitte diese Seite nicht manuell ändern." + Environment.NewLine);
            File.AppendAllText(dateiDokuwiki, "Siehe auch [[:kollegium|Kollegium]]." + Environment.NewLine);

            foreach (var teamSoll in (from t in Global.TeamsSoll where t.Beschreibung == null select t).ToList().OrderBy(s => s.DisplayName))
            {
                string mitgliederMail = "";
                string mitgliederNachname = "";

                // Die Schulleiterin wird zuerst genannt. Alle anderen Gruppen bleiben alphabetisch sortiert.

                if (teamSoll.DisplayName == "Schulleitung" || teamSoll.Members.Count == 1)
                {
                    foreach (var member in teamSoll.Members.OrderBy(x => x.Vorname))
                    {

                        mitgliederNachname += member.Vorname.Substring(0, 1) + "." + member.Nachname + ", ";
                        teamSoll.Hyperlink += "<" + member.Mail + ">" + ", ";
                        mitgliederMail += "<" + member.Mail + ">" + "; ";
                    }
                }
                else
                {
                    try
                    {
                        foreach (var member in teamSoll.Members.OrderBy(x => x.Nachname))
                        {

                            mitgliederNachname += member.Vorname.Substring(0, 1) + "." + member.Nachname + ", ";
                            mitgliederMail += "<" + member.Mail + ">; ";
                            teamSoll.Hyperlink += member.Mail + ", ";
                        }
                    }
                    catch (Exception)
                    {
                        foreach (var member in teamSoll.Members)
                        {
                            mitgliederNachname += member.Vorname.Substring(0, 1) + "." + member.Nachname + ", ";
                            mitgliederMail += "<" + member.Mail + ">; ";
                            teamSoll.Hyperlink += member.Mail + ", ";
                        }
                    }
                }

                teamSoll.Hyperlink = teamSoll.Hyperlink.TrimEnd(',');

                mitgliederMail = mitgliederMail.TrimEnd(' ');
                mitgliederNachname = mitgliederNachname.TrimEnd(' ');
                mitgliederNachname = mitgliederNachname.TrimEnd(',');

                string namensraum = ":" + teamSoll.DisplayNameGekürzt;
                string name = teamSoll.DisplayNameGekürzt;

                if (teamSoll.DisplayName.StartsWith("System"))
                {
                    string a = "";
                }

                if (teamSoll.DisplayName.Contains("chulleitung") ||
teamSoll.DisplayName.Contains("tatistik") ||
teamSoll.DisplayName.Contains("ildungsgangleit") ||
teamSoll.DisplayName.Contains("lassenleitung"))
                {
                    namensraum = ":sl:" + teamSoll.DisplayNameGekürzt;
                }

                if (teamSoll.DisplayName.Contains("ortbildung"))
                {
                    namensraum = ":Fortbildung";
                }

                if (teamSoll.DisplayName.Contains("FHR"))
                {
                    namensraum = "fhr-pruefungsplanung";
                }

                if (teamSoll.DisplayName.Contains("undenoptimierung"))
                {
                    namensraum = ":Stundenplanung";
                }

                if (teamSoll.DisplayName.Contains("ymna"))
                {
                    namensraum = "bildungsgaenge:berufliches_gymnasium:start";
                }

                if (teamSoll.DisplayName.Contains("FHR"))
                {
                    namensraum = ":fhr-pruefungsplanung";
                }

                if (teamSoll.DisplayName == "SV")
                {
                    namensraum = "schulmitwirkung:schuelerrat";
                }

                if (teamSoll.DisplayName == "ildungsgangleitungen")
                {
                    namensraum = "sl:bildungsgangleitungen";
                }

                if (teamSoll.Kategorie == "Bildungsgänge")
                {
                    namensraum = "";
                    if (teamSoll.Stufe.StartsWith("BS"))
                    {
                        namensraum = "bildungsgaenge:berufsschule";
                    }
                    if (teamSoll.Stufe.StartsWith("AV") || teamSoll.Stufe.StartsWith("IFK"))
                    {
                        namensraum = "bildungsgaenge:ausbildungsvorbereitung";
                    }
                    if (teamSoll.Stufe.StartsWith("FOS"))
                    {
                        namensraum = "bildungsgaenge:fachoberschule";
                    }
                    if (teamSoll.Stufe.StartsWith("GY"))
                    {
                        namensraum = "bildungsgaenge:berufliches_gymnasium";
                    }
                    if (teamSoll.Stufe.StartsWith("BAB") || teamSoll.Stufe.StartsWith("BK/HSA") || teamSoll.Stufe.StartsWith("BFS"))
                    {
                        namensraum = "bildungsgaenge:berufsfachschule";
                    }

                    if (teamSoll.BildungsgangKurzname.StartsWith("HB"))
                    {
                        namensraum = "bildungsgaenge:hoehere_berufsfachschule";
                    }

                    if (teamSoll.BildungsgangKurzname.StartsWith("AG"))
                    {
                        namensraum += ":" + "AGG-AGS";
                    }
                    else
                    {
                        namensraum += ":" + teamSoll.BildungsgangKurzname;
                    }
                    name = teamSoll.BildungsgangLanganme;
                }

                File.AppendAllText(dateiDokuwiki, "===== " + name + " =====" + Environment.NewLine);

                File.AppendAllText(dateiDokuwiki, "Zur Wiki-Seite: [[" + namensraum + "|" + name + "]]" + Environment.NewLine);

                if (teamSoll.Vorsitz != null)
                {
                    File.AppendAllText(dateiDokuwiki, teamSoll.Vorsitz + Environment.NewLine);
                }
                File.AppendAllText(dateiDokuwiki, "| " + mitgliederNachname + "|" + mitgliederMail + " |" + Environment.NewLine);
            }
            Process.Start("notepad++.exe", dateiDokuwiki);
        }

        internal void Hinzufügen(Teams teams)
        {
            foreach (var team in teams)
            {
                Add(team);
            }
        }


        internal void SyncTeams(bool verteilergruppe, bool teams, Lehrers lehrers, Teams teamsSoll)
        {
            foreach (var teamSoll in teamsSoll)
            {
                this.SyncTeam(lehrers, teamSoll);
            }
        }

        internal void SyncTeam(Lehrers lehrers, Team teamSoll)
        {
            if (teamSoll.IstVerteilergruppe)
            {
                Sync(lehrers, teamSoll, "Distribution");
            }

            if (teamSoll.IstO365Gruppe)
            {
                Sync(lehrers, teamSoll, "O365");
            }
        }

        private void Sync(Lehrers lehrers, Team teamSoll, string typ)
        {
            // Es werden bestimmte Gruppen ausgeklammert

            var nichtBeachten = new List<string>() { "Erweiterte-Schulleitung", "Vertretungsplanung" };

            if (!nichtBeachten.Contains(teamSoll.DisplayName))
            {
                // Nur wenn das Team-Soll mehr als einen Member hat und der Name keinen Punkt enthält, wird es angelegt. Ein Punkt im Namen deutet auf Prosa hin und wird nicht zur Verteilergruppe

                if (teamSoll.Members.Count > 1 && !teamSoll.DisplayName.Contains(".") && !teamSoll.DisplayName.Contains(":") && !teamSoll.DisplayName.Contains(","))
                {
                    bool teamAngelegt = false;

                    if (!(from teamIst in this where teamIst.DisplayName == teamSoll.DisplayName where teamIst.Typ == typ select teamIst).Any())
                    {
                        teamSoll.Typ = typ;
                        teamSoll.TeamAnlegen(typ);

                        // Ein neu angelegtes Team wird den Ist-Teams zugerechnet, damit anchließend direkt Member hinzugefügt werden können.
                        Team t = new Team();
                        t.DisplayName = teamSoll.DisplayName;
                        t.Typ = typ;
                        t.Members = new List<Lehrer>();
                        t.Owners = new List<Lehrer>();

                        this.Add(t);
                        teamAngelegt = true;
                    }

                    if (teamAngelegt)
                    {
                        Global.TeamsPs1.Add(@"      {");
                    }
                    foreach (var teamIst in (from i in this where i.DisplayName == teamSoll.DisplayName where i.Typ == typ select i).ToList())
                    {
                        teamSoll.OwnerUndMemberAnlegen(teamIst);
                    }
                    if (teamAngelegt)
                    {
                        Global.TeamsPs1.Add(@"      }");
                    }
                }

                foreach (var teamIst in (from i in this where i.DisplayName == teamSoll.DisplayName where i.Typ == typ select i).ToList())
                {
                    // Aus Fachschaften werden LuL nie wieder entfernt.

                    if (!teamSoll.DisplayName.StartsWith("Fachschaft"))
                    {
                        teamIst.OwnerUndMemberLöschen(teamSoll, lehrers);
                    }
                }
            }
        }
                
        internal List<Lehrer> Fachschaft(Lehrers lehrers, List<string> faecher)
        {
            var members = new List<Lehrer>();

            foreach (var klassenTeam in this)
            {
                foreach (var unterricht in klassenTeam.Unterrichts)
                {
                    if (faecher.Contains(unterricht.FachKürzel))
                    {
                        var lehrerMail = (from l in lehrers where l.Kürzel == unterricht.LehrerKürzel select l).FirstOrDefault();

                        if (lehrerMail != null)
                        {
                            if (!members.Contains(lehrerMail))
                            {
                                members.Add(lehrerMail);
                            }
                        }
                    }
                }
            }
            return members;
        }

        internal IEnumerable<string> SportFachschaft(Lehrers lehrers)
        {
            var members = new List<string>();

            foreach (var klassenTeam in this)
            {
                foreach (var unterricht in klassenTeam.Unterrichts)
                {
                    if (new List<string>() { "SP", "SP G1", "SP G2" }.Contains(unterricht.FachKürzel))
                    {
                        var lehrerMail = (from l in lehrers where l.Kürzel == unterricht.LehrerKürzel select l.Mail).FirstOrDefault();

                        if (lehrerMail != null)
                        {
                            if (!members.Contains(lehrerMail))
                            {
                                members.Add(lehrerMail);
                            }
                        }
                    }
                }
            }
            return members;
        }
    }
}