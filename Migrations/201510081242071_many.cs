namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class many : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TeamModels", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.TeamModels", new[] { "UserId" });
            CreateTable(
                "dbo.UserTeam",
                c => new
                    {
                        UserRefId = c.Int(nullable: false),
                        TeamRefId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserRefId, t.TeamRefId })
                .ForeignKey("dbo.TeamModels", t => t.UserRefId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.TeamRefId, cascadeDelete: true)
                .Index(t => t.UserRefId)
                .Index(t => t.TeamRefId);
            
            DropColumn("dbo.TeamModels", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TeamModels", "UserId", c => c.String(maxLength: 128));
            DropForeignKey("dbo.UserTeam", "TeamRefId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserTeam", "UserRefId", "dbo.TeamModels");
            DropIndex("dbo.UserTeam", new[] { "TeamRefId" });
            DropIndex("dbo.UserTeam", new[] { "UserRefId" });
            DropTable("dbo.UserTeam");
            CreateIndex("dbo.TeamModels", "UserId");
            AddForeignKey("dbo.TeamModels", "UserId", "dbo.AspNetUsers", "Id");
        }
    }
}
