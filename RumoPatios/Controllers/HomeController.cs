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

        /// <summary>
        /// lista de tarefas que atualmente são apenas transportes (se for necessário outro tipo de tarefa, criar uma classe para esta nova tarefa e fazer esta lista uma lista de objetos)
        /// </summary>
        List<Transporte> listaDeTarefas { get; set; }

        public ActionResult Index()
        {
            //cria o view model com todas as tabelas

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
                                arrival.randLoaded = pai2.Chegadas[arrival.ChegadaID - 1].randLoaded;
                                arrival.randEmpty = pai2.Chegadas[arrival.ChegadaID - 1].randEmpty;
                            }
                            else
                            {
                                arrival.randLoaded = pai1.Chegadas[0].randLoaded;
                                arrival.randEmpty = pai1.Chegadas[0].randEmpty;
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
                            {
                                load.aleatorio = pai2.Carregamentos[load.CarregamentoID - 1].aleatorio;
                                load.prioridade = pai2.Carregamentos[load.CarregamentoID - 1].prioridade;
                            }
                            else
                            {
                                load.aleatorio = pai1.Carregamentos[load.CarregamentoID - 1].aleatorio;
                                load.prioridade = pai1.Carregamentos[load.CarregamentoID - 1].prioridade;
                            }

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
            catch (Exception ex)
            {

            }

            return View();
        }

        internal void geraMutante(ResultadoOtimizaData result)
        {
            foreach (var arrival in result.Chegadas)
            {
                arrival.randLoaded = 0.10 + (0.50 * this.rand.NextDouble()); //blocos dos vagoes carregados, mínimo 10%, máximo 60%
                arrival.randEmpty = 0.25 + (0.75 * this.rand.NextDouble()); //blocos dos vagoes vazios, mínimo 25%, máximo 75%
            }

            foreach (var line in result.Linhas)
            {
                line.prioridade = this.rand.NextDouble();
                //if (String.IsNullOrEmpty(line.NomeTerminal))
                //{
                //    line.vagoesCarregadosAtual = line.QtdeVagoesCarregados;
                //    line.vagoesVaziosAtual = line.QtdeVagoesVazios;
                //}

            }

            foreach(var load in result.Carregamentos)
            {
                load.aleatorio = 0.50 + (0.50 * this.rand.NextDouble()); //fracionamento dos carregamentos, mínimo 50%, máximo 100%
                load.prioridade = this.rand.NextDouble();
            }
            //result.Carregamentos.ForEach(x => x.prioridade = this.rand.NextDouble());
        }

        internal ResultadoOtimizaData Decodificador(ResultadoOtimizaData result)
        {
            //this.rand = new Random();
            //this.db = new ApplicationDbContext();
            //this.result = new ResultadoOtimizaData(this.db);
            this.timeLine = new List<Evento>(100);
            this.listaDeTarefas = new List<Transporte>(100);

            ////TODO: implementar lista generica como abaixo
            ////c# order by using reflection
            //var timeLineTest = new List<object>();
            //timeLineTest.Add(new Evento(this.rand));
            //timeLineTest.Add(new Evento(this.rand));
            //timeLineTest = timeLineTest.OrderBy(x => x.GetType().GetProperty("instante").GetValue(x)).ToList();
            //timeLineTest = timeLineTest.OrderByDescending(x => x.GetType().GetProperty("instante").GetValue(x)).ToList();
            //var EventoDummy = new Evento();
            //timeLineTest = timeLineTest.Where(x => x.GetType().Equals(EventoDummy)).ToList();

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

                while (qtdeTotal < load.QtdeVagoes)
                {
                    qtdeRestante = load.QtdeVagoes - qtdeTotal;
                    qtdeAtual = Math.Min(qtdeRestante, (int)Math.Floor(load.aleatorio * load.QtdeVagoes));
                    qtdeTotal += qtdeAtual;

                    //listaDeTarefas.Add(new Tarefa(load, qtdeAtual));

                    listaDeTarefas.Add(new Transporte(null, load.Linha, qtdeAtual, false, this.instanteInicial, load.prioridade));
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

                #region as chegadas são divididas em blocos de Tarefas, para assim facilitar a designação dos vagoes às linhas de manobra
                //os vagoes carregados chegam na frente do comboio, por isso prioridade 0.0 e dos vazios 1.0
                while (loadTotal < arrival.QtdeVagoesCarregados)
                {
                    qtdeRestante = arrival.QtdeVagoesCarregados - loadTotal;
                    qtdeAtual = Math.Min(qtdeRestante, (int)Math.Floor(arrival.randLoaded * arrival.QtdeVagoesCarregados));
                    loadTotal += qtdeAtual;

                    timeLine.Add(new Evento(arrival, qtdeAtual, false));
                    //listaDeTarefas.Add(new Tarefa(arrival, qtdeAtual, 0.0));
                }

                while (emptyTotal < arrival.QtdeVagoesVazio)
                {
                    qtdeRestante = arrival.QtdeVagoesVazio - emptyTotal;
                    qtdeAtual = Math.Min(qtdeRestante, (int)Math.Floor(arrival.randEmpty * arrival.QtdeVagoesVazio));
                    emptyTotal += qtdeAtual;

                    timeLine.Add(new Evento(arrival, qtdeAtual, true));
                    //listaDeTarefas.Add(new Tarefa(arrival, -1 * qtdeAtual, 1.0));
                }
                #endregion

                //timeLine.Add(new Evento(arrival.HorarioChegada)); //adiciono um evento vazio, apenas pra dar um tick no relogio
            }


            #endregion

            var instantePrimeiraTarefa = listaDeTarefas.Min(x => x.instante);

            var vagoesLM = new List<VagaoLM>(); //como vagao nao é uma classe do banco, tenho que criar
            //var vagoesLmLivres = new List<Evento>();
            //var linhasCarregamentoLivres = new List<Evento>();

            for (int i = 0; i < maxMovParalelo; i++)
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
                    //var novaTarefaMov = new Transporte(line, null, line.QtdeVagoesCarregados, false); //cria uma tarefa de solicitacao de transporte da linha terminal
                    listaDeTarefas.Add(new Transporte(line, null, line.QtdeVagoesCarregados, false, instantePrimeiraTarefa, line.prioridade));
                }
                else if (line.QtdeVagoesVazios > 0)
                {
                    //var novaTarefaMov = new Transporte(line, null, line.QtdeVagoesVazios, true); 
                    //listaDeTarefas.Add(new Tarefa(novaTarefaMov, instantePrimeiraTarefa, line.prioridade));
                    listaDeTarefas.Add(new Transporte(line, null, line.QtdeVagoesVazios, true, instantePrimeiraTarefa, line.prioridade)); //cria uma tarefa de solicitacao de transporte da linha terminal
                }
                else
                {
                    this.timeLine.Add(new Evento(line, instantePrimeiraTarefa)); //linha terminal está inicialmente livre
                }
            }

            foreach (var line in linhasDeManobra)
            {
                line.vagoesVaziosAtual = line.QtdeVagoesVazios;
                line.vagoesCarregadosAtual = line.QtdeVagoesCarregados;
                //line.prioridade = rand.NextDouble();

                if (line.QtdeVagoesCarregados > 0)
                {
                    //preciso encaminhar as descarga destes vagoes carregados que estão inicialmente no patio
                    //TODO: fracionar estas descargas (ainda não precisa, pq inicialmente não existem muitos vagoes no patio)
                    //cria tarefas de descarga (uma para cada linha de manobra, por enquanto)
                    //provavelmente eles vao dizer que estas descargas precisam ser fracionadas
                    //perguntar mais ou menos quais são os tamanhos dos blocos deste tipo de fracionamento e implementar
                    listaDeTarefas.Add(new Transporte(line, null, line.QtdeVagoesCarregados, false, instantePrimeiraTarefa, line.prioridade));

                    //var novaTarefaMov = new Transporte(line, null, line.QtdeVagoesCarregados, false);
                    //listaDeTarefas.Add(new Tarefa(novaTarefaMov, instantePrimeiraTarefa, line.prioridade));
                    
                }

                //if(line.QtdeVagoesVazios > 0)
                //{
                //    //não preciso fazer nada pq o vagoes estão vazios
                //}

            }

            //listaDeTarefas = listaDeTarefas.OrderBy(x => x.instante)
            //    .ThenBy(x => x.prioridade)
            //    .ToList();

            listaDeTarefas.Sort((x, y) => x.prioridade.CompareTo(y.prioridade));

            //this.timeLine.Sort((x, y) => x.instante.CompareTo(y.instante));
            //vagoesLM.Sort((x, y) => x.instanteDeLiberacao.CompareTo(y.instanteDeLiberacao));
            //linhasDeCarregamento.Sort((x, y) => x.instanteDeLiberacao.CompareTo(y.instanteDeLiberacao));

            #region algoritmo por ultimo instante tratado

            linhasTerminais.Sort((x, y) => x.prioridade.CompareTo(y.prioridade));
            linhasDeManobra.Sort((x, y) => x.prioridade.CompareTo(y.prioridade));

            var vagoesLmLivres = new List<Evento>(); //TODO: trocar por lista da classe vagao LM (isso quando lista de tarefas e timeline já forem listas de objetos)
            var linhasTerminaisLivres = new List<Evento>(); //TODO: trocar por lista da classe linha (isso quando lista de tarefas e timeline já forem listas de objetos)

            while (this.timeLine.Any())
            {
                this.timeLine = this.timeLine.OrderBy(x => x.instante)
                    //.ThenBy(x => x.prioridade)
                    .ThenBy(x => x.vagaoLM == null ? 9999 : x.vagaoLM.Idx)
                    .ThenBy(x => x.linhaTerminal == null ? 9999 : x.linhaTerminal.LinhaID)
                    .ToList();

                //this.timeLine.Sort((x, y) => x.instante.CompareTo(y.instante));

                var timeLineAgrupada = this.timeLine.GroupBy(x => x.instante).ToList();
                var ultimoInstanteTratado = timeLineAgrupada.First().First().instante;
                var eventosDoInstante = timeLineAgrupada.First().Count();

                foreach (var evento in timeLineAgrupada.First())
                {
                    #region contabilizar o evento mais atual
                    //var evento = this.timeLine[0];

                    #region libera vagao LM
                    if (evento.vagaoLM != null)
                    {
                        result.rows.Add(new ResultadoOtimizaDataRow(evento.instante, String.Format("Vagão LM #{0} liberado", evento.vagaoLM.Idx), 0));
                        vagoesLmLivres.Add(evento);
                    }
                    #endregion
                    #region libera linha terminal
                    else if (evento.linhaTerminal != null)
                    {
                        //if(evento.qtdeVagoesLiberados > 0)
                        //{
                        //    //evento.linhaTerminal.QtdeVagoesCarregados += evento.qtdeVagoesLiberados;
                        //    var mensagem = String.Format("Terminal {0}: fim de carga [{1} vagões)", evento.linhaTerminal.Nome, evento.qtdeVagoesLiberados);
                        //    result.rows.Add(new ResultadoOtimizaDataRow(evento.instante, mensagem , 1));
                        //}
                        //else if(evento.qtdeVagoesLiberados < 0)
                        //{
                        //    //evento.linhaTerminal.QtdeVagoesVazios += -1 * evento.qtdeVagoesLiberados;
                        //    var mensagem = String.Format("Terminal {0}: fim de descarga [{1} vagões)", evento.linhaTerminal.Nome, -1 * evento.qtdeVagoesLiberados);
                        //    result.rows.Add(new ResultadoOtimizaDataRow(evento.instante, mensagem, 1));
                        //}
                        //else
                        //{
                        //    result.rows.Add(new ResultadoOtimizaDataRow(evento.instante, String.Format("Terminal {0} disponível", evento.linhaTerminal.Nome), 0));
                        //    linhasTerminaisLivres.Add(evento);
                        //}

                        result.rows.Add(new ResultadoOtimizaDataRow(evento.instante,
                            String.Format("Terminal {0} disponível [{1};{2}]",
                                evento.linhaTerminal.Nome,
                                evento.linhaTerminal.vagoesCarregadosAtual,
                                evento.linhaTerminal.vagoesVaziosAtual), 0));

                        linhasTerminaisLivres.Add(evento);

                    }
                    #endregion
                    #region contabiliza chegada
                    else if (evento.chegada != null)
                    {
                        //tratar a chegada - uma chegada implica em: 
                        //1 decidir em qual linha de manobra alocar os vagoes (contabilizando as novas qtdes vagoes atuais)
                        //2 no caso de vagoes carregados, mais descargas que precisam ser providenciadas

                        //em uma chegada, os vagoes são contabilizado instantaneamente à primeira linha que tiver capacidade
                        var linhaDeManobraDesignada = linhasDeManobra.FirstOrDefault(x =>
                            x.vagoesCarregadosAtual + x.vagoesVaziosAtual + evento.QtdeVagoes <= x.Capacidade);

                        if (evento.VagoesVazios == false)
                        {
                            linhaDeManobraDesignada.vagoesCarregadosAtual += evento.QtdeVagoes;

                            result.rows.Add(new ResultadoOtimizaDataRow(ultimoInstanteTratado,
                                String.Format("Chegada {0}. Levar {1} vagões carregados para a linha {2} [{3};{4}]",
                                evento.chegada.prefixo,
                                evento.QtdeVagoes,
                                linhaDeManobraDesignada.Nome,
                                linhaDeManobraDesignada.vagoesCarregadosAtual,
                                linhaDeManobraDesignada.vagoesVaziosAtual), 1));

                            //cria tarefa de transporte dos vagoes carregados que acabaram de chegar
                            listaDeTarefas.Add(new Transporte(linhaDeManobraDesignada, null, evento.QtdeVagoes, false, evento.instante, linhaDeManobraDesignada.prioridade));
                        }
                        else
                        {
                            linhaDeManobraDesignada.vagoesVaziosAtual += evento.QtdeVagoes;

                            result.rows.Add(new ResultadoOtimizaDataRow(ultimoInstanteTratado,
                                String.Format("Chegada {0}. Levar {1} vagões vazios para a linha {2} [{3};{4}]",
                                evento.chegada.prefixo,
                                evento.QtdeVagoes,
                                linhaDeManobraDesignada.Nome,
                                linhaDeManobraDesignada.vagoesCarregadosAtual,
                                linhaDeManobraDesignada.vagoesVaziosAtual), 1));
                        }

                    }
                    #endregion
                    #region contabiliza carregamento
                    //else if (evento.carregamento != null)
                    //{
                    //    eventosPendentes.Add(evento);
                    //} 
                    #endregion
                    #region contabiliza partida
                    //else if (evento.partida != null)
                    //{
                    //    //partidas ainda não são consideradas
                    //} 
                    #endregion

                    //var ultimoInstanteTratado = evento.instante;
                    //this.timeLine.RemoveAt(0); //removo o evento que foi tratado
                    //this.timeLine.Remove(evento);
                    #endregion
                }

                this.timeLine.RemoveRange(0, eventosDoInstante);

                //if(this.timeLine.Any() == false || this.timeLine[0].instante > ultimoInstanteTratado) {

                var tarefasDisponiveis = listaDeTarefas.Where(job => job.instante <= ultimoInstanteTratado)
                    .OrderBy(job => job.instante)
                    .ThenBy(job => job.prioridade)
                    //add terceira prioridade?
                    .ToList();

                #region resolver as tarefas disponiveis da fila
                foreach (var job in tarefasDisponiveis)
                {
                    if (job.linhaDestino != null)
                    {
                        //trata-se de um carregamento programado, pois conheço o terminal, mas não sei de onde devem vir os vagoes
                        #region faz o transporte de vagoes vazios para o destino encaminha o carregamento
                        if (linhasTerminaisLivres.Any(x => x.linhaTerminal.LinhaID == job.linhaDestino.LinhaID) == false)
                        {
                            continue; //se o terminal está ocupado
                        }

                        if (vagoesLmLivres.Any() == false) //(por enquanto estamos utilizando apenas uma LM pra fazer todos os transportes de vagoes necessários para um carregamento)
                            continue;

                        var vagaoDesignado = vagoesLmLivres[0];

                        var qtdeAtualVagoesVaziosNoPatio = linhasDeManobra.Sum(x => x.vagoesVaziosAtual);

                        if (qtdeAtualVagoesVaziosNoPatio < job.qtdeVagoes)
                        {
                            continue; //não trato o eventoAtual - não deve acontecer
                        }

                        this.FazCarregamento(vagaoDesignado, job, linhasDeManobra, ultimoInstanteTratado, result);

                        //carregamento foi encaminhado, então linha e vagao não estão mais livres
                        linhasTerminaisLivres.RemoveAll(x => x.linhaTerminal.LinhaID == job.linhaDestino.LinhaID); //apesar de usar All, só deve apagar um
                        vagoesLmLivres.RemoveAt(0);

                        #endregion
                    }
                    else
                    {
                        #region tem-se apenas a linha de origem e precisa decidir quais os destinos
                        if (String.IsNullOrEmpty(job.linhaOrigem.NomeTerminal)) //se a linha origem é de manobra
                        {
                            if (job.Vazios)
                            {
                                //esta tarefa nunca é criada, pois carregamentos são solicitados pela linha de destino (terminal)
                            }
                            else //caso contrário é uma descarga
                            {
                                #region faz o transporte e encaminha o carregamento
                                if (linhasTerminaisLivres.Any(x => x.linhaTerminal.Capacidade >= job.qtdeVagoes) == false)
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
                            #region faz apenas o transporte, pois quando a linha de origem é temrinal, só tenho que voltar os vagoes para o patio e contabilizar o fluxo no terminal e no patio
                            if (vagoesLmLivres.Any() == false)
                                continue;

                            var vagaoDesignado = vagoesLmLivres[0];

                            vagoesLmLivres.RemoveAt(0); //movimento foi encaminhado, então ocupo o vagao

                            vagaoDesignado.instante = ultimoInstanteTratado.AddMinutes(this.tempoMovEntreLinhas);

                            this.timeLine.Add(vagaoDesignado);

                            var linhaDestino = linhasDeManobra.FirstOrDefault(x => x.vagoesVaziosAtual + x.vagoesCarregadosAtual + job.qtdeVagoes <= x.Capacidade);

                            if (linhaDestino == null)
                                continue;

                            var acao = "";

                            if (job.Vazios == false)
                            {
                                job.linhaOrigem.vagoesCarregadosAtual -= job.qtdeVagoes;
                                linhaDestino.vagoesCarregadosAtual += job.qtdeVagoes;
                                //acao = String.Format("Levar {0} vagões carregados do terminal {1} para a linha {2} utilizando vagão LM #{3}",
                                //    job.qtdeVagoes,
                                //    job.linhaOrigem.NomeTerminal,
                                //    linhaDestino.Nome,
                                //    vagaoDesignado.vagaoLM.Idx);

                                acao = String.Format("Levar {0} vagões carregados do terminal {1} [{2};{3}] para a linha {4} [{5};{6}] utilizando vagão LM #{7}",
                                    job.qtdeVagoes,
                                    job.linhaOrigem.NomeTerminal,
                                    job.linhaOrigem.vagoesCarregadosAtual,
                                    job.linhaOrigem.vagoesVaziosAtual,
                                    linhaDestino.Nome,
                                    linhaDestino.vagoesCarregadosAtual,
                                    linhaDestino.vagoesVaziosAtual,
                                    vagaoDesignado.vagaoLM.Idx);

                            }
                            else
                            {
                                job.linhaOrigem.vagoesVaziosAtual -= job.qtdeVagoes;
                                linhaDestino.vagoesVaziosAtual += job.qtdeVagoes;
                                acao = String.Format("Levar {0} vagões vazios do terminal {1} [{2};{3}] para a linha {4} [{5};{6}] utilizando vagão LM #{7}",
                                    job.qtdeVagoes,
                                    job.linhaOrigem.NomeTerminal,
                                    job.linhaOrigem.vagoesCarregadosAtual,
                                    job.linhaOrigem.vagoesVaziosAtual,
                                    linhaDestino.Nome,
                                    linhaDestino.vagoesCarregadosAtual,
                                    linhaDestino.vagoesVaziosAtual,
                                    vagaoDesignado.vagaoLM.Idx);
                            }

                            var totalDaLinha = job.linhaOrigem.vagoesCarregadosAtual + job.linhaOrigem.vagoesVaziosAtual;
                            if (totalDaLinha == 0)
                            {
                                this.timeLine.Add(new Evento(job.linhaOrigem, vagaoDesignado.instante)); //linha está realmente livre assim que o vagao encerrar o transporte 
                            }

                            result.rows.Add(new ResultadoOtimizaDataRow(ultimoInstanteTratado, acao, 1));
                            #endregion
                        }
                        #endregion
                    }

                    job.concluido = 1;

                    
                    #region antigos jobs
                    //if (job.transporte != null)
                    //{

                    //}
                    //#region trata tarefa de chegada
                    //else if (job.chegada != null)
                    //{
                    //    //tratar a chegada - uma chegada implica em: 
                    //    //1 decidir em qual linha de manobra alocar os vagoes (contabilizando as novas qtdes vagoes atuais)
                    //    //2 no caso de vagoes carregados, mais descargas que precisam ser providenciadas

                    //    var qtdeVagoesAbs = Math.Abs(job.QtdeVagoesConsiderada);

                    //    var linhaDeManobraDesignada = linhasDeManobra.FirstOrDefault(x =>
                    //        x.vagoesCarregadosAtual + x.vagoesVaziosAtual + qtdeVagoesAbs <= x.Capacidade);

                    //    if (linhaDeManobraDesignada != null)
                    //    {
                    //        if (job.QtdeVagoesConsiderada > 0)
                    //        {
                    //            linhaDeManobraDesignada.vagoesCarregadosAtual += qtdeVagoesAbs;
                    //            //job.chegada.QtdeVagoesCarregados = 0;

                    //            result.rows.Add(new ResultadoOtimizaDataRow(ultimoInstanteTratado,
                    //                String.Format("Chegada {0}. Levar {1} vagões carregados para a linha {2} [{3};{4}]",
                    //                job.chegada.prefixo,
                    //                qtdeVagoesAbs,
                    //                linhaDeManobraDesignada.Nome,
                    //                linhaDeManobraDesignada.vagoesCarregadosAtual,
                    //                linhaDeManobraDesignada.vagoesVaziosAtual), 1));

                    //            //cria tarefa de transporte dos vagoes carregados que acabaram de chegar
                    //            listaDeTarefas.Add(new Tarefa(new Transporte(linhaDeManobraDesignada, null, job.QtdeVagoesConsiderada, false), ultimoInstanteTratado, linhaDeManobraDesignada.prioridade));
                    //            //timeLine.Add(new Evento(ultimoInstanteTratado));
                    //        }

                    //        if (job.QtdeVagoesConsiderada < 0)
                    //        {
                    //            linhaDeManobraDesignada.vagoesVaziosAtual += qtdeVagoesAbs;
                    //            //job.chegada.QtdeVagoesVazio = 0;

                    //            result.rows.Add(new ResultadoOtimizaDataRow(ultimoInstanteTratado,
                    //                String.Format("Chegada {0}. Levar {1} vagões vazios para a linha {2} [{3};{4}]",
                    //                job.chegada.prefixo,
                    //                qtdeVagoesAbs,
                    //                linhaDeManobraDesignada.Nome,
                    //                linhaDeManobraDesignada.vagoesCarregadosAtual,
                    //                linhaDeManobraDesignada.vagoesVaziosAtual), 1));
                    //        }

                    //        job.concluida = 1;
                    //    }

                    //} 
                    //#endregion
                    //#region trata tarefa de partida
                    //else if (job.partida != null)
                    //{
                    //    job.concluida = 1;
                    //} 
                    //#endregion 
                    #endregion
                }

                listaDeTarefas.RemoveAll(x => x.concluido == 1);

                #endregion

            } //fim de while 
            #endregion

            //result.rows.Sort((x, y) => x.horario.CompareTo(y.horario));

            result.FO = result.rows.Sum(x => x.qtdeManobras);
            result.rows.Add(new ResultadoOtimizaDataRow(DateTime.Now, "Total de manobras", result.FO));

            return result;
        }

        /// <summary>
        /// escolhe quais linhas fornecerao os vagoes para o carregamento no terminal
        /// </summary>
        /// <param name="eventoVagao"></param>
        /// <param name="job"></param>
        /// <param name="linhasDeManobra"></param>
        /// <param name="ultimoInstante"></param>
        /// <param name="result"></param>
        private void FazCarregamento(Evento eventoVagao, Transporte job, List<Linha> linhasDeManobra, DateTime ultimoInstante, ResultadoOtimizaData result)
        {
            var linhasDeManobraComVagoesVazios = linhasDeManobra.Where(x => x.vagoesVaziosAtual > 0).ToList(); //para o carregamento, só interessam as linhas que possuam vagoes vazios

            int linhasUsadas = 0;
            int qtdeVagoesAtribuidasAcumulada = 0;

            foreach (var line in linhasDeManobraComVagoesVazios)
            {
                linhasUsadas++;
                string acao = "";

                var qtdeRestante = job.qtdeVagoes - qtdeVagoesAtribuidasAcumulada;
                var qtdeDaLinha = Math.Min(qtdeRestante, line.vagoesVaziosAtual); //minimo entre quanto falta e quanto tem disponivel na linha
                qtdeVagoesAtribuidasAcumulada += qtdeDaLinha;

                var tempoCarregamento = (double)qtdeDaLinha / cargaDescarga;
                var instanteTerminoCarregamento = ultimoInstante.AddMinutes(tempoMovEntreLinhas + (60 * tempoCarregamento));
                //this.timeLine.Add(new Evento(line, job.carregamento.Linha, instanteTerminoCarregamento, qtdeDaLinha)); //termino do carregamento dos n vagoes

                //cria uma tarefa de solicitacao de movimento da linha terminal para a linha de manobra (tarefa de volta dos vagoes)
                listaDeTarefas.Add(new Transporte(job.linhaDestino, null, qtdeDaLinha, false, instanteTerminoCarregamento, job.prioridade)); //como é da volta, o primeiro argumento é destino e segundo null

                //var novaTarefaMov = new Transporte(job.linhaDestino, null, qtdeDaLinha, false); //como é da volta, o primeiro argumento é destino e segundo null
                //listaDeTarefas.Add(new Tarefa(novaTarefaMov, instanteTerminoCarregamento, job.prioridade));

                line.vagoesVaziosAtual -= qtdeDaLinha;
                job.linhaDestino.vagoesCarregadosAtual += qtdeDaLinha; //já considero que os vagoes estão carregados

                acao = String.Format(
                    "Levar {0} vagões da linha {1} [{2};{3}] para carregamento no terminal {4} [{5};{6}] utilizando vagão LM #{7}",
                    qtdeDaLinha,
                    line.Nome,
                    line.vagoesCarregadosAtual,
                    line.vagoesVaziosAtual,
                    job.linhaDestino.NomeTerminal,
                    job.linhaDestino.vagoesCarregadosAtual,
                    job.linhaDestino.vagoesVaziosAtual,
                    eventoVagao.vagaoLM.Idx
                    );

                result.rows.Add(new ResultadoOtimizaDataRow(ultimoInstante, acao, 1));
                if (qtdeVagoesAtribuidasAcumulada == job.qtdeVagoes)
                    break;
            }

            #region provavelmente apagar
            //var tempoCarregamentoTotal = (double)job.qtdeVagoes / cargaDescarga; //os tempos de movimentacoes dos vagoes ainda não sao considerados
            //this.timeLine.Add(new Evento(job.carregamento.Linha, ultimoInstante.AddHours(tempoCarregamentoTotal))); //evento de fim de carregamento na linha terminal 
            #endregion

            eventoVagao.instante = ultimoInstante.AddMinutes(linhasUsadas * tempoMovEntreLinhas);
            this.timeLine.Add(new Evento(eventoVagao.vagaoLM, eventoVagao.instante)); //evento de liberacao da LM (por enquanto estamos utilizando apenas uma LM pra fazer todos os transportes de vagoes necessários para um carregamento)

            job.concluido = 1;
        }


        private void FazDescarga(Evento eventoVagao, Transporte job, List<Evento> linhasTerminaisLivres, DateTime ultimoInstante, ResultadoOtimizaData result)
        {
            //int linhasUsadas = 0;
            //int qtdeVagoesAtribuidasAcumulada = 0;
            int t = 0;

            for (t = 0; t < linhasTerminaisLivres.Count; t++)
            {
                var terminal = linhasTerminaisLivres[t];
                if (terminal.linhaTerminal.Capacidade < job.qtdeVagoes)
                    continue; //caso o terminal não tenha capacidade para atender o transporte

                var tempoDescarga = (double)job.qtdeVagoes / this.cargaDescarga;
                var instanteTerminoDescarga = ultimoInstante.AddMinutes(tempoMovEntreLinhas + (60 * tempoDescarga));
                //var instanteLiberacaoDoTerminal = instanteTerminoDescarga.AddMinutes(tempoMovEntreLinhas);
                //this.timeLine.Add(new Evento(job.descarga.linhaOrigem, terminal.linhaTerminal, instanteTerminoDescarga, -1 * job.descarga.linhaOrigem.QtdeVagoesCarregados)); //termino da descarga dos n vagoes
                //this.timeLine.Add(new Evento(terminal.linhaTerminal, instanteLiberacaoDoTerminal)); //evento de liberacao da linha terminal

                listaDeTarefas.Add(new Transporte(terminal.linhaTerminal, null, job.qtdeVagoes, true, instanteTerminoDescarga, job.prioridade));

                //var novaTarefaMov = new Transporte(terminal.linhaTerminal, null, job.qtdeVagoes, true); //cria uma tarefa de solicitacao de movimento da linha terminal para a linha de manobra
                //listaDeTarefas.Add(new Tarefa(novaTarefaMov, instanteTerminoDescarga, job.prioridade));

                job.linhaOrigem.vagoesCarregadosAtual -= job.qtdeVagoes;
                terminal.linhaTerminal.vagoesVaziosAtual += job.qtdeVagoes; //já considero os vagoes carregados como vazios

                string acao = String.Format(
                    "Levar {0} vagões da linha {1} [{2};{3}] para descarga no terminal {4} [{5};{6}] utilizando vagão LM #{7}",
                    job.qtdeVagoes,
                    job.linhaOrigem.Nome,
                    job.linhaOrigem.vagoesCarregadosAtual,
                    job.linhaOrigem.vagoesVaziosAtual,
                    terminal.linhaTerminal.NomeTerminal,
                    terminal.linhaTerminal.vagoesCarregadosAtual,
                    terminal.linhaTerminal.vagoesVaziosAtual,
                    eventoVagao.vagaoLM.Idx
                    );

                result.rows.Add(new ResultadoOtimizaDataRow(ultimoInstante, acao, 1));

                break;
            }

            linhasTerminaisLivres.RemoveAt(t);

            eventoVagao.instante = ultimoInstante.AddMinutes(tempoMovEntreLinhas);
            this.timeLine.Add(new Evento(eventoVagao.vagaoLM, eventoVagao.instante)); //evento de liberacao da LM

            job.concluido = 1;
        }

    }


}