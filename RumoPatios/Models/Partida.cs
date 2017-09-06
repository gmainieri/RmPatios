using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RumoPatios.Models
{
    public class Partida
    {
        public int PartidaID { get; set; }

        public DateTime HorarioPartida { get; set; }

        public string prefixo { get; set; }
        public int QtdeVagoesCarregados { get; set; }
        public int QtdeVagoesVazio { get; set; }

        //[NotMapped]
        //public int qtdeVagoesTotais { get; set; }

    }
}