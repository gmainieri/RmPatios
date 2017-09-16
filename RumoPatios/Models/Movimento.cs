using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RumoPatios.Models
{
    public class Movimento
    {
        public Movimento() { }

        public Movimento(Linha linhaFrom, Linha linhaTo, int qtde)
        {
            this.linhaOrigem = linhaFrom;
            this.linhaDestino = linhaTo;
            this.qtdeVagoes = qtde;
        }

        public Linha linhaOrigem { get; set; }
        public Linha linhaDestino { get; set; }
        public int qtdeVagoes { get; set; }

    }
}