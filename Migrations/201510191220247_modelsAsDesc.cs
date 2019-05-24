namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class modelsAsDesc : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "RegDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.TeamModels", "RegDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.TeamModels", "MaxUserPrevPerMonth", c => c.Int(nullable: false));
            AddColumn("dbo.TeamModels", "AdministratorId", c => c.String(maxLength: 128));
            CreateIndex("dbo.TeamModels", "AdministratorId");
            AddForeignKey("dbo.TeamModels", "AdministratorId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TeamModels", "AdministratorId", "dbo.AspNetUsers");
            DropIndex("dbo.TeamModels", new[] { "AdministratorId" });
            DropColumn("dbo.TeamModels", "AdministratorId");
            DropColumn("dbo.TeamModels", "MaxUserPrevPerMonth");
            DropColumn("dbo.TeamModels", "RegDate");
            DropColumn("dbo.AspNetUsers", "RegDate");
        }
    }
}
