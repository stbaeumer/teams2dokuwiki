using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace teams2dokuwiki
{
    class Program
    {
        public static int SuL { get; private set; }

        static void Main(string[] args)
        {
            try
            {
                Global.Initialize();
                Periodes periodes = new Periodes();
                var periode = (from p in periodes where p.Bis >= DateTime.Now.Date where DateTime.Now.Date >= p.Von select p.IdUntis).FirstOrDefault();
                var aktJahr = DateTime.Now.Month > 7 ? DateTime.Now.Year - 2000 : DateTime.Now.Year - 1 - 2000;

                Raums raums = new Raums(periode);
                Lehrers lehrers = new Lehrers(periode, raums, aktJahr);
                Klasses klasses = new Klasses(periode, lehrers);
                Schuelers schuelers = new Schuelers(klasses);
                Fachs fachs = new Fachs();
                Fachschaften fachschaften = new Fachschaften(fachs);
                Unterrichtsgruppes unterrichtsgruppes = new Unterrichtsgruppes();
                Unterrichts unterrichts = new Unterrichts(periode, klasses, lehrers, fachs, raums, unterrichtsgruppes);
                Anrechnungs anrechnungen = new Anrechnungs(periode);
                Teams klassenMitAllenSoll = new Teams(klasses, lehrers, schuelers, unterrichts);
                Global.TeamsSoll.Hinzufügen(anrechnungen, lehrers, klassenMitAllenSoll);

                Global.TeamsSoll.Hinzufügen(new Teams(klassenMitAllenSoll, "Klassenteams-LuL", lehrers, anrechnungen));
                Global.TeamsSoll.Hinzufügen(new Teams(klassenMitAllenSoll, "Bildungsgaenge-LuL", lehrers, anrechnungen));
                Global.TeamsSoll.Add(new Team("Kollegium", lehrers, lehrers.Kollegium()));
                Global.TeamsSoll.Add(new Team("Berufliches Gymnasium", lehrers, unterrichts.Abitur(lehrers)));
                Global.TeamsSoll.Add(new Team("FHR", lehrers, unterrichts.Fhr(lehrers)));
                Global.TeamsSoll.Add(new Team("SV"));
                Global.TeamsSoll.Add(new Team("Lehrerinnen", lehrers, lehrers.Lehrerinnen()));
                Global.TeamsSoll.Add(new Team("Referendare", lehrers, lehrers.Referendare()));
                Global.TeamsSoll.Add(new Team("Bildungsgangleitungen", lehrers, lehrers.GetAnrechungenAusBeschreibung("Bildungsgangleitungen")));
                Global.TeamsSoll.Add(new Team("Bildungsgangleitungen A", lehrers, lehrers.GetAnrechungenAusBeschreibung("Bildungsgangleitungen A")));
                Global.TeamsSoll.Add(new Team("Bildungsgangleitungen B", lehrers, lehrers.GetAnrechungenAusBeschreibung("Bildungsgangleitungen B")));
                Global.TeamsSoll.Add(new Team("Bildungsgangleitungen C", lehrers, lehrers.GetAnrechungenAusBeschreibung("Bildungsgangleitungen C")));
                Global.TeamsSoll.Add(new Team("Bildungsgangleitungen D", lehrers, lehrers.GetAnrechungenAusBeschreibung("Bildungsgangleitungen D")));
                Global.TeamsSoll.Add(new Team("Klassenleitungen", lehrers, klasses.GetKlassenleitungen()));
                //Global.TeamsSoll.Hinzufügen(new Team("Bildungsgangleitungen", lehrers, members));
                //Global.TeamsSoll.Hinzufügen(new Team("KlassenleitungenBlaueBriefe", lehrers, klasses.KlassenleitungenBlaueBriefe(aktJahr)));

                lehrers.DateiKollegiumErzeugen(unterrichts, klasses, fachschaften);
                //Global.TeamsSoll.DateiGruppenUndMitgliederErzeugen();
                Global.TeamsSoll.DateiPraktiumErzeugen();
                lehrers.DateiAnrechnungenErzeugen(klasses);
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}