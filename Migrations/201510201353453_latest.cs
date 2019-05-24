namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class latest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GoalModels", "TeamId", c => c.Int());
            CreateIndex("dbo.GoalModels", "TeamId");
            AddForeignKey("dbo.GoalModels", "TeamId", "dbo.TeamModels", "TeamId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GoalModels", "TeamId", "dbo.TeamModels");
            DropIndex("dbo.GoalModels", new[] { "TeamId" });
            DropColumn("dbo.GoalModels", "TeamId");
        }
    }
}
