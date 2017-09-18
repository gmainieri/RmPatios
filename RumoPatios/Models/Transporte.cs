using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RumoPatios.Models
{
    /// <summary>
    /// Tarefa de transporte de vagoes entre linhas (terminal->manobra ou manobra->terminal)
    /// </summary>
    public class Transporte
    {
        public Transporte() { }

        /// <summary>
        /// Tarefa de transporte de vagoes entre linhas (terminal->manobra ou manobra->terminal)
        /// </summary>
        /// <param name="linhaFrom">linha origem</param>
        /// <param name="linhaTo">linha destino</param>
        /// <param name="qtde">quantidade de vagoes, se carregados, positiva, negativa caso contrário</param>
        //public Transporte(Linha linhaFrom, Linha linhaTo, int qtde, bool vazios)
        public Transporte(Linha linhaFrom, int qtde, bool vazios)
        {
            this.linhaOrigem = linhaFrom;
            //this.linhaDestino = linhaTo;
            this.qtdeVagoes = qtde;
            this.Vazios = vazios;
        }


        public Linha linhaOrigem { get; set; }
        
        /// <summary>
        /// TODO: remover linha de destino de uma Tarefa de movimento, já que só se deve escolher a linha de destino, no momento exato do movimento
        /// </summary>
        //public Linha linhaDestino { get; set; }

        /// <summary>
        /// positiva: carregados, negativa: vazios
        /// </summary>
        public int qtdeVagoes { get; set; }

        /// <summary>
        /// 1 se a qtde de vagoes refere-se a vagoes vazios, zero c.c.
        /// </summary>
        public bool Vazios { get; set; }

    }
}