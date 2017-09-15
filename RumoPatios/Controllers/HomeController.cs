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
        /// <summary>
        /// Considere uma descarga/carregamento de 10 vgs/hora, para todos terminais
        /// </summary>
        int cargaDescarga = 10; 

        /// <summary>
        /// tempo de manobra entre linhas (em minutos)
        /// </summary>
        int tempoMovEntreLinhas = 30;
        
        /// <summary>
        /// //ou seja, tenho 5 LM
        /// </summary>
        int maxMovParalelo = 5; 

        /// <summary>
        /// //cada LM pode manobrar no maximo 60 vagoes
        /// </summary>
        int maxVagoesMov = 60; 

        ResultadoOtimizaData result { get; set; }
        ApplicationDbContext db { get; set; }
        Random rand { get; set; }
        List<Evento> timeLine { get; set; }
        List<Tarefa> listaDeTarefas { get; set; }

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
            //this.Otimizador();

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
            this.rand = new Random();
            this.result = new ResultadoOtimizaData();
            this.db = new ApplicationDbContext();
            this.timeLine = new List<Evento>();
            this.listaDeTarefas = new List<Tarefa>();

            ////TODO: implementar lista generica como abaixo
            ////c# order by using reflection
            //var timeLineTest = new List<object>();
            //timeLineTest.Add(new Evento(this.rand));
            //timeLineTest.Add(new Evento(this.rand));
            //timeLineTest = timeLineTest.OrderBy(x => x.GetType().GetProperty("instante").GetValue(x)).ToList();
            //timeLineTest = timeLineTest.OrderByDescending(x => x.GetType().GetProperty("instante").GetValue(x)).ToList();

            var carregamentos = this.db.Carregamentos.ToList();
            var chegadas = this.db.Chegadas.ToList();
            var linhas = this.db.Linhas.ToList();

            var linhasTerminais = linhas.Where(x => String.IsNullOrEmpty(x.NomeTerminal) == false).ToList();
            var linhasDeManobra = linhas.Where(x => String.IsNullOrEmpty(x.NomeTerminal) == true).ToList();

            #region contruir lista de tarefas
            foreach (var load in carregamentos)
            {
                listaDeTarefas.Add(new Tarefa(load, rand));
                timeLine.Add(new Evento(load.HorarioCarregamento)); //adiciono um evento vazio, apenas pra dar um tick no relogio
            }

            foreach (var arrival in chegadas)
            {
                listaDeTarefas.Add(new Tarefa(arrival, rand));
                timeLine.Add(new Evento(arrival.HorarioChegada)); //adiciono um evento vazio, apenas pra dar um tick no relogio
            }

            
            #endregion

            var instantePrimeiraTarefa = listaDeTarefas.Min(x => x.instante);
            
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
                line.prioridade = rand.NextDouble();
                //linhasCarregamentoLivres.Add(new Evento(line, instantePrimeiroEvento));
                this.timeLine.Add(new Evento(line, instantePrimeiraTarefa)); //linha terminal está inicialmente livre 
            }

            foreach(var line in linhasDeManobra)
            {
                line.vagoesVaziosAtual = line.QtdeVagoesVazios;
                line.vagoesCarregadosAtual = line.QtdeVagoesCarregados;
                line.prioridade = rand.NextDouble();

                if(line.QtdeVagoesCarregados > 0)
                {
                    //cria tarefas de descarga (uma para cada linha de manobra, por enquanto)
                    listaDeTarefas.Add(new Tarefa(new Descarga(line), instantePrimeiraTarefa, line.prioridade));
                }
                
            }

            listaDeTarefas = listaDeTarefas.OrderBy(x => x.instante)
                .ThenBy(x => x.prioridade)
                .ToList(); 

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

            linhasTerminais.Sort((x, y) => x.prioridade.CompareTo(y.prioridade));
            linhasDeManobra.Sort((x, y) => x.prioridade.CompareTo(y.prioridade));

            var vagoesLmLivres = new List<Evento>();
            var linhasTerminaisLivres = new List<Evento>();

            //var filaCarregamentos = new List<Carregamento>();
            //var filaChegadas = new List<Chegada>();

            while (this.timeLine.Any())
            {
                this.timeLine = this.timeLine.OrderBy(x => x.instante)
                    //.ThenBy(x => x.prioridade)
                    .ThenBy(x => x.vagaoLM == null ? 9999 : x.vagaoLM.Idx)
                    .ThenBy(x => x.linhaTerminal == null ? 9999 : x.linhaTerminal.LinhaID)
                    .ToList();

                //Evento evento;

                #region contabilizar o evento mais atual

                var evento = this.timeLine[0];

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
                    //if(evento.qtdeVagoesLiberados > 0)
                    //{
                    //    //evento.linhaTerminal.QtdeVagoesCarregados += evento.qtdeVagoesLiberados;
                    //    var mensagem = String.Format("Terminal {0}: fim de carga ({1} vagões)", evento.linhaTerminal.Nome, evento.qtdeVagoesLiberados);
                    //    result.rows.Add(new ResultadoOtimizaDataRow(evento.instante, mensagem , 1));
                    //}
                    //else if(evento.qtdeVagoesLiberados < 0)
                    //{
                    //    //evento.linhaTerminal.QtdeVagoesVazios += -1 * evento.qtdeVagoesLiberados;
                    //    var mensagem = String.Format("Terminal {0}: fim de descarga ({1} vagões)", evento.linhaTerminal.Nome, -1 * evento.qtdeVagoesLiberados);
                    //    result.rows.Add(new ResultadoOtimizaDataRow(evento.instante, mensagem, 1));
                    //}
                    //else
                    //{
                    //    result.rows.Add(new ResultadoOtimizaDataRow(evento.instante, String.Format("Terminal {0} disponível", evento.linhaTerminal.Nome), 0));
                    //    linhasTerminaisLivres.Add(evento);
                    //}

                    result.rows.Add(new ResultadoOtimizaDataRow(evento.instante, String.Format("Terminal {0} disponível", evento.linhaTerminal.Nome), 0));
                    linhasTerminaisLivres.Add(evento);
                    
                }
                #endregion
                //else if (evento.carregamento != null)
                //{
                //    eventosPendentes.Add(evento);
                //}
                //else if (evento.chegada != null)
                //{
                //}
                //else if (evento.partida != null)
                //{
                //    //partidas ainda não são consideradas
                //}

                var ultimoInstanteTratado = evento.instante;
                this.timeLine.RemoveAt(0); //removo o evento que foi tratado
                #endregion

                if(this.timeLine.Any() == false || this.timeLine[0].instante > ultimoInstanteTratado)
                {
                    var tarefasEmOrdem = listaDeTarefas.OrderBy(x => x.instante)
                        .ThenBy(x => x.prioridade)
                        //TODO: add terceira prioridade?
                        .ToList();

                    #region resolver as tarefas da fila
                    foreach (var job in tarefasEmOrdem)
                    {
                        //TODO: aqui acredito que tenha que existir uma verificacao se a tarefa esta disponivel no instante considerado (tarefas como carregamento, estão disponíveis antes do instante, mas a maioria nao) - pq o instante do carregamento é um limite, data de entrega, os demais são instantes de acontecimento mesmo
                        if (job.carregamento != null)
                        {
                            if (linhasTerminaisLivres.Any(x => x.linhaTerminal.LinhaID == job.carregamento.LinhaID) == false)
                            {
                                continue; //se o terminal está ocupado
                            }

                            if (vagoesLmLivres.Any() == false)
                                continue;

                            var vagaoDesignado = vagoesLmLivres[0];

                            var qtdeAtualVagoesVaziosNoPatio = linhasDeManobra.Sum(x => x.vagoesVaziosAtual);

                            if (qtdeAtualVagoesVaziosNoPatio < job.carregamento.QtdeVagoes)
                            {
                                continue; //não trato o eventoAtual - não deve acontecer
                            }

                            this.FazCarregamento(vagaoDesignado, job, linhasDeManobra);

                            //carregamento foi encaminhado, então linha e vagao não estão mais livres
                            linhasTerminaisLivres.RemoveAll(x => x.linhaTerminal.LinhaID == job.carregamento.LinhaID); //apesar de usar All, só deve apagar um
                            vagoesLmLivres.RemoveAt(0);
                        }
                        else if (job.descarga != null)
                        {
                            if (linhasTerminaisLivres.Any(x => x.linhaTerminal.Capacidade >= job.descarga.linhaManobra.QtdeVagoesCarregados) == false)
                            {
                                continue; //se o terminal está ocupado
                            }

                            if (vagoesLmLivres.Any() == false)
                                continue;

                            var vagaoLivre = vagoesLmLivres[0];

                            this.FazDescarga(vagaoLivre, job, linhasTerminaisLivres);

                            //carregamento foi encaminhado, então linha e vagao não estão mais livres
                            vagoesLmLivres.RemoveAt(0);

                        }
                        else if (job.movimento != null && ultimoInstanteTratado >= job.instante)
                        {
                            if (vagoesLmLivres.Any() == false)
                                continue;

                            var vagaoDesignado = vagoesLmLivres[0];

                            vagoesLmLivres.RemoveAt(0); //movimento foi encaminhado, então ocupo o vagao

                            vagaoDesignado.instante = ultimoInstanteTratado.AddMinutes(this.tempoMovEntreLinhas);

                            this.timeLine.Add(vagaoDesignado);

                            var acao = "";

                            if (job.movimento.qtdeVagoes > 0)
                            {
                                acao = String.Format("Levar {0} vagoes carregados do terminal {1} para a linha {2} utilizando vagão LM #{3}",
                                    job.movimento.qtdeVagoes, job.movimento.linhaOrigem.Nome, job.movimento.linhaDestino.Nome, vagaoDesignado.vagaoLM.Idx);
                            }
                            else
                            {
                                acao = String.Format("Levar {0} vagoes vazios do terminal {1} para a linha {2} utilizando vagão LM #{3}",
                                    -1 * job.movimento.qtdeVagoes, job.movimento.linhaOrigem.Nome, job.movimento.linhaDestino.Nome, vagaoDesignado.vagaoLM.Idx);
                            }

                            result.rows.Add(new ResultadoOtimizaDataRow(ultimoInstanteTratado, acao, 1));

                            job.concluida = 1;
                        }
                        else if (job.chegada != null && ultimoInstanteTratado >= job.chegada.HorarioChegada)
                        {
                            //TODO: tratar a chegada
                            //uma chegada implica em decidir em quais linhas de manobra alocar os vagoes
                            //uma chegada implica em mais vagoes carregados que precisam ser descarregados
                            //ja neste instante, preciso providenciar as descargas dos vagoes do patio e que acabaram de chegar, criar tarefas Movimento?

                            result.rows.Add(new ResultadoOtimizaDataRow(job.chegada.HorarioChegada, String.Format("Chegada {0}", job.chegada.prefixo), 0));
                            job.concluida = 1;
                        }
                        else if (job.partida != null)
                        {
                            job.concluida = 1;
                        }
                    }

                    listaDeTarefas.RemoveAll(x => x.concluida == 1);

                    #endregion
                } //fim de if
            } //fim de while 
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
                var instanteTerminoCarregamento = evento.instante.AddMinutes(tempoMovEntreLinhas + (60 * tempoCarregamento));
                //this.timeLine.Add(new Evento(line, job.carregamento.Linha, instanteTerminoCarregamento, qtdeDaLinha)); //termino do carregamento dos n vagoes

                var novaTarefaMov = new Movimento(job.carregamento.Linha, line, qtdeDaLinha); //cria uma tarefa de solicitacao de movimento da linha terminal para a linha de manobra
                listaDeTarefas.Add(new Tarefa(novaTarefaMov, instanteTerminoCarregamento, job.prioridade));

                acao = String.Format("Levar {0} vagoes da linha {1} para carregamento no terminal {2} utilizando vagão LM #{3}",
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


        private void FazDescarga(Evento eventoVagao, Tarefa job, List<Evento> linhasTerminaisLivres)
        {
            //int linhasUsadas = 0;
            //int qtdeVagoesAtribuidasAcumulada = 0;
            int t = 0;

            for (t = 0; t < linhasTerminaisLivres.Count; t++)
            {
                var terminal = linhasTerminaisLivres[t];
                if (terminal.linhaTerminal.Capacidade < job.descarga.linhaManobra.QtdeVagoesCarregados)
                    continue;
            
                var tempoDescarga = (double)job.descarga.linhaManobra.QtdeVagoesCarregados / this.cargaDescarga;
                var instanteTerminoDescarga = eventoVagao.instante.AddMinutes(tempoMovEntreLinhas + (60 * tempoDescarga));
                //var instanteLiberacaoDoTerminal = instanteTerminoDescarga.AddMinutes(tempoMovEntreLinhas);
                //this.timeLine.Add(new Evento(job.descarga.linhaOrigem, terminal.linhaTerminal, instanteTerminoDescarga, -1 * job.descarga.linhaOrigem.QtdeVagoesCarregados)); //termino da descarga dos n vagoes
                //this.timeLine.Add(new Evento(terminal.linhaTerminal, instanteLiberacaoDoTerminal)); //evento de liberacao da linha terminal

                var novaTarefaMov = new Movimento(terminal.linhaTerminal, job.descarga.linhaManobra, - 1 * job.descarga.linhaManobra.QtdeVagoesCarregados); //cria uma tarefa de solicitacao de movimento da linha terminal para a linha de manobra
                listaDeTarefas.Add(new Tarefa(novaTarefaMov, instanteTerminoDescarga , job.prioridade));

                string acao = String.Format(
                    "Levar {0} vagoes da linha {1} para descarga no terminal {2} utilizando vagão LM #{3}",
                    job.descarga.linhaManobra.QtdeVagoesCarregados, 
                    job.descarga.linhaManobra.Nome, 
                    terminal.linhaTerminal.Nome, 
                    eventoVagao.vagaoLM.Idx
                    );

                this.result.rows.Add(new ResultadoOtimizaDataRow(eventoVagao.instante, acao, 1));

                break;
            }

            linhasTerminaisLivres.RemoveAt(t);

            eventoVagao.instante = eventoVagao.instante.AddMinutes(tempoMovEntreLinhas);
            this.timeLine.Add(new Evento(eventoVagao.vagaoLM, eventoVagao.instante)); //evento de liberacao da LM

            job.concluida = 1;
        }

    }

    
}