using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RumoPatios.Models
{
    /// <summary>
    /// Tarefa de transporte de vagoes entre linhas (terminal->manobra ou manobra->terminal)
    /// </summary>
    public class Movimento
    {
        public Movimento() { }

        /// <summary>
        /// Tarefa de transporte de vagoes entre linhas (terminal->manobra ou manobra->terminal)
        /// </summary>
        /// <param name="linhaFrom">linha origem</param>
        /// <param name="linhaTo">linha destino</param>
        /// <param name="qtde">quantidade de vagoes, se carregados, positiva, negativa caso contrário</param>
        public Movimento(Linha linhaFrom, Linha linhaTo, int qtde)
        {
            this.linhaOrigem = linhaFrom;
            this.linhaDestino = linhaTo;
            this.qtdeVagoes = qtde;
        }


        public Linha linhaOrigem { get; set; }
        
        /// <summary>
        /// TODO: remover linha de destino de uma Tarefa de movimento, já que só se deve escolher a linha de destino, no momento exato do movimento
        /// </summary>
        public Linha linhaDestino { get; set; }

        /// <summary>
        /// positiva: carregados, negativa: vazios
        /// </summary>
        public int qtdeVagoes { get; set; }

    }
}