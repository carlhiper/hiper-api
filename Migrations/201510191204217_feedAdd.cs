namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class feedAdd : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.UserGoal", name: "TeamRefId", newName: "GoalRefId");
            RenameIndex(table: "dbo.UserGoal", name: "IX_TeamRefId", newName: "IX_GoalRefId");
            CreateTable(
                "dbo.RepeatModels",
                c => new
                    {
                        RepeatId = c.Int(nullable: false, identity: true),
                        RepeatDescription = c.String(),
                    })
                .PrimaryKey(t => t.RepeatId);
            
            CreateTable(
                "dbo.SurveyModels",
                c => new
                    {
                        SurveyId = c.Int(nullable: false, identity: true),
                        SurveyDescription = c.String(),
                    })
                .PrimaryKey(t => t.SurveyId);
            
            CreateTable(
                "dbo.SurveyGoal",
                c => new
                    {
                        SurveyRefId = c.Int(nullable: false),
                        GoalSurveyRefId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.SurveyRefId, t.GoalSurveyRefId })
                .ForeignKey("dbo.GoalModels", t => t.SurveyRefId, cascadeDelete: true)
                .ForeignKey("dbo.SurveyModels", t => t.GoalSurveyRefId, cascadeDelete: true)
                .Index(t => t.SurveyRefId)
                .Index(t => t.GoalSurveyRefId);
            
            AddColumn("dbo.GoalModels", "RepeatId", c => c.Int());
            AddColumn("dbo.GoalModels", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.GoalModels", "ClosedDate", c => c.DateTime(nullable: false));
            CreateIndex("dbo.GoalModels", "RepeatId");
            AddForeignKey("dbo.GoalModels", "RepeatId", "dbo.RepeatModels", "RepeatId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SurveyGoal", "GoalSurveyRefId", "dbo.SurveyModels");
            DropForeignKey("dbo.SurveyGoal", "SurveyRefId", "dbo.GoalModels");
            DropForeignKey("dbo.GoalModels", "RepeatId", "dbo.RepeatModels");
            DropIndex("dbo.SurveyGoal", new[] { "GoalSurveyRefId" });
            DropIndex("dbo.SurveyGoal", new[] { "SurveyRefId" });
            DropIndex("dbo.GoalModels", new[] { "RepeatId" });
            DropColumn("dbo.GoalModels", "ClosedDate");
            DropColumn("dbo.GoalModels", "CreatedDate");
            DropColumn("dbo.GoalModels", "RepeatId");
            DropTable("dbo.SurveyGoal");
            DropTable("dbo.SurveyModels");
            DropTable("dbo.RepeatModels");
            RenameIndex(table: "dbo.UserGoal", name: "IX_GoalRefId", newName: "IX_TeamRefId");
            RenameColumn(table: "dbo.UserGoal", name: "GoalRefId", newName: "TeamRefId");
        }
    }
}
