using RumoPatios.Models;
using RumoPatios.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;

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

        DateTime instanteInicial = new DateTime(2017, 08, 16, 4, 0, 0);

        //ResultadoOtimizaData result { get; set; }
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
            //this.Decodificador(); //para debugar

            try
            {
                this.rand = new Random();
                this.db = new ApplicationDbContext();
                //this.db.Configuration.LazyLoadingEnabled = false;

                //int maxIndividous = 100;
                int numGeracoes = 50;
                int pop = 100;
                int popElite = 20;
                int popMutante = 20;
                double prob = 0.55;

                //var parametros = new Parametros(this.db);

                ResultadoOtimizaData pai1, pai2;

#if DEBUG
                numGeracoes = 50;
#endif
                var populacao = new List<ResultadoOtimizaData>(pop + 10);

                #region gera pop inicial
                for (int i = 0; i < pop; i++)
                {
                    var result = new ResultadoOtimizaData(this.db);

                    this.geraMutante(result);
                    populacao.Add(this.Decodificador(result));
                } 
                #endregion

                //var teste = String.Join(";", vmList.Select(x => x.FO).ToList());

                populacao.Sort((x, y) => x.FO.CompareTo(y.FO));

                for (int geracao = 1; geracao <= numGeracoes; geracao++)
                {
                    var nextPop = new List<ResultadoOtimizaData>(pop + 10);

                    #region copia dos individuos de elite
                    while (nextPop.Count < popElite)
                    {
                        nextPop.Add(populacao[0]); //copia o primeiro individuo da pop anterior para a prox pop
                        populacao.RemoveAt(0); //remove o individuo que acabou de ser copiado
                    } 
                    #endregion

                    #region faz cruzamentos
                    while (nextPop.Count < pop - popMutante)
                    {
                        try
                        {
                            pai1 = nextPop.ElementAt(rand.Next(0, popElite)); //sorteio um elite
                            pai2 = populacao.ElementAt(rand.Next(0, pop - popElite)); //sorteio um não-elite
                        }
                        catch
                        {
                            //caso de erro nos sorteios (nao deve acontecer)
                            pai1 = nextPop[0];
                            pai2 = populacao[0];
                        }


                        var filho = new ResultadoOtimizaData(this.db);

                        //pai1.Chegadas.Sort((x, y) => x.ChegadaID.CompareTo(y.ChegadaID));
                        //pai2.Chegadas.Sort((x, y) => x.ChegadaID.CompareTo(y.ChegadaID));
                        foreach (var arrival in filho.Chegadas)
                        {
                            if (rand.NextDouble() > prob)
                            {
                                arrival.randLoad = pai2.Chegadas[arrival.ChegadaID - 1].randLoad;
                                arrival.randUnload = pai2.Chegadas[arrival.ChegadaID - 1].randUnload;
                            }
                            else
                            {
                                arrival.randLoad = pai1.Chegadas[0].randLoad;
                                arrival.randUnload = pai1.Chegadas[0].randUnload;
                            }
                        }

                        //pai1.Linhas.Sort((x, y) => x.LinhaID.CompareTo(y.LinhaID));
                        //pai2.Linhas.Sort((x, y) => x.LinhaID.CompareTo(y.LinhaID));
                        for (int i = 0; i < filho.Linhas.Count; i++)
                        {
                            var line = filho.Linhas[i];
                            if (rand.NextDouble() > prob)
                            {
                                line.prioridade = pai2.Linhas[i].prioridade;
                            }
                            else
                            {
                                line.prioridade = pai1.Linhas[i].prioridade;
                            }
                        }
                                

                        //pai1.Carregamentos.Sort((x, y) => x.CarregamentoID.CompareTo(y.CarregamentoID));
                        //pai2.Carregamentos.Sort((x, y) => x.CarregamentoID.CompareTo(y.CarregamentoID));
                        foreach (var load in filho.Carregamentos)
                            if (rand.NextDouble() > prob)
                                load.prioridade = pai2.Carregamentos[load.CarregamentoID - 1].prioridade;
                            else
                                load.prioridade = pai1.Carregamentos[load.CarregamentoID - 1].prioridade;

                        nextPop.Add(this.Decodificador(filho));


                    } 
                    #endregion

                    #region completa a populacao com mutantes
                    while (nextPop.Count < pop)
                    {
                        var novoMut = new ResultadoOtimizaData(this.db);
                        this.geraMutante(novoMut);
                        nextPop.Add(this.Decodificador(novoMut));
                    } 
                    #endregion

                    nextPop.Sort((x, y) => x.FO.CompareTo(y.FO));

                    populacao = nextPop;

                }

                return View("_RespostaOtimiza", populacao[0]);
            }
            catch(Exception ex)
            {

            }

            return View();
        }

        internal void geraMutante(ResultadoOtimizaData result)
        {
            foreach(var arrival in result.Chegadas)
            {
                arrival.randLoad = 0.10 + 0.50 * (this.rand.NextDouble()); //blocos do carregamento, são de no mínimo 10% < X < 60%
                arrival.randUnload = 0.25 + 0.75 * (this.rand.NextDouble());
            }

            foreach(var line in result.Linhas)
            {
                line.prioridade = this.rand.NextDouble();
                //if (String.IsNullOrEmpty(line.NomeTerminal))
                //{
                //    line.vagoesCarregadosAtual = line.QtdeVagoesCarregados;
                //    line.vagoesVaziosAtual = line.QtdeVagoesVazios;
                //}
                
            }

            result.Carregamentos.ForEach(x => x.prioridade = this.rand.NextDouble());
        }

        internal ResultadoOtimizaData Decodificador(ResultadoOtimizaData result)
        {
            //this.rand = new Random();
            //this.db = new ApplicationDbContext();
            //this.result = new ResultadoOtimizaData(this.db);
            this.timeLine = new List<Evento>(100);
            this.listaDeTarefas = new List<Tarefa>(100);

            ////TODO: implementar lista generica como abaixo
            ////c# order by using reflection
            //var timeLineTest = new List<object>();
            //timeLineTest.Add(new Evento(this.rand));
            //timeLineTest.Add(new Evento(this.rand));
            //timeLineTest = timeLineTest.OrderBy(x => x.GetType().GetProperty("instante").GetValue(x)).ToList();
            //timeLineTest = timeLineTest.OrderByDescending(x => x.GetType().GetProperty("instante").GetValue(x)).ToList();

            //var carregamentos = this.db.Carregamentos.ToList();
            //var chegadas = this.db.Chegadas.ToList();
            //var linhas = this.db.Linhas.ToList();

            var linhasTerminais = result.Linhas.Where(x => String.IsNullOrEmpty(x.NomeTerminal) == false).ToList();
            var linhasDeManobra = result.Linhas.Where(x => String.IsNullOrEmpty(x.NomeTerminal) == true).ToList();

            #region contruir lista de tarefas
            foreach (var load in result.Carregamentos)
            {
                int qtdeTotal = 0;
                int qtdeAtual = 0;
                int qtdeRestante = 0;

                while(qtdeTotal < load.QtdeVagoes)
                {
                    qtdeRestante = load.QtdeVagoes - qtdeTotal;
                    qtdeAtual = Math.Min(qtdeRestante, (int)Math.Floor((0.30 + 0.40 * load.prioridade) * load.QtdeVagoes));
                    qtdeTotal += qtdeAtual;

                    listaDeTarefas.Add(new Tarefa(load, qtdeAtual));
                }
                
                timeLine.Add(new Evento(load.HorarioCarregamento)); //adiciono um evento vazio, apenas pra dar um tick no relogio
            }

            foreach (var arrival in result.Chegadas)
            {
                //arrival.randLoad = 0.20 + 0.80 * (this.rand.NextDouble()); //blocos do carregamento, são de no mínimo 20%
                //arrival.randUnload = 0.25 + 0.75 * (this.rand.NextDouble());

                int loadTotal = 0;
                int emptyTotal = 0;
                int qtdeAtual = 0;
                int qtdeRestante = 0;

                //TODO: usar este fracionamento como modelo para o fracionamento dos carregamentos (que ainda não existe)
                #region as chegadas são divididas em blocos de Tarefas, para assim facilitar a designação dos vagoes às linhas de manobra
                //os vagoes carregados chegam na frente do comboio, por isso prioridade 0.0 e dos vazios 1.0
                while (loadTotal < arrival.QtdeVagoesCarregados)
                {
                    qtdeRestante = arrival.QtdeVagoesCarregados - loadTotal;
                    qtdeAtual = Math.Min(qtdeRestante, (int)Math.Floor(arrival.randLoad * arrival.QtdeVagoesCarregados));
                    loadTotal += qtdeAtual;

                    listaDeTarefas.Add(new Tarefa(arrival, qtdeAtual, 0.0)); 
                }

                while (emptyTotal < arrival.QtdeVagoesVazio)
                {
                    qtdeRestante = arrival.QtdeVagoesVazio - emptyTotal;
                    qtdeAtual = Math.Min(qtdeRestante, (int)Math.Floor(arrival.randUnload * arrival.QtdeVagoesVazio));
                    emptyTotal += qtdeAtual;

                    listaDeTarefas.Add(new Tarefa(arrival, -1 * qtdeAtual, 1.0));
                } 
                #endregion

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
                //linhasCarregamentoLivres.Add(new Evento(line, instantePrimeiroEvento));
                line.vagoesVaziosAtual = line.QtdeVagoesVazios;
                line.vagoesCarregadosAtual = line.QtdeVagoesCarregados;

                if (line.QtdeVagoesCarregados > 0)
                {
                    //var linhaDeManobraEscolhida = linhasDeManobra.FirstOrDefault(x => x.QtdeVagoesCarregados + x.QtdeVagoesVazios + line.QtdeVagoesCarregados <= x.Capacidade);
                    //var novaTarefaMov = new Transporte(line, linhaDeManobraEscolhida == null ? linhasDeManobra.First() : linhaDeManobraEscolhida, line.QtdeVagoesCarregados, false); //cria uma tarefa de solicitacao de transporte da linha terminal para a linha de manobra
                    var novaTarefaMov = new Transporte(line, line.QtdeVagoesCarregados, false); //cria uma tarefa de solicitacao de transporte da linha terminal
                    listaDeTarefas.Add(new Tarefa(novaTarefaMov, instantePrimeiraTarefa, line.prioridade));
                }
                else if (line.QtdeVagoesVazios > 0)
                {
                    var novaTarefaMov = new Transporte(line, line.QtdeVagoesVazios, true); //cria uma tarefa de solicitacao de transporte da linha terminal
                    listaDeTarefas.Add(new Tarefa(novaTarefaMov, instantePrimeiraTarefa, line.prioridade));
                }
                else
                {
                    this.timeLine.Add(new Evento(line, instantePrimeiraTarefa)); //linha terminal está inicialmente livre
                }
            }

            foreach(var line in linhasDeManobra)
            {
                line.vagoesVaziosAtual = line.QtdeVagoesVazios;
                line.vagoesCarregadosAtual = line.QtdeVagoesCarregados;
                //line.prioridade = rand.NextDouble();

                if(line.QtdeVagoesCarregados > 0)
                {
                    //cria tarefas de descarga (uma para cada linha de manobra, por enquanto)
                    var novaTarefaMov = new Transporte(line, line.QtdeVagoesCarregados, false);
                    listaDeTarefas.Add(new Tarefa(novaTarefaMov, instantePrimeiraTarefa, line.prioridade));
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

            var vagoesLmLivres = new List<Evento>(); //TODO: trocar por lista da classe vagao LM (isso quando lista de tarefas e timeline já forem listas de objetos)
            var linhasTerminaisLivres = new List<Evento>(); //TODO: trocar por lista da classe linha (isso quando lista de tarefas e timeline já forem listas de objetos)

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
                        if (job.carregamento != null)
                        {
                            #region faz o transporte e carregamento
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

                            this.FazCarregamento(vagaoDesignado, job, linhasDeManobra, ultimoInstanteTratado, result);

                            //carregamento foi encaminhado, então linha e vagao não estão mais livres
                            linhasTerminaisLivres.RemoveAll(x => x.linhaTerminal.LinhaID == job.carregamento.LinhaID); //apesar de usar All, só deve apagar um
                            vagoesLmLivres.RemoveAt(0);

                            #endregion
                        }
                        else if (job.transporte != null && ultimoInstanteTratado >= job.instante)
                        {
                            if(String.IsNullOrEmpty(job.transporte.linhaOrigem.NomeTerminal)) //se a linha origem é de manobra
                            {
                                if(job.transporte.Vazios) 
                                {
                                    //se os vagoes estão vazios, é um carregamento, este caso é tratado no carregamento
                                }
                                else //caso contrário é uma descarga
                                {
                                    #region faz o transporte e encaminha o carregamento
                                    if (linhasTerminaisLivres.Any(x => x.linhaTerminal.Capacidade >= job.transporte.qtdeVagoes) == false)
                                    {
                                        continue; //se o terminal está ocupado
                                    }

                                    if (vagoesLmLivres.Any() == false)
                                        continue;

                                    var vagaoLivre = vagoesLmLivres[0];

                                    this.FazDescarga(vagaoLivre, job, linhasTerminaisLivres, ultimoInstanteTratado, result);

                                    //carregamento foi encaminhado, então linha e vagao não estão mais livres
                                    vagoesLmLivres.RemoveAt(0); 
                                    #endregion
                                }
                            }
                            else
                            {
                                #region faz apenas o transporte
                                if (vagoesLmLivres.Any() == false)
                                    continue;

                                var vagaoDesignado = vagoesLmLivres[0];

                                vagoesLmLivres.RemoveAt(0); //movimento foi encaminhado, então ocupo o vagao

                                vagaoDesignado.instante = ultimoInstanteTratado.AddMinutes(this.tempoMovEntreLinhas);

                                this.timeLine.Add(vagaoDesignado);

                                var linhaDestino = linhasDeManobra.FirstOrDefault(x => x.vagoesVaziosAtual + x.vagoesCarregadosAtual + job.transporte.qtdeVagoes <= x.Capacidade);

                                if (linhaDestino == null)
                                    continue;

                                var acao = "";

                                if (job.transporte.Vazios == false)
                                {
                                    job.transporte.linhaOrigem.vagoesCarregadosAtual -= job.transporte.qtdeVagoes;
                                    acao = String.Format("Levar {0} vagões carregados do terminal {1} para a linha {2} utilizando vagão LM #{3}",
                                        job.transporte.qtdeVagoes,
                                        job.transporte.linhaOrigem.Nome,
                                        linhaDestino.Nome,
                                        vagaoDesignado.vagaoLM.Idx);
                                }
                                else
                                {
                                    job.transporte.linhaOrigem.vagoesVaziosAtual -= job.transporte.qtdeVagoes;
                                    acao = String.Format("Levar {0} vagões vazios do terminal {1} para a linha {2} utilizando vagão LM #{3}",
                                        job.transporte.qtdeVagoes,
                                        job.transporte.linhaOrigem.Nome,
                                        linhaDestino.Nome,
                                        vagaoDesignado.vagaoLM.Idx);
                                }

                                if (job.transporte.linhaOrigem.vagoesCarregadosAtual + job.transporte.linhaOrigem.vagoesVaziosAtual == 0)
                                {
                                    this.timeLine.Add(new Evento(job.transporte.linhaOrigem, vagaoDesignado.instante)); //linha está realmente livre assim que o vagao encerrar a movimentacao
                                }

                                result.rows.Add(new ResultadoOtimizaDataRow(ultimoInstanteTratado, acao, 1)); 
                                #endregion
                            }

                            job.concluida = 1;
                        }
                        else if (job.chegada != null && ultimoInstanteTratado >= job.chegada.HorarioChegada)
                        {
                            //TODO: tratar a chegada - uma chegada implica em: 
                            //1 decidir em quais linhas de manobra alocar os vagoes
                            //2 mais vagoes carregados que precisam ser descarregados
                            //3 contabilizar a qtde vagoes vazios

                            var qtdeVagoesAbs = Math.Abs(job.QtdeVagoesConsiderada);

                            var linhaDeManobraDesignada = linhasDeManobra.FirstOrDefault(x => 
                                x.vagoesCarregadosAtual + x.vagoesVaziosAtual + qtdeVagoesAbs <= x.Capacidade);

                            if (linhaDeManobraDesignada != null)
                            {
                                if (job.QtdeVagoesConsiderada > 0)
                                {
                                    result.rows.Add(new ResultadoOtimizaDataRow(ultimoInstanteTratado,
                                        String.Format("Chegada {0}. Levar {1} vagões carregados para a linha {2}",
                                        job.chegada.prefixo,
                                        qtdeVagoesAbs,
                                        linhaDeManobraDesignada.Nome), 1));

                                    linhaDeManobraDesignada.vagoesCarregadosAtual += qtdeVagoesAbs;
                                    //job.chegada.QtdeVagoesCarregados = 0;

                                    //cria tarefa de transporte dos vagoes carregados que acabaram de chegar
                                    listaDeTarefas.Add(new Tarefa(new Transporte(linhaDeManobraDesignada, job.QtdeVagoesConsiderada, false), ultimoInstanteTratado, linhaDeManobraDesignada.prioridade)); 
                                    //timeLine.Add(new Evento(ultimoInstanteTratado));
                                }

                                if (job.QtdeVagoesConsiderada < 0)
                                {
                                    result.rows.Add(new ResultadoOtimizaDataRow(ultimoInstanteTratado,
                                        String.Format("Chegada {0}. Levar {1} vagões vazios para a linha {2}",
                                        job.chegada.prefixo,
                                        qtdeVagoesAbs,
                                        linhaDeManobraDesignada.Nome), 1));

                                    linhaDeManobraDesignada.vagoesVaziosAtual += qtdeVagoesAbs;
                                    //job.chegada.QtdeVagoesVazio = 0;
                                }

                                job.concluida = 1;
                            }
                            
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
            
            result.FO = result.rows.Sum(x => x.qtdeManobras);
            result.rows.Add(new ResultadoOtimizaDataRow(DateTime.Now, "Total de manobras", result.FO));

            return result;
        }

        private void FazCarregamento(Evento eventoVagao, Tarefa job, List<Linha> linhasDeManobra, DateTime ultimoInstante, ResultadoOtimizaData result)
        {
            var linhasDeManobraComVagoesVazios = linhasDeManobra.Where(x => x.vagoesVaziosAtual > 0).ToList(); //para o carregamento, só interessam as linhas que possuam vagoes vazios

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
                var instanteTerminoCarregamento = ultimoInstante.AddMinutes(tempoMovEntreLinhas + (60 * tempoCarregamento));
                //this.timeLine.Add(new Evento(line, job.carregamento.Linha, instanteTerminoCarregamento, qtdeDaLinha)); //termino do carregamento dos n vagoes

                //cria uma tarefa de solicitacao de movimento da linha terminal para a linha de manobra (tarefa de volta dos vagoes)
                var novaTarefaMov = new Transporte(job.carregamento.Linha, qtdeDaLinha, false); 
                listaDeTarefas.Add(new Tarefa(novaTarefaMov, instanteTerminoCarregamento, job.prioridade));

                acao = String.Format(
                    "Levar {0} vagões da linha {1} para carregamento no terminal {2} utilizando vagão LM #{3}",
                    qtdeDaLinha, 
                    line.Nome, 
                    job.carregamento.Linha.Nome, 
                    eventoVagao.vagaoLM.Idx
                    );

                line.vagoesVaziosAtual -= qtdeDaLinha;
                result.rows.Add(new ResultadoOtimizaDataRow(ultimoInstante, acao, 1));
                if (qtdeVagoesAtribuidasAcumulada == job.carregamento.QtdeVagoes)
                    break;
            }

            //TODO: está errado, pois logo após terminar o carregamento, o terminal não está livre, ele só estará livre quando os vagoes forem transportados para as linhas de manobra, ou seja, a sua liberação está associada a conclusão das tarefas de movimentos de vagoes
            #region provavelmente apagar 
            var tempoCarregamentoTotal = (double)job.carregamento.QtdeVagoes / cargaDescarga; //os tempos de movimentacoes dos vagoes ainda não sao considerados
            this.timeLine.Add(new Evento(job.carregamento.Linha, ultimoInstante.AddHours(tempoCarregamentoTotal))); //evento de fim de carregamento na linha terminal 
            #endregion

            eventoVagao.instante = ultimoInstante.AddMinutes(linhasUsadas * tempoMovEntreLinhas);
            this.timeLine.Add(new Evento(eventoVagao.vagaoLM, eventoVagao.instante)); //evento de liberacao da LM

            job.concluida = 1;
        }


        private void FazDescarga(Evento eventoVagao, Tarefa job, List<Evento> linhasTerminaisLivres, DateTime ultimoInstante, ResultadoOtimizaData result)
        {
            //int linhasUsadas = 0;
            //int qtdeVagoesAtribuidasAcumulada = 0;
            int t = 0;

            for (t = 0; t < linhasTerminaisLivres.Count; t++)
            {
                var terminal = linhasTerminaisLivres[t];
                if (terminal.linhaTerminal.Capacidade < job.transporte.qtdeVagoes) 
                    continue; //caso o terminal não tenha capacidade para atender o transporte

                var tempoDescarga = (double)job.transporte.qtdeVagoes / this.cargaDescarga;
                var instanteTerminoDescarga = ultimoInstante.AddMinutes(tempoMovEntreLinhas + (60 * tempoDescarga));
                //var instanteLiberacaoDoTerminal = instanteTerminoDescarga.AddMinutes(tempoMovEntreLinhas);
                //this.timeLine.Add(new Evento(job.descarga.linhaOrigem, terminal.linhaTerminal, instanteTerminoDescarga, -1 * job.descarga.linhaOrigem.QtdeVagoesCarregados)); //termino da descarga dos n vagoes
                //this.timeLine.Add(new Evento(terminal.linhaTerminal, instanteLiberacaoDoTerminal)); //evento de liberacao da linha terminal

                var novaTarefaMov = new Transporte(terminal.linhaTerminal, job.transporte.qtdeVagoes, true); //cria uma tarefa de solicitacao de movimento da linha terminal para a linha de manobra
                listaDeTarefas.Add(new Tarefa(novaTarefaMov, instanteTerminoDescarga , job.prioridade));

                string acao = String.Format(
                    "Levar {0} vagões da linha {1} para descarga no terminal {2} utilizando vagão LM #{3}",
                    job.transporte.qtdeVagoes, 
                    job.transporte.linhaOrigem.Nome, 
                    terminal.linhaTerminal.Nome, 
                    eventoVagao.vagaoLM.Idx
                    );

                result.rows.Add(new ResultadoOtimizaDataRow(ultimoInstante, acao, 1));

                break;
            }

            linhasTerminaisLivres.RemoveAt(t);

            eventoVagao.instante = ultimoInstante.AddMinutes(tempoMovEntreLinhas);
            this.timeLine.Add(new Evento(eventoVagao.vagaoLM, eventoVagao.instante)); //evento de liberacao da LM

            job.concluida = 1;
        }

    }

    
}