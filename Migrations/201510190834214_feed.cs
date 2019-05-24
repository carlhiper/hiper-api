namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class feed : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StatusGoalModels",
                c => new
                    {
                        StatusGoalId = c.Int(nullable: false, identity: true),
                        StatusGoalDescription = c.String(),
                    })
                .PrimaryKey(t => t.StatusGoalId);
            
            CreateTable(
                "dbo.GoalTypeModels",
                c => new
                    {
                        GoalTypeId = c.Int(nullable: false, identity: true),
                        GoalTypeDescription = c.String(),
                    })
                .PrimaryKey(t => t.GoalTypeId);
            
            CreateTable(
                "dbo.TeamFeedModels",
                c => new
                    {
                        TeamFeedId = c.Int(nullable: false, identity: true),
                        TeamId = c.Int(),
                        UserId = c.String(maxLength: 128),
                        GoalId = c.Int(),
                        UpdateTypeId = c.Int(),
                    })
                .PrimaryKey(t => t.TeamFeedId)
                .ForeignKey("dbo.GoalModels", t => t.GoalId)
                .ForeignKey("dbo.TeamModels", t => t.TeamId)
                .ForeignKey("dbo.UpdateTypeModels", t => t.UpdateTypeId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.TeamId)
                .Index(t => t.UserId)
                .Index(t => t.GoalId)
                .Index(t => t.UpdateTypeId);
            
            CreateTable(
                "dbo.UpdateTypeModels",
                c => new
                    {
                        UpdateTypeId = c.Int(nullable: false, identity: true),
                        UpdateTypeDescription = c.String(),
                    })
                .PrimaryKey(t => t.UpdateTypeId);
            
            AddColumn("dbo.GoalModels", "GoalTypeId", c => c.Int());
            AddColumn("dbo.GoalModels", "StatusOfGoalId", c => c.Int());
            CreateIndex("dbo.GoalModels", "GoalTypeId");
            CreateIndex("dbo.GoalModels", "StatusOfGoalId");
            AddForeignKey("dbo.GoalModels", "StatusOfGoalId", "dbo.StatusGoalModels", "StatusGoalId");
            AddForeignKey("dbo.GoalModels", "GoalTypeId", "dbo.GoalTypeModels", "GoalTypeId");
            DropColumn("dbo.GoalModels", "Type");
        }
        
        public override void Down()
        {
            AddColumn("dbo.GoalModels", "Type", c => c.String());
            DropForeignKey("dbo.TeamFeedModels", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.TeamFeedModels", "UpdateTypeId", "dbo.UpdateTypeModels");
            DropForeignKey("dbo.TeamFeedModels", "TeamId", "dbo.TeamModels");
            DropForeignKey("dbo.TeamFeedModels", "GoalId", "dbo.GoalModels");
            DropForeignKey("dbo.GoalModels", "GoalTypeId", "dbo.GoalTypeModels");
            DropForeignKey("dbo.GoalModels", "StatusOfGoalId", "dbo.StatusGoalModels");
            DropIndex("dbo.TeamFeedModels", new[] { "UpdateTypeId" });
            DropIndex("dbo.TeamFeedModels", new[] { "GoalId" });
            DropIndex("dbo.TeamFeedModels", new[] { "UserId" });
            DropIndex("dbo.TeamFeedModels", new[] { "TeamId" });
            DropIndex("dbo.GoalModels", new[] { "StatusOfGoalId" });
            DropIndex("dbo.GoalModels", new[] { "GoalTypeId" });
            DropColumn("dbo.GoalModels", "StatusOfGoalId");
            DropColumn("dbo.GoalModels", "GoalTypeId");
            DropTable("dbo.UpdateTypeModels");
            DropTable("dbo.TeamFeedModels");
            DropTable("dbo.GoalTypeModels");
            DropTable("dbo.StatusGoalModels");
        }
    }
}
