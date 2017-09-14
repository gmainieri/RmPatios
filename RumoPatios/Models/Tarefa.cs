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

        public Tarefa(Descarga descarga, DateTime instante, Random rnd)
        {
            this.instante = instante;
            this.descarga = descarga;
            this.prioridade = rnd.NextDouble();
        }

        public Tarefa(Movimento mov, DateTime inst, Random rnd)
        {
            this.instante = inst;
            this.movimento = mov;
            this.prioridade = rnd.NextDouble();
        }
        #endregion


        #region propriedades
        /// <summary>
        /// Os carregamentos devem ser concluidos antes do instante, as chegadas devem ser tratadas a partir do instante. 
        /// </summary>
        internal DateTime instante { get; set; }

        /// <summary>
        /// prioridade da tarefa
        /// </summary>
        public double prioridade { get; set; }

        /// <summary>
        /// 1 se foi, 0 caso contrario
        /// </summary>
        public int concluida { get; set; }

        #region tarefa esta associada a alguma das tarefas do banco
        public Chegada chegada { get; set; }
        public Carregamento carregamento { get; set; }
        public Partida partida { get; set; }
        public Descarga descarga { get; set; }
        public Movimento movimento { get; set; }
        #endregion  
        #endregion
    }
}