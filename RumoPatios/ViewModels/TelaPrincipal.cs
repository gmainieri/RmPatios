using RumoPatios.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RumoPatios.ViewModels
{
    public class TelaPrincipal
    {
        public List<Linha> linhas { get; set; }
        public List<Partida> partidas { get; set; }
        public List<Chegada> chegadas { get; set; }
        public List<Carregamento> carregamentos { get; set; }

        public TelaPrincipal()
        {
            this.linhas = new List<Linha>();
            this.partidas = new List<Partida>();
            this.chegadas = new List<Chegada>();
            this.carregamentos = new List<Carregamento>();
        }

        public TelaPrincipal(ApplicationDbContext db)
        {
            this.linhas = db.Linhas.ToList();
            this.partidas = db.Partidas.ToList();
            this.chegadas = db.Chegadas.ToList();
            this.carregamentos = db.Carregamentos.ToList();
        }
    }
}