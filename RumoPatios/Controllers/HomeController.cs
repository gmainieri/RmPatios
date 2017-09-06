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
            var rnd = new Random(1978);

            var cargaDescarga = 10; //Considere uma descarga/carregamento de 10 vgs/hora, para todos terminais
            var tempoMovEntreLinhas = 30; //minutos
            var maxMovParalelo = 5; //ou seja, tenho 5 LM
            var maxVagoesMov = 60; //cada LM pode manobrar no maximo 60 vagoes
            
            var db = new ApplicationDbContext();

            var carregamentos = db.Carregamentos.ToList();
            var chegadas = db.Chegadas.ToList();
            var linhas = db.Linhas.ToList();

            var linhasTerminais = linhas.Where(x => String.IsNullOrEmpty(x.NomeTerminal) == false).ToList();
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

            //timeLine.Sort((x, y) => x.instante.CompareTo(y.instante));

            var instantePrimeiroEvento = timeLine.Min(x => x.instante);
            var vagoesLM = new List<VagaoLM>(); //como vagao nao é uma classe do banco, tenho que criar
            //var vagoesLmLivres = new List<Evento>();
            //var linhasCarregamentoLivres = new List<Evento>();

            var carregamentosEncaminhados = new List<Carregamento>();
            var chegadasEncaminhadas = new List<Chegada>();

            for(int i = 0; i < maxMovParalelo; i++)
            {
                var novoVagao = new VagaoLM (i, instantePrimeiroEvento);
                vagoesLM.Add(novoVagao);
                //vagoesLmLivres.Add(new Evento(novoVagao, instantePrimeiroEvento));
            }

            foreach (var line in linhasTerminais)
            {
                line.instanteDeLiberacao = instantePrimeiroEvento; //todas as linhas de carregamento estao livres em t = 0
                //linhasCarregamentoLivres.Add(new Evento(line, instantePrimeiroEvento));
            }

            foreach(var line in linhasDeManobra)
            {
                line.vagoesVaziosAtual = line.QtdeVagoesVazios;
                line.vagoesCarregadosAtual = line.QtdeVagoesCarregados;
                line.aleatorio = rnd.NextDouble();
            }

            timeLine.Sort((x, y) => x.instante.CompareTo(y.instante));
            //vagoesLM.Sort((x, y) => x.instanteDeLiberacao.CompareTo(y.instanteDeLiberacao));
            //linhasDeCarregamento.Sort((x, y) => x.instanteDeLiberacao.CompareTo(y.instanteDeLiberacao));
            //timeLine = timeLine.OrderBy(x => x.instante)
            //    .ThenBy(x => x.chegada == null ? 9999 : x.chegada.ChegadaID)
            //    .ThenBy(x => x.carregamento == null ? 9999 : x.carregamento.CarregamentoID)
            //    .ToList();
            
            while(timeLine.Any())
            {
                var carregamento = timeLine[0].carregamento;
                var chegada = timeLine[0].chegada;

                var qtdeAtualVagoesVazios = linhasDeManobra.Sum(x => x.vagoesVaziosAtual);
                var qtdeAtualVagoesCarregados = linhasDeManobra.Sum(x => x.vagoesCarregadosAtual);

                if(carregamento != null)
                {
                    //var qtdeMinVagoesLM = (int) Math.Ceiling((double) carregamento.QtdeVagoes / maxVagoesMov);

                    //if (qtdeMinVagoesLM > vagoesLM.Count)
                    //    break; //nunca deve acontecer

                    if (qtdeAtualVagoesVazios < carregamento.QtdeVagoes)
                        break; //para debugar (provavelmente não deve acontecer, complica um pouco se acontecer, pq teriamos que colocar o carregamento em espera)

                    var quantidadesDeVagoesVaziosPorLinha = (int)Math.Floor((double)carregamento.QtdeVagoes / vagoesLM.Count);

                    #region escolher de quais linhas virao os vagoes e atualizar as quantidades nestas linhas

                    var linhasDeManobraEmOrdem = linhasDeManobra.Where(x => x.vagoesVaziosAtual >= quantidadesDeVagoesVaziosPorLinha).OrderBy(x => x.aleatorio).ToList();

                    if (linhasDeManobraEmOrdem.Count() < vagoesLM.Count)
                        break; //não existem n linhas com pelo menos X quantidade de vagoes vazios por linha

                    foreach(var linhaM in linhasDeManobraEmOrdem)
                    {
                        linhaM.QtdeVagoesVazios -= quantidadesDeVagoesVaziosPorLinha;
                    }

                    foreach(var linhaT in linhasTerminais)
                    {
                        linhaT.instanteDeLiberacao.AddMinutes(tempoMovEntreLinhas); //adicionar o tempo de carregamento
                    }

                    
                    #endregion

                    #region atualizo os instantes de liberacao dos vagoes ocupados
                    for (int i = 0; i < vagoesLM.Count; i++)
                    {
                        vagoesLM[i].instanteDeLiberacao.AddMinutes(tempoMovEntreLinhas);
                    } 
                    #endregion

                    vagoesLM.Sort((x, y) => x.instanteDeLiberacao.CompareTo(y.instanteDeLiberacao));
                    carregamentosEncaminhados.Add(carregamento);
                }

                if(chegada != null)
                {
                    //uma chegada implica em decidir em quais linhas de manobra alocar os vagoes
                    //uma chegada implica em mais vagoes carregados que precisam ser descarregados
                    //ja neste instante, preciso providenciar as descargas dos vagoes do patio e que acabaram de chegar
                    chegadasEncaminhadas.Add(chegada);
                }


                timeLine.RemoveAt(0);
                continue;
            }

            var result = new ResultadoOtimizaData();
            return result;
        }

    }
}