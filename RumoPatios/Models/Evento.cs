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
        public Evento(Carregamento _carregamento, Random rnd)
        {
            this.carregamento = _carregamento;
            this.instante = carregamento.HorarioCarregamento;
            this.prioridade = rnd.NextDouble();
            //this.tipo = 4;
        }

        public Evento(Chegada _chegada, Random rnd)
        {
            this.chegada = _chegada;
            this.instante = _chegada.HorarioChegada;
            this.prioridade = rnd.NextDouble();
        }

        public Evento(VagaoLM _vagao, DateTime inst)
        {
            this.vagaoLM = _vagao;
            this.instante = inst;
            this.prioridade = -1.0; //recursos tem prioridade frente a eventos
        }

        public Evento(Linha _linha, DateTime inst, int vagoesVazioLiberados)
        {
            this.linhaTerminal = _linha;
            this.instante = inst;
            this.prioridade = -1.0;  //recursos tem prioridade frente a eventos
            this.qtdeVagoescCarregadosLiberados = vagoesVazioLiberados;
        } 
        #endregion

        #region propriedades
        internal DateTime instante { get; set; }

        /// <summary>
        /// prioridade do evento (-1 para recursos (vagao ou linha), [0,1] para eventos do banco)
        /// </summary>
        internal double prioridade { get; set; }

        /// <summary>
        /// aplica-se para liberacao de linhas terminais
        /// </summary>
        internal int qtdeVagoescCarregadosLiberados { get; set; }

        #region se for uma liberação, o evento está associado a um recurso (vagao LM ou linha terminal)
        internal VagaoLM vagaoLM { get; set; }
        internal Linha linhaTerminal { get; set; }
        #endregion
        
        #region caso contrario esta ligado a algum dos eventos do banco
        internal Chegada chegada { get; set; }
        internal Carregamento carregamento { get; set; }
        internal Partida partida { get; set; }
        #endregion 

        //internal int Idx { get; set; }

        /// <summary>
        /// 1 - vagão LM liberado, 2 - linha de carregamento liberada, 3 - chegada de trem, 4 - carregamento, 
        /// ainda não considerados: linha de partida liberada?, partida?
        /// </summary>
        //internal int tipo { get; set; }
        #endregion
    }
}