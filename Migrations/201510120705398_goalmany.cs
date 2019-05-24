namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class goalmany : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AspNetUsers", "GoalId", "dbo.GoalModels");
            DropIndex("dbo.AspNetUsers", new[] { "GoalId" });
            CreateTable(
                "dbo.UserGoal",
                c => new
                    {
                        UserRefId = c.Int(nullable: false),
                        TeamRefId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserRefId, t.TeamRefId })
                .ForeignKey("dbo.GoalModels", t => t.UserRefId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.TeamRefId, cascadeDelete: true)
                .Index(t => t.UserRefId)
                .Index(t => t.TeamRefId);
            
            DropColumn("dbo.AspNetUsers", "GoalId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "GoalId", c => c.Int());
            DropForeignKey("dbo.UserGoal", "TeamRefId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserGoal", "UserRefId", "dbo.GoalModels");
            DropIndex("dbo.UserGoal", new[] { "TeamRefId" });
            DropIndex("dbo.UserGoal", new[] { "UserRefId" });
            DropTable("dbo.UserGoal");
            CreateIndex("dbo.AspNetUsers", "GoalId");
            AddForeignKey("dbo.AspNetUsers", "GoalId", "dbo.GoalModels", "GoalId");
        }
    }
}
