﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace teams2dokuwiki
{
    public class Klasse
    {
        public int IdUntis { get; internal set; }
        public string NameUntis { get; internal set; }
        public List<Lehrer> Klassenleitungen { get; internal set; }
        public bool IstVollzeit { get; internal set; }
        public string BildungsgangLangname { get; internal set; }
        public string BildungsgangGekürzt { get; internal set; }
        public string Text { get; internal set; }
        public string Stufe { get; internal set; }
        public string Schule { get; private set; }        
    }
}
