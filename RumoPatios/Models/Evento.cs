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
        internal int Idx { get; set; }

        /// <summary>
        /// vagão LM liberado, linha de carregamento liberada, linha de partida liberada?, chegada de trem
        /// carregamento?, partida?
        /// </summary>
        internal int tipo { get; set; }

        internal DateTime instante { get; set; }


        #region se for uma liberação, o evento está associado a um recurso (vagao ou linha)
        internal Vagao vagao { get; set; }
        internal Linha linha { get; set; } 
        #endregion

        internal Chegada chegada { get; set; }
        internal Carregamento carregamento { get; set; }
        internal Partida partida { get; set; }
        
    }
}