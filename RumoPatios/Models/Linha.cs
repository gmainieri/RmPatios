using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RumoPatios.Models
{
    public class Linha
    {
        public int LinhaID { get; set; }

        public string Nome { get; set; }

        /// <summary>
        /// capacidade (em vagões) da linha
        /// </summary>
        public int Capacidade { get; set; }

        /// <summary>
        /// capacidade (em vagões) da linha
        /// </summary>
        public int QtdeVagoesVazios { get; set; }

        /// <summary>
        /// capacidade (em vagões) da linha
        /// </summary>
        public int QtdeVagoesCarregados { get; set; }

        /// <summary>
        /// comprimento da linha (em metros?)
        /// </summary>
        public int Comprimento { get; set; }

        /// <summary>
        /// null se é linha
        /// </summary>
        public string NomeTerminal { get; set; }

        //public virtual ICollection<Carregamento> Carregamentos { get; set; }

        [NotMapped]
        public LinkedList<Vagao> listaDeVagoes { get; set; }
    }
}