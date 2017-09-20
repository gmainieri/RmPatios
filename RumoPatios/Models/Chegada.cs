using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RumoPatios.Models
{
    public class Chegada
    {
        public int ChegadaID { get; set; }

        public DateTime HorarioChegada { get; set; }

        public string prefixo { get; set; }
        public int QtdeVagoesCarregados { get; set; }
        public int QtdeVagoesVazio { get; set; }

        //assim tem o mesmo efeito de not mapped
        //public double randLoad = 0.0;
        //public double randUnload = 0.0;

        /// <summary>
        /// fracionamento dos vagoes carregados
        /// </summary>
        [NotMapped]
        public double randLoaded { get; set; }

        /// <summary>
        /// fracionamento dos vagoes vazios
        /// </summary>
        [NotMapped]
        public double randEmpty { get; set; }

        //chegadas não tem prioridade pq elas nunca são colocadas na lista de tarefas
        //[NotMapped]
        //public double prioridade { get; set; }

    }
}