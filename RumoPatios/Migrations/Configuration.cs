namespace RumoPatios.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using RumoPatios.Models;
    using System.Collections.Generic;

    internal sealed class Configuration : DbMigrationsConfiguration<RumoPatios.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(RumoPatios.Models.ApplicationDbContext context)
        {
            context.Configuration.AutoDetectChangesEnabled = false;

            context.Chegadas.AddOrUpdate(
                x => x.prefixo,
                new Chegada { prefixo = "D34", HorarioChegada = DateTime.Parse("16/08/2017 04:00"), QtdeVagoesCarregados = 91, QtdeVagoesVazio = 0 },
                new Chegada { prefixo = "D02", HorarioChegada = DateTime.Parse("16/08/2017 07:00"), QtdeVagoesCarregados = 83, QtdeVagoesVazio = 0 },
                new Chegada { prefixo = "D36", HorarioChegada = DateTime.Parse("16/08/2017 10:00"), QtdeVagoesCarregados = 80, QtdeVagoesVazio = 0 },
                new Chegada { prefixo = "D38", HorarioChegada = DateTime.Parse("16/08/2017 11:00"), QtdeVagoesCarregados = 78, QtdeVagoesVazio = 0 },
                new Chegada { prefixo = "L04", HorarioChegada = DateTime.Parse("16/08/2017 13:00"), QtdeVagoesCarregados = 40, QtdeVagoesVazio = 20 },
                new Chegada { prefixo = "D26", HorarioChegada = DateTime.Parse("16/08/2017 15:00"), QtdeVagoesCarregados = 79, QtdeVagoesVazio = 0 },
                new Chegada { prefixo = "K70", HorarioChegada = DateTime.Parse("16/08/2017 20:00"), QtdeVagoesCarregados = 80, QtdeVagoesVazio = 0 }
                );

            context.Partidas.AddOrUpdate(
                x => x.prefixo,
                new Partida { prefixo = "M33", HorarioPartida = DateTime.Parse("16/08/2017 13:00"), QtdeVagoesCarregados = 0, QtdeVagoesVazio = 40 },
                new Partida { prefixo = "M31", HorarioPartida = DateTime.Parse("16/08/2017 18:00"), QtdeVagoesCarregados = 60, QtdeVagoesVazio = 20 },
                new Partida { prefixo = "L05", HorarioPartida = DateTime.Parse("16/08/2017 19:30"), QtdeVagoesCarregados = 70, QtdeVagoesVazio = 10 },
                new Partida { prefixo = "M05", HorarioPartida = DateTime.Parse("16/08/2017 22:00"), QtdeVagoesCarregados = 40, QtdeVagoesVazio = 40 },
                new Partida { prefixo = "L07", HorarioPartida = DateTime.Parse("16/08/2017 23:30"), QtdeVagoesCarregados = 0, QtdeVagoesVazio = 40 }
                );

            var LinhasList = new List<Linha>();
            LinhasList.Add(new Linha { LinhaID = 1, Nome = "DPL28", NomeTerminal = "T1", Capacidade = 41, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 2, Nome = "PASA L6", NomeTerminal = "T2", Capacidade = 59, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 3, Nome = "MARTINI M", NomeTerminal = "T3", Capacidade = 59, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 4, Nome = "DPL32", NomeTerminal = "T4", Capacidade = 26, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 5, Nome = "LJT", NomeTerminal = "T5", Capacidade = 59, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 6, Nome = "LKY", NomeTerminal = "T6", Capacidade = 59, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 7, Nome = "K5LP", NomeTerminal = "", Capacidade = 118, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 50 });
            LinhasList.Add(new Linha { LinhaID = 8, Nome = "K5L01", NomeTerminal = "", Capacidade = 118, QtdeVagoesCarregados = 50, QtdeVagoesVazios = 20 });
            LinhasList.Add(new Linha { LinhaID = 9, Nome = "K5L02", NomeTerminal = "", Capacidade = 118, QtdeVagoesCarregados = 30, QtdeVagoesVazios = 25 });
            LinhasList.Add(new Linha { LinhaID = 10, Nome = "K5L04", NomeTerminal = "", Capacidade = 88, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 11, Nome = "K5L06", NomeTerminal = "", Capacidade = 88, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 12, Nome = "K5L08", NomeTerminal = "", Capacidade = 88, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 13, Nome = "K5L10", NomeTerminal = "", Capacidade = 88, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 14, Nome = "K5L12", NomeTerminal = "", Capacidade = 118, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 30 });
            LinhasList.Add(new Linha { LinhaID = 15, Nome = "K5L14", NomeTerminal = "", Capacidade = 88, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 16, Nome = "K5L16", NomeTerminal = "", Capacidade = 88, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 17, Nome = "K5L17", NomeTerminal = "", Capacidade = 88, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 18, Nome = "K5L18", NomeTerminal = "", Capacidade = 118, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 19, Nome = "K5L19", NomeTerminal = "", Capacidade = 59, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 20, Nome = "K5L20", NomeTerminal = "", Capacidade = 59, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 50 });
            LinhasList.Add(new Linha { LinhaID = 21, Nome = "K5L21", NomeTerminal = "", Capacidade = 59, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 50 });
            LinhasList.Add(new Linha { LinhaID = 22, Nome = "K5L22", NomeTerminal = "", Capacidade = 59, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 50 });
            LinhasList.Add(new Linha { LinhaID = 23, Nome = "K5L23", NomeTerminal = "", Capacidade = 59, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 24, Nome = "K5L24", NomeTerminal = "", Capacidade = 18, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 25, Nome = "K5L25", NomeTerminal = "", Capacidade = 59, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 30 });
            LinhasList.Add(new Linha { LinhaID = 26, Nome = "K5L27", NomeTerminal = "", Capacidade = 18, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 27, Nome = "K5L29", NomeTerminal = "", Capacidade = 18, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 28, Nome = "K5L31", NomeTerminal = "", Capacidade = 59, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 29, Nome = "L999", NomeTerminal = "", Capacidade = 18, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 30, Nome = "MNB", NomeTerminal = "", Capacidade = 59, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 31, Nome = "XXXD", NomeTerminal = "", Capacidade = 54, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            LinhasList.Add(new Linha { LinhaID = 32, Nome = "XXXP", NomeTerminal = "", Capacidade = 10, QtdeVagoesCarregados = 0, QtdeVagoesVazios = 0 });
            //var LinhasArray = LinhasList.ToArray();
            context.Linhas.AddOrUpdate(x => x.LinhaID, LinhasList.ToArray());


            context.Carregamentos.AddOrUpdate(
                x => new { x.Terminal, x.Cliente, x.Produto },
                new Carregamento { LinhaID = 4, Cliente = "Ipiranga", Produto = "Oleo vegetal", QtdeVagoes = 70, HorarioCarregamento = DateTime.Parse("16/08/2017 08:00") },
                new Carregamento { LinhaID = 1, Cliente = "Cargill", Produto = "Fertilizante", QtdeVagoes = 40, HorarioCarregamento = DateTime.Parse("16/08/2017 14:00") },
                new Carregamento { LinhaID = 3, Cliente = "Bunge", Produto = "Fertilizante", QtdeVagoes = 30, HorarioCarregamento = DateTime.Parse("16/08/2017 15:30") }
                );

            //  This method will be called after migrating to the latest version.
            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
