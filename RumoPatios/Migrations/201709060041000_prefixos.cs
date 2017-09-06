namespace RumoPatios.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class prefixos : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Chegada", "prefixo", c => c.String());
            AddColumn("dbo.Partida", "prefixo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Partida", "prefixo");
            DropColumn("dbo.Chegada", "prefixo");
        }
    }
}
