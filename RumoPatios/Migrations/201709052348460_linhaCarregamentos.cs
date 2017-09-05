namespace RumoPatios.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class linhaCarregamentos : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Carregamento", "LinhaID", c => c.Int(nullable: false));
            CreateIndex("dbo.Carregamento", "LinhaID");
            AddForeignKey("dbo.Carregamento", "LinhaID", "dbo.Linha", "LinhaID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Carregamento", "LinhaID", "dbo.Linha");
            DropIndex("dbo.Carregamento", new[] { "LinhaID" });
            DropColumn("dbo.Carregamento", "LinhaID");
        }
    }
}
