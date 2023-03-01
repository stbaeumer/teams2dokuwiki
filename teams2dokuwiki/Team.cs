using System;
using System.Collections.Generic;
using System.Linq;

namespace teams2dokuwiki
{
    public class Team
    {
        public string DisplayName { get; set; }
        public string DisplayNameGekürzt { get; private set; }
        public string TeamId { get; set; }
        public List<Lehrer> Owners { get; set; }
        public bool IstVerteilergruppe { get; set; }
        public bool IstWordTeam { get; set; }
        public bool IstO365Gruppe { get; set; }
        public List<Lehrer> Members { get; set; }
        public Unterrichts Unterrichts { get; private set; }
        public string Kategorie { get; set; }
        public Lehrer Klassenleitung { get; internal set; }
        public string Typ { get; set; }
        public string MembersString { get; private set; }
        public string BildungsgangLanganme { get; internal set; }
        public string Bildungsgangleitung { get; internal set; }
        
        public IEnumerable<string> Enumerable { get; }
        public List<string> List { get; }
        public string BildungsgangKurzname { get; internal set; }
        public string BildungsgangGekürzt { get; internal set; }
        public string Vorsitz { get; set; }
        public string Hyperlink { get; internal set; }
        
        public string Beschreibung { get; internal set; }
        public List<Schueler> Schuelers { get; internal set; }
        public string Stufe { get; internal set; }
        public string Text { get; internal set; }


        public Team(string name)
        {
            DisplayName = name.Replace("--", "-")
                                        .Replace("/ ", "-")
                                        .Replace("---", "-")
                                        .Replace("ä", "ae")
                                        .Replace("ö", "oe")
                                        .Replace("ü", "ue")
                                        .Replace("ß", "ss")
                                        .Replace("Ä", "Ae")
                                        .Replace("Ö", "Oe")
                                        .Replace("Ü", "Ue")
                                        .Replace(" ", "-")
                                        .Replace("/", "-")

                                        .Replace("(", "")
                                        .Replace(")", "")
                                        .Replace("---", "-");
            Members = new List<Lehrer>();
            Owners = new List<Lehrer>();
            Hyperlink = "https://teams.microsoft.com/l/chat/0/0?users=";
        }

        public Team(string displayName, string kategorie)
        {
            this.DisplayName = displayName;
            Kategorie = kategorie;
            Owners = new List<Lehrer>() { };
            Members = new List<Lehrer>() { };
            Unterrichts = new Unterrichts();
            Schuelers = new List<Schueler> { };
            Typ = "Verteilergruppe";
        }

        public Team(Teams klassenteams, Klasses klasses, Lehrers lehrers, string name, bool untisAnrechnungen)
        {
            Members = new List<Lehrer>();
            Owners = new List<Lehrer>();
            IstVerteilergruppe = true;
            IstO365Gruppe = false;

            DisplayName = name.Replace("--", "-")
                                        .Replace("---", "-")
                                        .Replace("ä", "ae")
                                        .Replace("ö", "oe")
                                        .Replace("ü", "ue")
                                        .Replace("ß", "ss")
                                        .Replace("Ä", "Ae")
                                        .Replace("Ö", "Oe")
                                        .Replace("Ü", "Ue")
                                        .Replace(" ", "-")
                                        .Replace("/", "-")
                                        .Replace("---", "-");

            int sj = Convert.ToInt32(Global.AktSj[0].Substring(2, 2));

            // Klassenleitungen

            if (name == "Klassenleitungen")
            {
                IstWordTeam = true;

                foreach (var klasse in klasses)
                {
                    foreach (var klassenleitung in klasse.Klassenleitungen)
                    {
                        if (!this.Members.Contains(klassenleitung))
                        {
                            this.Members.Add(klassenleitung);
                        }
                    }
                }
            }

            if (name == "Klassenleitungen-AnlageA")
            {
                IstWordTeam = true;

                foreach (var klasse in klasses)
                {
                    if (
                        !klasse.NameUntis.StartsWith("G") &&
                        !klasse.NameUntis.StartsWith("H") &&
                        !klasse.NameUntis.StartsWith("AV") &&
                        !klasse.NameUntis.StartsWith("BS") &&
                        !klasse.NameUntis.StartsWith("IF") &&
                        !klasse.NameUntis.StartsWith("F")
                        )
                    {
                        foreach (var klassenleitung in klasse.Klassenleitungen)
                        {
                            if (!this.Members.Contains(klassenleitung))
                            {
                                this.Members.Add(klassenleitung);
                            }
                        }
                    }
                }
            }

            // Klassenleitungen-AnlageA-Unterstufe

            if (name == "Klassenleitungen-AnlageA-Unterstufe")
            {
                foreach (var klasse in klasses)
                {
                    if (
                        klasse.NameUntis.Contains((sj).ToString()) &&
                        !klasse.NameUntis.StartsWith("G") &&
                        !klasse.NameUntis.StartsWith("H") &&
                        !klasse.NameUntis.StartsWith("AV") &&
                        !klasse.NameUntis.StartsWith("BS") &&
                        !klasse.NameUntis.StartsWith("IF") &&
                        !klasse.NameUntis.StartsWith("F")
                        )
                    {
                        foreach (var klassenleitung in klasse.Klassenleitungen)
                        {
                            if (!this.Members.Contains(klassenleitung))
                            {
                                this.Members.Add(klassenleitung);
                            }
                        }
                    }
                }
            }

            if (name == "Klassenpflegschaft-Teilzeit")
            {
                IstWordTeam = true;

                foreach (var klasse in klasses)
                {
                    if (!klasse.IstVollzeit)
                    {
                        foreach (var klassenleitung in klasse.Klassenleitungen)
                        {
                            if (!this.Members.Contains(klassenleitung))
                            {
                                this.Members.Add(klassenleitung);
                            }
                        }
                    }
                }
            }

            if (name == "Klassenpflegschaft-Vollzeit")
            {
                IstWordTeam = true;

                foreach (var klasse in klasses)
                {
                    if (klasse.IstVollzeit)
                    {
                        foreach (var klassenleitung in klasse.Klassenleitungen)
                        {
                            if (!this.Members.Contains(klassenleitung))
                            {
                                this.Members.Add(klassenleitung);
                            }
                        }
                    }
                }
            }

            if (name == "Klassenleitungen-Gym13")
            {
                foreach (var klasse in klasses)
                {
                    if (klasse.NameUntis.StartsWith("G") && klasse.NameUntis.Contains((sj - 2).ToString()))
                    {
                        foreach (var klassenleitung in klasse.Klassenleitungen)
                        {
                            if (!this.Members.Contains(klassenleitung))
                            {
                                this.Members.Add(klassenleitung);
                            }
                        }
                    }
                }
            }

            if (name == "Klassenleitungen-AbschlussklassenBC")
            {
                foreach (var klasse in klasses)
                {
                    if (
                            klasse.NameUntis.StartsWith("BS") && klasse.NameUntis.Contains((sj - 1).ToString()) ||
                            klasse.NameUntis.StartsWith("HB") && klasse.NameUntis.Contains((sj - 1).ToString()) ||
                            klasse.NameUntis.StartsWith("FM") && klasse.NameUntis.Contains((sj).ToString()) ||
                            klasse.NameUntis.StartsWith("FS") && klasse.NameUntis.Contains((sj - 1).ToString())
                        )
                    {
                        foreach (var klassenleitung in klasse.Klassenleitungen)
                        {
                            if (!this.Members.Contains(klassenleitung))
                            {
                                this.Members.Add(klassenleitung);
                            }
                        }
                    }
                }
            }

            if (name == "Klassenleitungen-Versetzungsklassen")
            {
                foreach (var klasse in klasses)
                {
                    if (
                            klasse.NameUntis.StartsWith("BS") && klasse.NameUntis.Contains((sj).ToString()) ||
                            klasse.NameUntis.StartsWith("HB") && klasse.NameUntis.Contains((sj).ToString()) ||
                            klasse.NameUntis.StartsWith("G") && klasse.NameUntis.Contains((sj).ToString()) ||
                            klasse.NameUntis.StartsWith("FS") && klasse.NameUntis.Contains((sj).ToString())
                        )
                    {
                        foreach (var klassenleitung in klasse.Klassenleitungen)
                        {
                            if (!this.Members.Contains(klassenleitung))
                            {
                                this.Members.Add(klassenleitung);
                            }
                        }
                    }
                }
            }

            if (name == "Gym13-SuS")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (klassenTeam.DisplayName.StartsWith("G") && klassenTeam.DisplayName.Contains((sj - 2).ToString()))
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Gym13-LuL")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (klassenTeam.DisplayName.StartsWith("G") && klassenTeam.DisplayName.Contains((sj - 2).ToString()))
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "AbschlussklassenBC-LuL")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            klassenTeam.DisplayName.StartsWith("BS") && klassenTeam.DisplayName.Contains((sj - 1).ToString()) ||
                            klassenTeam.DisplayName.StartsWith("HB") && klassenTeam.DisplayName.Contains((sj - 1).ToString()) ||
                            klassenTeam.DisplayName.StartsWith("FM") && klassenTeam.DisplayName.Contains((sj).ToString()) ||
                            klassenTeam.DisplayName.StartsWith("FS") && klassenTeam.DisplayName.Contains((sj - 1).ToString())
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "AbschlussklassenBC-SuS")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            klassenTeam.DisplayName.StartsWith("BS") && klassenTeam.DisplayName.Contains((sj - 1).ToString()) ||
                            klassenTeam.DisplayName.StartsWith("HB") && klassenTeam.DisplayName.Contains((sj - 1).ToString()) ||
                            klassenTeam.DisplayName.StartsWith("FM") && klassenTeam.DisplayName.Contains((sj).ToString()) ||
                            klassenTeam.DisplayName.StartsWith("FS") && klassenTeam.DisplayName.Contains((sj - 1).ToString())
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "BlaueBriefe-LuL")
            {
                IstWordTeam = true;

                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            klassenTeam.DisplayName.StartsWith("G") && klassenTeam.DisplayName.Contains((sj).ToString()) ||
                            klassenTeam.DisplayName.StartsWith("BS") && klassenTeam.DisplayName.Contains((sj).ToString()) ||
                            klassenTeam.DisplayName.StartsWith("HB") && klassenTeam.DisplayName.Contains((sj).ToString())
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            // Halbjahreszeugniskonferenzen

            if (name == "Halbjahreszeugniskonferenzen BS HBG FS") // namensgleich mit Outlook-Termin
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            klassenTeam.DisplayName.StartsWith("BS") ||
                            klassenTeam.DisplayName.StartsWith("FS") ||
                            klassenTeam.DisplayName.StartsWith("HBG")
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Halbjahreszeugniskonferenzen BW HBW") // namensgleich mit Outlook-Termin
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            klassenTeam.DisplayName.StartsWith("BW") ||
                            klassenTeam.DisplayName.StartsWith("HBW")
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Halbjahreszeugniskonferenzen BT HBT FM") // namensgleich mit Outlook-Termin
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            klassenTeam.DisplayName.StartsWith("BT") ||
                            klassenTeam.DisplayName.StartsWith("HBT") ||
                            klassenTeam.DisplayName.StartsWith("FM")
                       )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Halbjahreszeugniskonferenzen GE GT GW") // namensgleich mit Outlook-Termin
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            klassenTeam.DisplayName.StartsWith("G")
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            // Jahreszeugniskonferenzen

            if (name == "Jahreszeugniskonferenzen BS HBG GG12") // namensgleich mit Outlook-Termin
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            (klassenTeam.DisplayName.StartsWith("BS") && klassenTeam.DisplayName.Contains((sj).ToString())) ||
                            (klassenTeam.DisplayName.StartsWith("HBG") && klassenTeam.DisplayName.Contains((sj).ToString())) ||
                            (klassenTeam.DisplayName.StartsWith("GG") && klassenTeam.DisplayName.Contains((sj - 1).ToString()))
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Jahreszeugniskonferenzen BW HBW GW12")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            (klassenTeam.DisplayName.StartsWith("BW") && klassenTeam.DisplayName.Contains((sj).ToString())) ||
                            (klassenTeam.DisplayName.StartsWith("HBW") && klassenTeam.DisplayName.Contains((sj).ToString())) ||
                            (klassenTeam.DisplayName.StartsWith("GW12") && klassenTeam.DisplayName.Contains((sj - 1).ToString()))
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Jahreszeugniskonferenzen BT HBT GT12")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            (klassenTeam.DisplayName.StartsWith("BT") && klassenTeam.DisplayName.Contains((sj).ToString())) ||
                            (klassenTeam.DisplayName.StartsWith("HBT") && klassenTeam.DisplayName.Contains((sj).ToString())) ||
                            (klassenTeam.DisplayName.StartsWith("GT12") && klassenTeam.DisplayName.Contains((sj - 1).ToString()))
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Jahreszeugniskonferenzen GG GT GW 11")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            (klassenTeam.DisplayName.StartsWith("GG") && klassenTeam.DisplayName.Contains((sj).ToString())) ||
                            (klassenTeam.DisplayName.StartsWith("GT") && klassenTeam.DisplayName.Contains((sj).ToString())) ||
                            (klassenTeam.DisplayName.StartsWith("GW") && klassenTeam.DisplayName.Contains((sj).ToString()))
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Jahreszeugniskonferenzen FS")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            (klassenTeam.DisplayName.StartsWith("FS") && klassenTeam.DisplayName.Contains((sj).ToString()))
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }



            if (name == "Einschulung-Teilzeit-740")
            {
                foreach (var klasse in klasses)
                {
                    if (
                            !klasse.IstVollzeit
                            && klasse.NameUntis.Contains((sj).ToString())
                            && !klasse.NameUntis.StartsWith("AL")
                            && !klasse.NameUntis.StartsWith("AV")
                        )
                    {
                        foreach (var klassenleitung in klasse.Klassenleitungen)
                        {
                            if (!this.Members.Contains(klassenleitung))
                            {
                                this.Members.Add(klassenleitung);
                            }
                        }
                    }
                }
            }

            if (name == "Einschulung-Teilzeit-1100")
            {
                foreach (var klasse in klasses)
                {
                    if (
                            !klasse.IstVollzeit
                            && klasse.NameUntis.Contains((sj).ToString())
                            && klasse.NameUntis.StartsWith("AL")
                        )
                    {
                        foreach (var klassenleitung in klasse.Klassenleitungen)
                        {
                            if (!this.Members.Contains(klassenleitung))
                            {
                                this.Members.Add(klassenleitung);
                            }
                        }
                    }
                }
            }

            if (name == "Einschulung-Vollzeit")
            {
                foreach (var klasse in klasses)
                {
                    if (
                            klasse.IstVollzeit && klasse.NameUntis.Contains((sj).ToString())
                        )
                    {
                        foreach (var klassenleitung in klasse.Klassenleitungen)
                        {
                            if (!this.Members.Contains(klassenleitung))
                            {
                                this.Members.Add(klassenleitung);
                            }
                        }
                    }
                }
            }

            if (name == "Einschulung-AV")
            {
                foreach (var klasse in klasses)
                {
                    if (
                            klasse.NameUntis.StartsWith("AV") && klasse.NameUntis.Contains((sj).ToString())
                        )
                    {
                        foreach (var klassenleitung in klasse.Klassenleitungen)
                        {
                            if (!this.Members.Contains(klassenleitung))
                            {
                                this.Members.Add(klassenleitung);
                            }
                        }
                    }
                }
            }

            if (name == "Vollzeitkraefte")
            {
                IstWordTeam = true;

                foreach (var vollzeitkraft in (from l in lehrers where l.Deputat >= 25.5 select l))
                {
                    if (!this.Members.Contains(vollzeitkraft))
                    {
                        this.Members.Add(vollzeitkraft);
                    }
                }
            }

            if (name == "Teilzeitkraefte")
            {
                IstWordTeam = true;

                foreach (var teilzeitkraft in (from l in lehrers where l.Deputat < 25.5 select l))
                {
                    if (!this.Members.Contains(teilzeitkraft))
                    {
                        this.Members.Add(teilzeitkraft);
                    }
                }
            }

            // Praktika

            if (name == "Wahlpraktikum-GG-GT-GW")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            klassenTeam.DisplayName.StartsWith("GW") && klassenTeam.DisplayName.Contains((sj).ToString()) ||
                            klassenTeam.DisplayName.StartsWith("GT") && klassenTeam.DisplayName.Contains((sj).ToString()) ||
                            klassenTeam.DisplayName.StartsWith("GG") && klassenTeam.DisplayName.Contains((sj).ToString())
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Wahlpraktikum-HBW-Unterstufe-A+B")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            (klassenTeam.DisplayName.StartsWith("HBW") && klassenTeam.DisplayName.Contains((sj).ToString()) && klassenTeam.DisplayName.EndsWith("A")) ||
                            (klassenTeam.DisplayName.StartsWith("HBW") && klassenTeam.DisplayName.Contains((sj).ToString()) && klassenTeam.DisplayName.EndsWith("B"))
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Wahlpraktikum-HBW-Unterstufe-C+D")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            (klassenTeam.DisplayName.StartsWith("HBW") && klassenTeam.DisplayName.Contains((sj).ToString()) && klassenTeam.DisplayName.EndsWith("C")) ||
                            (klassenTeam.DisplayName.StartsWith("HBW") && klassenTeam.DisplayName.Contains((sj).ToString()) && klassenTeam.DisplayName.EndsWith("D"))
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Wahlpraktikum-BW")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            (klassenTeam.DisplayName.StartsWith("BW") && klassenTeam.DisplayName.Contains((sj).ToString()))
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Wahlpraktikum-BT")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            (klassenTeam.DisplayName.StartsWith("BT") && klassenTeam.DisplayName.Contains((sj).ToString()))
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Wahlpraktikum-HBT-Oberstufe")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            (klassenTeam.DisplayName.StartsWith("HBT") && klassenTeam.DisplayName.Contains((sj - 1).ToString()))
                       )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Wahlpraktikum-HBT-Unterstufe")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            (klassenTeam.DisplayName.StartsWith("HBT") && klassenTeam.DisplayName.Contains((sj).ToString()))
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Wahlpraktikum-HBG-Unterstufe")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            (klassenTeam.DisplayName.StartsWith("HBG") && klassenTeam.DisplayName.Contains((sj).ToString()))
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Pflichtpraktikum-BS-Unterstufe")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            klassenTeam.DisplayName.StartsWith("BS") && klassenTeam.DisplayName.Contains((sj).ToString())
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Pflichtpraktikum-BS-Oberstufe")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            (klassenTeam.DisplayName.StartsWith("BS") && klassenTeam.DisplayName.Contains((sj - 1).ToString()))
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Wahlpraktikum-BS-Oberstufe")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            (klassenTeam.DisplayName.StartsWith("BS") && klassenTeam.DisplayName.Contains((sj - 1).ToString()))
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }

            if (name == "Pflichtpraktikum-BS-Oberstufe")
            {
                foreach (var klassenTeam in klassenteams)
                {
                    if (
                            (klassenTeam.DisplayName.StartsWith("BS") && klassenTeam.DisplayName.Contains((sj - 1).ToString()))
                        )
                    {
                        foreach (var member in klassenTeam.Members)
                        {
                            if (!this.Members.Contains(member) && member.Mail.Contains("student"))
                            {
                                this.Members.Add(member);
                            }
                        }
                    }
                }
            }
        }

        public Team()
        {
        }
        
        public Team(string name, Lehrers lehrers, List<Lehrer> list) : this(name)
        {
            DisplayName = name.Replace("--", "-")
                                        .Replace("/ ", "-")
                                        .Replace("ä", "ae")
                                        .Replace("ö", "oe")
                                        .Replace("ü", "ue")
                                        .Replace("ß", "ss")
                                        .Replace("Ä", "Ae")
                                        .Replace("Ö", "Oe")
                                        .Replace("Ü", "Ue")
                                        .Replace(" ", "-")
                                        .Replace("/", "-")
                                        .Replace("(", "")
                                        .Replace(")", "")
                                        .Replace("---", "-");

            if (DisplayName.StartsWith("Fachschaft"))
            {
                Vorsitz = (from l in lehrers from a in l.Anrechnungen where a.TextGekürzt == name select l.Mail).FirstOrDefault();

                if (Vorsitz == null)
                {
                    Vorsitz = "N.N.";
                }
            }

            Members = list;
            Typ = (new List<string>() { "Abitur", "Kollegium", "FHR" }).Contains(DisplayName) ? "Team" : "Verteilergruppe";
        }

        

        //public Team(string name, Lehrers lehrers, List<Lehrer> list) : this(name)
        //{
        //    DisplayName = name.Replace("--", "-").Replace("/ ", "-");

        //    if (Vorsitz == null)
        //    {
        //        Vorsitz = "";
        //    }

        //    if (DisplayName.StartsWith("Berufliches-Gym"))
        //    {
        //        var leh = (from l in lehrers from a in l.Anrechnungen where a.TextGekürzt == name select l).FirstOrDefault();

        //        if (leh != null)
        //        {
        //            Vorsitz = @"\\ Leitung: [[chat>" + leh.Mail.Replace("@berufskolleg-borken.de", "") + "|" + (leh.Titel != null ? leh.Titel : "") + leh.Vorname + " " + leh.Nachname + "]]";
        //        }
        //        else
        //        {
        //            Vorsitz = "";
        //        }
        //    }

        //    if (DisplayName.StartsWith("Lehrerinnen"))
        //    {
        //        var leh = (from l in lehrers from a in l.Anrechnungen where a.TextGekürzt.StartsWith("Ansprechpartnerin") select l).FirstOrDefault();

        //        if (leh != null)
        //        {
        //            Vorsitz = @"\\ AfG: [[chat>" + leh.Mail.Replace("@berufskolleg-borken.de", "") + "|" + (leh.Titel != null ? leh.Titel : "") + leh.Vorname + " " + leh.Nachname + "]]";
        //        }
        //        else
        //        {
        //            Vorsitz = "";
        //        }
        //    }

        //    //Members = list;
        //    Typ = (new List<string>() { "Abitur", "Kollegium", "FHR" }).Contains(DisplayName) ? "Team" : "Verteilergruppe";
        //}

        internal void TeamAnlegen(string typ)
        {
            if (Owners.Count + Members.Count > 0)
            {
                if (this.Typ == typ)
                {

                    Console.WriteLine("[+] Neue Verteilergruppe : " + DisplayName);
                    Global.TeamsPs1.Add(@" ");
                    Global.TeamsPs1.Add(@"Write-Host ''");
                    Global.TeamsPs1.Add(@"Write-Host -ForegroundColor Green '[+] Neue Verteilergruppe : *" + DisplayName + "* - Soll sie von extern erreichbar sein? (j/ENTER)' -NoNewline");
                    Global.TeamsPs1.Add(@"  $bool = switch (Read-Host) { 'j' { $true } default { $false }}");
                    Global.TeamsPs1.Add(@"    New-DistributionGroup -Name " + DisplayName + " -PrimarySmtpAddress " + ToSmtp(DisplayName) + "@berufskolleg-borken.de -RequireSenderAuthenticationEnabled $bool; $bool = $false; Set-Group -Identity " + DisplayName + " -Notes 'automatisch aktualisiert | BM | " + DateTime.Now.ToShortDateString() + "'");
                    Global.TeamsPs1.Add(@"Write-Host -ForegroundColor Yellow 'Auch die Member der Verteilergruppe " + DisplayName + @" anlegen? (ENTER/n)' -NoNewline");
                    Global.TeamsPs1.Add(@"    if ((Read-Host) -eq '')");
                }
                else
                {
                    if (Kategorie == "Klasse")
                    {
                        Console.WriteLine(" Die Klassengruppen müssen zuerst händisch angelegt werden. Danach bitte neu starten.");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                }
            }
        }

        private string ToSmtp(string displayName)
        {
            return displayName.Replace("--", "-")
                                        .Replace("---", "-")
                                        .Replace("ä", "ae")
                                        .Replace("ö", "oe")
                                        .Replace("ü", "ue")
                                        .Replace("ß", "ss")
                                        .Replace("Ä", "Ae")
                                        .Replace("Ö", "Oe")
                                        .Replace("Ü", "Ue")
                                        .Replace(" ", "-")
                                        .Replace("/", "-")
                                        .Replace("---", "-");
        }

        internal void OwnerUndMemberAnlegen(Team teamIst)
        {
            foreach (var sollMember in this.Members)
            {
                if (!teamIst.Members.Contains(sollMember))
                {
                    Console.WriteLine(" Neuer " + teamIst.Typ.Substring(0, 4) + "-Member  : " + sollMember.Mail.PadRight(44) + " -> " + teamIst.DisplayName);
                    Global.TeamsPs1.Add(@"       Write-Host '[+] Neuer " + teamIst.Typ.Substring(0, 4) + "-Member    : " + sollMember.Mail.PadRight(44) + " -> " + teamIst.DisplayName + "'");

                    if (teamIst.Typ == "O365")
                    {
                        Global.TeamsPs1.Add(@"         Add-UnifiedGroupLinks -Identity " + teamIst.TeamId + " -LinkType Member -Links '" + sollMember + "' -Confirm:$confirm");
                    }
                    if (teamIst.Typ == "Distribution")
                    {
                        string updateMarker = UpdateMarker(teamIst.DisplayName);
                        Global.TeamsPs1.Add(@"        Add-DistributionGroupMember -Identity " + teamIst.DisplayName + " -Member '" + sollMember + "'; " + updateMarker);
                    }
                }
            }
            foreach (var sollOwner in this.Owners)
            {
                if (!teamIst.Owners.Contains(sollOwner))
                {
                    if (teamIst.Typ == "O365")
                    {
                        Console.WriteLine(" Neuer " + teamIst.Typ.Substring(0, 4) + "-Owner  :  " + sollOwner.Mail.PadRight(44) + " -> " + teamIst.DisplayName);
                        Global.TeamsPs1.Add(@"       Write-Host '[+] Neuer " + teamIst.Typ.Substring(0, 4) + "-Owner     : " + sollOwner.Mail.PadRight(44) + " -> " + teamIst.DisplayName + "'");
                        Global.TeamsPs1.Add(@"        Add-UnifiedGroupLinks -Identity " + teamIst.TeamId + " -LinkType Owner -Links '" + sollOwner + "' -Confirm:$confirm");
                    }

                    // In Verteilergruppen werden alle Owner zu Member

                    if (teamIst.Typ == "Distribution" && !teamIst.Members.Contains(sollOwner))
                    {
                        Console.WriteLine(" Neuer " + teamIst.Typ.Substring(0, 4) + "-Owner  :  " + sollOwner.Mail.PadRight(44) + " -> " + teamIst.DisplayName);
                        Global.TeamsPs1.Add(@"       Write-Host '[+] Neuer " + teamIst.Typ.Substring(0, 4) + "-Owner     : " + sollOwner.Mail.PadRight(44) + " -> " + teamIst.DisplayName + "'");
                        string updateMarker = UpdateMarker(teamIst.DisplayName);
                        Global.TeamsPs1.Add(@"            Add-DistributionGroupMember -Identity " + teamIst.DisplayName + " -Member '" + sollOwner + "'; " + updateMarker);
                    }
                }
            }
        }

        private string UpdateMarker(string displayName)
        {
            foreach (var item in Global.TeamsPs1)
            {
                if (item.Contains("Set-Group -Identity " + displayName + " -Notes 'automatisch aktualisiert | BM | " + DateTime.Now.ToShortDateString() + "'"))
                {
                    return "";
                }
            }
            return "Set-Group -Identity " + displayName + " -Notes 'automatisch aktualisiert | BM | " + DateTime.Now.ToShortDateString() + "'";
        }

        internal void OwnerUndMemberLöschen(Team teamSoll, Lehrers lehrers)
        {
            foreach (var istMember in this.Members)
            {
                if (!teamSoll.Members.Contains(istMember))
                {
                    // Schüler werden bedingungslos gelöscht.
                    // Lehrer werden nur im August, September und nach den HZ-Zeugniskonferenzen gelöscht.
                    // Andere Member (Sekretariat usw.) werden nie gelöscht.

                    if (istMember.Mail.Contains("students") || (lehrers.istLehrer(istMember) && (DateTime.Now.Month == 9 || DateTime.Now.Month == 8 || (DateTime.Now.Month == 2 && DateTime.Now.Day > 8))))
                    {
                        Console.WriteLine("    - Dist-Member entfernen :" + istMember + " aus " + this.DisplayName);
                        string updateMarker = UpdateMarker(this.DisplayName);
                        Global.TeamsPs1.Add(@" ");
                        Global.TeamsPs1.Add(@"Write-Host -ForegroundColor Red '[-] Dist-Member " + istMember + " aus der Verteilergruppe " + this.DisplayName + " entfernen? (ENTER/n)' -NoNewline");
                        Global.TeamsPs1.Add(@"if ((Read-Host ) -eq ''){");
                        Global.TeamsPs1.Add(@" Remove-DistributionGroupMember -Identity " + this.DisplayName + " -Member '" + istMember + "' -Confirm:$false; " + updateMarker + "}");
                    }
                }
            }
            foreach (var istOwner in this.Owners)
            {
                if (!teamSoll.Owners.Contains(istOwner))
                {
                    // Lehrer werden nur im August, September und nach den HZ-Zeugniskonferenzen gelöscht.
                    // Nur wenn die Kategorie "Klasse" ist, werden Lehrer als Owner gelöscht.

                    if ((DateTime.Now.Month == 9 || DateTime.Now.Month == 8 || (DateTime.Now.Month == 2 && DateTime.Now.Day > 8)) && Kategorie == "Klasse" && !istOwner.Mail.Contains("students"))
                    {
                        if ((from l in lehrers where l == istOwner select l).Any())
                        {
                            Console.WriteLine("      - Owner  entfernen:" + istOwner + " aus " + this.DisplayName);
                            string updateMarker = UpdateMarker(this.DisplayName);
                            Global.TeamsPs1.Add(@" ");
                            Global.TeamsPs1.Add(@"if ((Read-Host '[-] Dist-Owner " + istOwner + " aus der Verteilergruppe " + this.DisplayName + " entfernen? (ENTER/n)') -eq ''){");
                            Global.TeamsPs1.Add(@" Remove-DistributionGroupMember -Identity " + this.DisplayName + " -Member '" + istOwner + "' -Confirm:$false; " + updateMarker + "}");
                        }
                    }
                }
            }
        }
    }
}