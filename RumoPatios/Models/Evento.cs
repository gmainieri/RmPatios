using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RumoPatios.Models
{
    /// <summary>
    /// não é classe do banco
    /// </summary>
    public class Evento
    {
        #region construtores
        //public Evento(Carregamento _carregamento, Random rnd)
        //{
        //    this.carregamento = _carregamento;
        //    this.instante = carregamento.HorarioCarregamento;
        //    this.prioridade = rnd.NextDouble();
        //}

        //public Evento(Chegada _chegada, Random rnd)
        //{
        //    this.chegada = _chegada;
        //    this.instante = _chegada.HorarioChegada;
        //    this.prioridade = rnd.NextDouble();
        //}

        public Evento()
        {
        }

        public Evento(Random rand) 
        {
            this.instante = DateTime.Now.AddMinutes(rand.Next(5000));
        }

        public Evento(DateTime horario)
        {
            this.instante = horario;
        }

        public Evento(VagaoLM _vagao, DateTime inst)
        {
            this.vagaoLM = _vagao;
            this.instante = inst;
            //this.prioridade = -1.0; //recursos tem prioridade frente a eventos
        }

        public Evento(Linha _linha, DateTime inst)
        {
            this.linhaTerminal = _linha;
            this.instante = inst;
            //this.prioridade = -1.0;  //recursos tem prioridade frente a eventos
            //this.qtdeVagoesLiberados = vagoesVazioLiberados;
        }

        //public Evento(Linha manobra, Linha terminal, DateTime inst, int vagoesCarregadosLiberados)
        //{
        //    this.linhaDeManobra = manobra;
        //    this.linhaTerminal = terminal;
        //    this.instante = inst;
        //    this.qtdeVagoesLiberados = vagoesCarregadosLiberados;
        //} 
        #endregion

        #region propriedades
        public DateTime instante { get; set; }

        //// prioridade do evento (-1 para recursos (vagao ou linha), [0,1] para eventos do banco)
        //internal double prioridade { get; set; }

        /// <summary>
        /// aplica-se para liberacao de linhas terminais (positivo: qtde vagoes carregados, negativo: quantidade de vagoes vazios)
        /// </summary>
        //internal int qtdeVagoesLiberados { get; set; }

        #region se for uma liberação, o evento está associado a um recurso (vagao LM ou linha terminal)
        /// <summary>
        /// se esta propriedade não for nula, trata-se da liberacao de um vagao LM
        /// </summary>
        public VagaoLM vagaoLM { get; set; }
        
        /// <summary>
        /// se esta propriedade não for nula (&& qtdeVagoesLiberados == 0), trata-se da liberacao de um terminal
        /// </summary>
        /// <remarks>
        /// linha terminal pode vir acompanhada de linha de manobra e qtdeVagoesLiberados.
        /// </remarks>
        public Linha linhaTerminal { get; set; }
        
        /// <summary>
        /// se esta propriedade não for nula, então (qtdeVagoesLiberados != 0). Significado: carga/descarga foi concluida de N vagoes na linha terminal que devem ser realocados a linha de manobra da onde eles vieram.
        /// </summary>
        //internal Linha linhaDeManobra { get; set; }
        #endregion
        
        //#region caso contrario esta ligado a algum dos eventos do banco
        //internal Chegada chegada { get; set; }
        //internal Carregamento carregamento { get; set; }
        //internal Partida partida { get; set; }
        //#endregion 

        //TODO: implementar este string só para dar uma organizada (não será usado, mas fica mais facil de entender o codigo)
        //internal string Nome { get; set; }

        /// <summary>
        /// 1 - vagão LM liberado, 2 - linha de carregamento liberada, 3 - chegada de trem, 4 - carregamento, ainda não considerados: linha de partida liberada?, partida?
        /// </summary>
        //internal int tipo { get; set; }
        #endregion
    }
}