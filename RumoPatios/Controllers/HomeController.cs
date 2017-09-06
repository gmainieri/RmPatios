using RumoPatios.Models;
using RumoPatios.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RumoPatios.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //TODO: criar o view model com todas as tabelas

            var db = new ApplicationDbContext();

            var vm = new TelaPrincipal(db);

            return View("Index", vm);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult ExecutaCompleto()
        {
            try
            {
                var vm = this.Otimizador();

                return View("_RespostaOtimiza", vm);
            }
            catch
            {

            }

            return View();
        }

        internal ResultadoOtimizaData Otimizador()
        {
            var cargaDescarga = 10; //Considere uma descarga/carregamento de 10 vgs/hora, para todos terminais
            var tempoMovEntreLinhas = 30; //minutos
            var maxMovParalelo = 5; //ou seja, tenho 5 LM
            var maxVagoesMov = 60; //cada LM pode manobrar no maximo 60 vagoes
            
            var db = new ApplicationDbContext();

            var carregamentos = db.Carregamentos.ToList();
            var chegadas = db.Chegadas.ToList();
            var linhas = db.Linhas.ToList();

            var linhasDeCarregamento = linhas.Where(x => String.IsNullOrEmpty(x.NomeTerminal) == false).ToList();
            var linhasDeManobra = linhas.Where(x => String.IsNullOrEmpty(x.NomeTerminal) == true).ToList();

            //var primeiroCarregamento = carregamentos.Min(x => x.HorarioCarregamento);
            //var primeiraChegada = chegadas.Min(x => x.HorarioChegada);

            var timeLine = new List<Evento>();

            foreach (var carrega in carregamentos)
            {
                timeLine.Add(new Evento(carrega));
            }

            foreach(var chega in chegadas)
            {
                timeLine.Add(new Evento(chega));
            }

            timeLine.Sort((x, y) => x.instante.CompareTo(y.instante));

            var primeiroEvento = timeLine.Min(x => x.instante);
            var vagoesLM = new List<Vagao>();

            for(int i = 0; i < maxMovParalelo; i++)
            {
                var novoVagao = new Vagao { Idx = i, nome = "LM", tipo = 2 };
                vagoesLM.Add(novoVagao);
                timeLine.Add(new Evento(novoVagao, primeiroEvento));
            }

            foreach(var line in linhasDeCarregamento)
            {
                timeLine.Add(new Evento(line, primeiroEvento));
            }

            timeLine.Sort((x, y) => x.instante.CompareTo(y.instante));
            
            var result = new ResultadoOtimizaData();
            return result;
        }

    }
}