namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class goalnull1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AspNetUsers", "GoalId", "dbo.GoalModels");
            DropIndex("dbo.AspNetUsers", new[] { "GoalId" });
            AlterColumn("dbo.AspNetUsers", "GoalId", c => c.Int());
            CreateIndex("dbo.AspNetUsers", "GoalId");
            AddForeignKey("dbo.AspNetUsers", "GoalId", "dbo.GoalModels", "GoalId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "GoalId", "dbo.GoalModels");
            DropIndex("dbo.AspNetUsers", new[] { "GoalId" });
            AlterColumn("dbo.AspNetUsers", "GoalId", c => c.Int(nullable: false));
            CreateIndex("dbo.AspNetUsers", "GoalId");
            AddForeignKey("dbo.AspNetUsers", "GoalId", "dbo.GoalModels", "GoalId", cascadeDelete: true);
        }
    }
}
