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
        /// <summary>
        /// Data de entrega
        /// </summary>
        internal DateTime instante { get; set; }

        /// <summary>
        /// prioridade da tarefa
        /// </summary>
        internal double prioridade { get; set; }
    }
}