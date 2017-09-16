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

        [NotMapped]
        public double randLoad { get; set; }

        [NotMapped]
        public double randUnload { get; set; }

        //[NotMapped]
        //public double prioridade { get; set; }

    }
}