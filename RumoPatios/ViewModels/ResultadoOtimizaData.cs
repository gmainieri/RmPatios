using RumoPatios.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Entity;

namespace RumoPatios.ViewModels
{
    public class ResultadoOtimizaData
    {
        public int FO { get; set; }

        public List<Carregamento> Carregamentos { get; set; }
        public List<Chegada> Chegadas { get; set; }
        public List<Linha> Linhas { get; set; }

        public List<ResultadoOtimizaDataRow> rows { get; set; }

        public ResultadoOtimizaData(ApplicationDbContext db)
        {
            //this.Carregamentos = new List<Carregamento>();
            //this.Chegadas = new List<Chegada>();
            //this.Linhas = new List<Linha>();

            this.Carregamentos = db.Carregamentos.Include(x => x.Linha).AsNoTracking().ToList();
            this.Chegadas = db.Chegadas.AsNoTracking().ToList();
            this.Linhas = db.Linhas.AsNoTracking().ToList();

            this.rows = new List<ResultadoOtimizaDataRow>();

            //for(int i = 0; i < 5; i++)
            //{
            //    this.rows.Add(new ResultadoOtimizaDataRow(i));
            //    System.Threading.Thread.Sleep(50);
            //}
        }
    }

    public class ResultadoOtimizaDataRow
    {
        public DateTime horario { get; set; }
        public string acao { get; set; }
        public int qtdeManobras { get; set; }

        public ResultadoOtimizaDataRow()
        {
            

            //this.horario = DateTime.Now;
            //this.acao = "";
        }

        //public ResultadoOtimizaDataRow(int delay)
        //{
        //    this.horario = DateTime.Now.AddHours(Convert.ToDouble(delay));
        //    this.acao = "";
        //}

        public ResultadoOtimizaDataRow(DateTime instante, string Acao, int nManobras)
        {
            this.horario = instante;
            this.acao = Acao;
            this.qtdeManobras = nManobras;
        }
    }
}