using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RumoPatios.Models
{
    public class Vagao
    {
        internal int Idx { get; set; }

        internal string nome { get; set; }

        /// <summary>
        /// 0 - vagao, 1 - LM, 2 - LT
        /// </summary>
        internal int tipo { get; set; }

        /// <summary>
        /// -1 carregado a descarregar, 0 vazio, 1 carregado para sair do patio
        /// </summary>
        internal int status { get; set; }
    }
}