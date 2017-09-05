namespace RumoPatios.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class qtdeVagoesEmLinha : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Linha", "QtdeVagoesVazios", c => c.Int(nullable: false));
            AddColumn("dbo.Linha", "QtdeVagoesCarregados", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Linha", "QtdeVagoesCarregados");
            DropColumn("dbo.Linha", "QtdeVagoesVazios");
        }
    }
}
