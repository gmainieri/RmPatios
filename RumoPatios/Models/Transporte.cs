using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RumoPatios.Models
{
    /// <summary>
    /// Solicitação de transporte de vagoes entre linhas (terminal=>manobra ou manobra=>terminal)
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
        public Transporte(Linha linhaFrom, Linha linhaTo, int qtde, bool vazios)
        {
            if (linhaFrom != null && linhaTo != null)
                return; //evita a criacao de um transporte com as duas linhas definidas (por enquanto não existe nenuma situação na qual este origem e destino são conhecidos)
            
            this.linhaOrigem = linhaFrom;
            this.linhaDestino = linhaTo;
            this.qtdeVagoes = qtde;
            this.Vazios = vazios;
        }

        /// <summary>
        /// linha de origem = linha que contém os vagoes
        /// </summary>
        public Linha linhaOrigem { get; set; }
        
        /// <summary>
        /// apenas no caso dos carregamentos, quem solicita o transporte é a linha de destino
        /// </summary>
        public Linha linhaDestino { get; set; }

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