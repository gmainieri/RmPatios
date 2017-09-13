using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RumoPatios.Models
{
    public class Descarga
    {
        public Descarga() { }

        public Descarga(Linha linhaOrigem)
        {
            //this.QtdeVagoes = qtde;
            this.linhaOrigem = linhaOrigem;
        }

        internal int Idx { get; set; }

        //internal DateTime instante { get; set; }

        //internal int QtdeVagoes { get; set; }

        //internal string Terminal { get; set; }
        //internal string Produto { get; set; }
        //internal string Cliente { get; set; }
        internal Linha linhaOrigem { get; set; }
    }
}