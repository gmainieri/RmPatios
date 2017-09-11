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
        public Evento(Carregamento _carregamento)
        {
            this.carregamento = _carregamento;
            this.instante = carregamento.HorarioCarregamento;
            //this.tipo = 4;
        }

        public Evento(Chegada _chegada)
        {
            this.chegada = _chegada;
            this.instante = _chegada.HorarioChegada;
        }

        public Evento(VagaoLM _vagao, DateTime inst)
        {
            this.vagao = _vagao;
            this.instante = inst;
        }

        public Evento(Linha _linha, DateTime inst)
        {
            this.linha = _linha;
            this.instante = inst;
        } 
        #endregion

        #region propriedades
        internal DateTime instante { get; set; }

        #region se for uma liberação, o evento está associado a um recurso (vagao ou linha)
        internal VagaoLM vagao { get; set; }
        internal Linha linha { get; set; }
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