using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;

namespace RumoPatios.Models
{
    public class Parametros
    {
        public List<Carregamento> Carregamentos { get; set; }
        public List<Chegada> Chegadas { get; set; }
        public List<Linha> Linhas { get; set; }


        public Parametros(ApplicationDbContext db)
        {
            this.Carregamentos = db.Carregamentos.Include(x => x.Linha).AsNoTracking().ToList();
            this.Chegadas = db.Chegadas.AsNoTracking().ToList();
            this.Linhas = db.Linhas.AsNoTracking().ToList();
        }
        
    }
}