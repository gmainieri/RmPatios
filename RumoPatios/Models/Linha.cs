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
        /// </summary>
        public int QtdeVagoesVazios { get; set; }

        /// <summary>
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

        public virtual ICollection<Carregamento> Carregamentos { get; set; }

        //[NotMapped]
        //public DateTime instanteDeLiberacao { get; set; }

        [NotMapped]
        public int vagoesVaziosAtual { get; set; }

        [NotMapped]
        public int vagoesCarregadosAtual { get; set; }

        [NotMapped]
        public double aleatorio { get; set; }

        //[NotMapped]
        //public LinkedList<VagaoLM> listaDeVagoes { get; set; }
    }
}