using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RumoPatios.Models
{
    /// <summary>
    /// Uma tarefa é um carregamento, uma chegada ou uma partida
    /// </summary>
    public class Tarefa
    {
        #region construtores
        public Tarefa() { }

        public Tarefa(Carregamento load, Random rnd)
        {
            this.instante = load.HorarioCarregamento;
            this.carregamento = load;
            this.prioridade = rnd.NextDouble();
        }

        public Tarefa(Chegada arrival, Random rnd)
        {
            this.instante = arrival.HorarioChegada;
            this.chegada = arrival;
            this.prioridade = rnd.NextDouble();
        } 
        #endregion


        #region propriedades
        /// <summary>
        /// Data de entrega
        /// </summary>
        internal DateTime instante { get; set; }

        /// <summary>
        /// prioridade da tarefa
        /// </summary>
        internal double prioridade { get; set; }

        /// <summary>
        /// 1 se foi, 0 caso contrario
        /// </summary>
        internal int concluida { get; set; }

        #region tarefa esta associada a alguma das tarefas do banco
        internal Chegada chegada { get; set; }
        internal Carregamento carregamento { get; set; }
        internal Partida partida { get; set; }
        #endregion  
        #endregion
    }
}