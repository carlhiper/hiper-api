namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class goals : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GoalModels",
                c => new
                    {
                        GoalId = c.Int(nullable: false, identity: true),
                        Desctiption = c.String(),
                        Title = c.String(),
                        Type = c.String(),
                        TargetAmount = c.Double(nullable: false),
                        DeadLine = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.GoalId);
            
            AddColumn("dbo.AspNetUsers", "GoalId", c => c.Int(nullable: false));
            CreateIndex("dbo.AspNetUsers", "GoalId");
            AddForeignKey("dbo.AspNetUsers", "GoalId", "dbo.GoalModels", "GoalId", cascadeDelete: true);
            DropColumn("dbo.AspNetUsers", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "UserId", c => c.Int(nullable: false, identity: true));
            DropForeignKey("dbo.AspNetUsers", "GoalId", "dbo.GoalModels");
            DropIndex("dbo.AspNetUsers", new[] { "GoalId" });
            DropColumn("dbo.AspNetUsers", "GoalId");
            DropTable("dbo.GoalModels");
        }
    }
}
