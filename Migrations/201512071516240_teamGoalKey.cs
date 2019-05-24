namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class teamGoalKey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GoalModels", "IsTeamGoal", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GoalModels", "IsTeamGoal");
        }
    }
}
