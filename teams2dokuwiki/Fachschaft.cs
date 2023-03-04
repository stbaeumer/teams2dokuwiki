namespace teams2dokuwiki
{
    internal class Fachschaft
    {
        public Fachschaft(string kürzelUntis, string beschr)
        {
            KürzelUntis = kürzelUntis;
            Beschr = beschr;
        }

        public string KürzelUntis { get; private set; }
        public string Beschr { get; }
    }
}