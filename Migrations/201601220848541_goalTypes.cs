namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class goalTypes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TeamTypesModels",
                c => new
                    {
                        TeamTypeId = c.Int(nullable: false, identity: true),
                        TeamTypeDescription = c.String(),
                        TeamMembersCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TeamTypeId);
            
            AddColumn("dbo.TeamModels", "TeamTypeId", c => c.Int());
            CreateIndex("dbo.TeamModels", "TeamTypeId");
            AddForeignKey("dbo.TeamModels", "TeamTypeId", "dbo.TeamTypesModels", "TeamTypeId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TeamModels", "TeamTypeId", "dbo.TeamTypesModels");
            DropIndex("dbo.TeamModels", new[] { "TeamTypeId" });
            DropColumn("dbo.TeamModels", "TeamTypeId");
            DropTable("dbo.TeamTypesModels");
        }
    }
}
