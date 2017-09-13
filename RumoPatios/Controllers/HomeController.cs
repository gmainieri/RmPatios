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
        int cargaDescarga = 10; //Considere uma descarga/carregamento de 10 vgs/hora, para todos terminais
        int tempoMovEntreLinhas = 30; //minutos
        int maxMovParalelo = 5; //ou seja, tenho 5 LM
        int maxVagoesMov = 60; //cada LM pode manobrar no maximo 60 vagoes

        ResultadoOtimizaData result { get; set; }
        ApplicationDbContext db { get; set; }
        Random rnd { get; set; }
        List<Evento> timeLine { get; set; }

        public ActionResult Index()
        {
            //TODO: criar o view model com todas as tabelas

            this.db = new ApplicationDbContext();

            var vm = new TelaPrincipal(this.db);

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
            this.rnd = new Random();
            this.result = new ResultadoOtimizaData();
            this.db = new ApplicationDbContext();

            var carregamentos = this.db.Carregamentos.ToList();
            var chegadas = this.db.Chegadas.ToList();
            var linhas = this.db.Linhas.ToList();

            var linhasTerminais = linhas.Where(x => String.IsNullOrEmpty(x.NomeTerminal) == false).ToList();
            var linhasDeManobra = linhas.Where(x => String.IsNullOrEmpty(x.NomeTerminal) == true).ToList();

            #region contruir lista de tarefas
            var listaDeTarefas = new List<Tarefa>();

            foreach (var load in carregamentos)
            {
                listaDeTarefas.Add(new Tarefa(load, rnd));
            }

            foreach (var arrival in chegadas)
            {
                listaDeTarefas.Add(new Tarefa(arrival, rnd));
            }

            listaDeTarefas = listaDeTarefas
                .OrderBy(x => x.instante)
                .ThenBy(x => x.prioridade)
                .ToList(); 
            #endregion

            var instantePrimeiraTarefa = listaDeTarefas.Min(x => x.instante);
            this.timeLine = new List<Evento>();
            var vagoesLM = new List<VagaoLM>(); //como vagao nao é uma classe do banco, tenho que criar
            //var vagoesLmLivres = new List<Evento>();
            //var linhasCarregamentoLivres = new List<Evento>();

            for(int i = 0; i < maxMovParalelo; i++)
            {
                //var novoVagao = new VagaoLM (i, instantePrimeiroEvento);
                var novoVagaoLM = new VagaoLM(i);
                vagoesLM.Add(novoVagaoLM);
                //vagoesLmLivres.Add(new Evento(novoVagao, instantePrimeiroEvento));
                this.timeLine.Add(new Evento(novoVagaoLM, instantePrimeiraTarefa)); //LM está inicialmente livre
            }

            foreach (var line in linhasTerminais)
            {
                //line.instanteDeLiberacao = instantePrimeiroEvento; //todas as linhas de carregamento estao livres em t = 0
                line.aleatorio = rnd.NextDouble();
                //linhasCarregamentoLivres.Add(new Evento(line, instantePrimeiroEvento));
                this.timeLine.Add(new Evento(line, instantePrimeiraTarefa)); //linha terminal está inicialmente livre 
            }

            foreach(var line in linhasDeManobra)
            {
                line.vagoesVaziosAtual = line.QtdeVagoesVazios;
                line.vagoesCarregadosAtual = line.QtdeVagoesCarregados;
                line.aleatorio = rnd.NextDouble();
            }

            //this.timeLine.Sort((x, y) => x.instante.CompareTo(y.instante));
            //vagoesLM.Sort((x, y) => x.instanteDeLiberacao.CompareTo(y.instanteDeLiberacao));
            //linhasDeCarregamento.Sort((x, y) => x.instanteDeLiberacao.CompareTo(y.instanteDeLiberacao));
            
            #region algoritmo como agrupamento de eventos por instante
            //while (timeLine.Any())
            //{
            //    timeLine.Sort((x, y) => x.instante.CompareTo(y.instante));
            //    var timeLineAgrupada = timeLine.GroupBy(x => x.instante).ToList();
            //    var vagoesLmLivres = new List<Evento>();
            //    var linhasCarregamentoLivres = new List<Evento>();
            //    var filaCarregamentos = new List<Evento>();
            //    var filaChegadas = new List<Evento>();

            //    foreach (var evento in timeLineAgrupada.First()) //para todos os eventos do instante mais recente
            //    {
            //        if (evento.vagaoLM != null)
            //        {
            //            vagoesLmLivres.Add(evento);
            //        }
            //        else if (evento.linha != null)
            //        {
            //            linhasCarregamentoLivres.Add(evento);
            //        }
            //        else if (evento.carregamento != null)
            //        {
            //            filaCarregamentos.Add(evento);
            //        }
            //        else if (evento.chegada != null)
            //        {
            //            filaChegadas.Add(evento);
            //        }

            //        timeLine.Remove(evento);
            //    }

            //} 
            #endregion
            
            #region algoritmo evento por evento

            linhasTerminais.Sort((x, y) => x.aleatorio.CompareTo(y.aleatorio));
            linhasDeManobra.Sort((x, y) => x.aleatorio.CompareTo(y.aleatorio));

            var vagoesLmLivres = new List<Evento>();
            var linhasTerminaisLivres = new List<Evento>();

            //var filaCarregamentos = new List<Carregamento>();
            //var filaChegadas = new List<Chegada>();

            while (this.timeLine.Any())
            {
                Evento evento;

                #region tratamento de eventos pendentes
                foreach(var job in listaDeTarefas)
                {
                    if(job.carregamento != null)
                    {
                        if (linhasTerminaisLivres.Any(x => x.linhaTerminal.LinhaID == job.carregamento.LinhaID) == false)
                        {
                            continue; //se o terminal está ocupado
                        }

                        if (vagoesLmLivres.Any() == false)
                            continue;

                        evento = vagoesLmLivres[0];

                        var qtdeAtualVagoesVaziosNoPatio = linhasDeManobra.Sum(x => x.vagoesVaziosAtual);

                        if (qtdeAtualVagoesVaziosNoPatio < job.carregamento.QtdeVagoes)
                        {
                            continue; //não trato o eventoAtual - não deve acontecer
                        }

                        this.FazCarregamento(evento, job, linhasDeManobra);

                        //carregamento foi encaminhado, então linha e vagao não estão mais livres
                        linhasTerminaisLivres.RemoveAll(x => x.linhaTerminal.LinhaID == job.carregamento.LinhaID); //apesar de usar All, só deve apagar um
                        vagoesLmLivres.RemoveAt(0);
                    }
                    else if(job.chegada != null)
                    {
                        if (this.timeLine[0].instante >= job.chegada.HorarioChegada)
                        {
                            //TODO: tratar a chegada

                            result.rows.Add(new ResultadoOtimizaDataRow(job.chegada.HorarioChegada, String.Format("Chegada {0}", job.chegada.prefixo), 0));
                            job.concluida = 1;
                        }
                    }
                    else if(job.partida != null)
                    {
                        job.concluida = 1;
                    }
                }

                listaDeTarefas.RemoveAll(x => x.concluida == 1);

                #endregion

                #region tratamento de eventos
                this.timeLine = this.timeLine.OrderBy(x => x.instante)
                    //.ThenBy(x => x.prioridade)
                    //.ThenBy(x => x.chegada == null ? 9999 : x.chegada.ChegadaID)
                    //.ThenBy(x => x.carregamento == null ? 9999 : x.carregamento.CarregamentoID)
                    .ToList();

                evento = this.timeLine[0];

                #region libera vagao LM
                if (evento.vagaoLM != null)
                {
                    result.rows.Add(new ResultadoOtimizaDataRow(evento.instante, String.Format("Vagão LM #{0} liberado",  evento.vagaoLM.Idx), 0));
                    vagoesLmLivres.Add(evento);

                    //if (eventosPendentes.Any(x => x.carregamento != null))
                    //{
                    //    #region ocupar vagao com carregamento
                    //    var eventoAtual = eventosPendentes[0];

                    //    if (linhasTerminaisLivres.Any(x => x.linhaTerminal.LinhaID == eventoAtual.linhaTerminal.LinhaID) == false)
                    //    {
                    //        vagoesLmLivres.Add(evento);
                    //        continue; //se o terminal está ocupado
                    //    }

                    //    var qtdeAtualVagoesVaziosNoPatio = linhasDeManobra.Sum(x => x.vagoesVaziosAtual);

                    //    if (qtdeAtualVagoesVaziosNoPatio < eventoAtual.carregamento.QtdeVagoes)
                    //    {
                    //        vagoesLmLivres.Add(evento);
                    //        continue; //não trato o eventoAtual - não deve acontecer
                    //    }

                    //    TratarCarregamento(evento, eventoAtual, timeLine, linhasDeManobra, result);
                        
                    //    #endregion
                    //}
                    //else
                    //{
                    //    vagoesLmLivres.Add(evento);
                    //}
                }
                #endregion
                #region libera linha terminal
                else if (evento.linhaTerminal != null)
                {
                    if(evento.qtdeVagoesCarregadosLiberados > 0)
                    {
                        evento.linhaTerminal.QtdeVagoesCarregados += evento.qtdeVagoesCarregadosLiberados;
                        var mensagem = String.Format("Terminal {0}: fim do carrgamento de {1} vagões", evento.linhaTerminal.Nome, evento.qtdeVagoesCarregadosLiberados);
                        result.rows.Add(new ResultadoOtimizaDataRow(evento.instante, mensagem , 1));

                        //TODO: neste momento estou com n vagoes para levar de volta da linha terminal pra linha de manobra, criar tarefa pra isso
                    }
                    else
                    {
                        result.rows.Add(new ResultadoOtimizaDataRow(evento.instante, String.Format("Terminal {0} disponível", evento.linhaTerminal.Nome), 0));
                        linhasTerminaisLivres.Add(evento);
                    }

                    //if (eventosPendentes.Any(x => x.carregamento != null) && vagoesLmLivres.Any())
                    //{
                    //    #region ocupar linha com carregamento

                    //    #endregion
                    //}
                    //else
                    //{
                    //    linhasTerminaisLivres.Add(evento);
                    //}
                }
                #endregion
                //#region coloca carregamento na fila de eventos pendentes
                //else if (evento.carregamento != null)
                //{
                //    eventosPendentes.Add(evento);
                //}
                //#endregion
                //#region coloca chegada na fila de eventos pendentes
                //else if (evento.chegada != null)
                //{
                //    //uma chegada implica em decidir em quais linhas de manobra alocar os vagoes
                //    //uma chegada implica em mais vagoes carregados que precisam ser descarregados
                //    //ja neste instante, preciso providenciar as descargas dos vagoes do patio e que acabaram de chegar
                //}
                //#endregion
                //#region coloca partida na fila de eventos pendentes
                //else if (evento.partida != null)
                //{
                //    //partidas ainda não são consideradas
                //}
                //#endregion 
                #endregion

                this.timeLine.RemoveAt(0); //removo o evento que foi tratado
                continue;
            } 
            #endregion

            //result.rows.Sort((x, y) => x.horario.CompareTo(y.horario));
            return result;
        }

        private void FazCarregamento(Evento evento, Tarefa job, List<Linha> linhasDeManobra)
        {
            var linhasDeManobraComVagoesVazios = linhasDeManobra.Where(x => x.vagoesVaziosAtual > 0).ToList();

            int linhasUsadas = 0;
            int qtdeVagoesAtribuidasAcumulada = 0;

            foreach (var line in linhasDeManobraComVagoesVazios)
            {
                linhasUsadas++;
                string acao = "";

                var qtdeRestante = job.carregamento.QtdeVagoes - qtdeVagoesAtribuidasAcumulada;
                var qtdeDaLinha = Math.Min(qtdeRestante, line.vagoesVaziosAtual); //minimo entre quanto falta e quanto tem disponivel na linha
                qtdeVagoesAtribuidasAcumulada += qtdeDaLinha;

                var tempoCarregamento = (double)qtdeDaLinha / cargaDescarga;
                var instanteTerminoCarregamento = evento.instante.AddMinutes(60 * tempoCarregamento + tempoMovEntreLinhas);
                this.timeLine.Add(new Evento(line, job.carregamento.Linha, instanteTerminoCarregamento, qtdeDaLinha)); //termino do carregamento dos n vagoes

                acao = String.Format("Levar {0} vagoes da linha {1} para serem carregados no terminal {2} utilizando vagão LM #{3}",
                    qtdeDaLinha, line.Nome, job.carregamento.Linha.Nome, evento.vagaoLM.Idx);

                line.vagoesVaziosAtual -= qtdeDaLinha;
                result.rows.Add(new ResultadoOtimizaDataRow(evento.instante, acao, 1));
                if (qtdeVagoesAtribuidasAcumulada == job.carregamento.QtdeVagoes)
                    break;
            }

            double tempoCarregamentoTotal = (double)job.carregamento.QtdeVagoes/cargaDescarga; //os tempos de movimentacoes dos vagoes ainda não sao considerados
            this.timeLine.Add(new Evento(job.carregamento.Linha, evento.instante.AddHours(tempoCarregamentoTotal))); //evento de liberacao da linha terminal

            evento.instante = evento.instante.AddMinutes(linhasUsadas * tempoMovEntreLinhas);
            this.timeLine.Add(new Evento(evento.vagaoLM, evento.instante)); //evento de liberacao da LM

            job.concluida = 1;
        }

    }

    
}