using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RumoPatios.Models
{
    public class Carregamento
    {
        public int CarregamentoID { get; set; }

        public DateTime HorarioCarregamento { get; set; }

        public int QtdeVagoes { get; set; }

        public string Terminal { get; set; }

        public string Produto { get; set; }

        public string Cliente { get; set; }

        public int LinhaID { get; set; }
        public virtual Linha Linha { get; set; }

        [NotMapped]
        public double prioridade { get; set; }

    }
}