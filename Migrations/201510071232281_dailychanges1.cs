namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dailychanges1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TeamModels", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.TeamModels", new[] { "UserId" });
            AlterColumn("dbo.TeamModels", "UserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.TeamModels", "UserId");
            AddForeignKey("dbo.TeamModels", "UserId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TeamModels", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.TeamModels", new[] { "UserId" });
            AlterColumn("dbo.TeamModels", "UserId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.TeamModels", "UserId");
            AddForeignKey("dbo.TeamModels", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
