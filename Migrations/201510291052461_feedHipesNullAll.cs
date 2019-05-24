namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class feedHipesNullAll : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.GoalModels", "TargetAmount", c => c.Double());
            AlterColumn("dbo.GoalModels", "ReachedAmount", c => c.Double());
            AlterColumn("dbo.GoalModels", "CreatedDate", c => c.DateTime());
            AlterColumn("dbo.GoalModels", "ClosedDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.GoalModels", "ClosedDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.GoalModels", "CreatedDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.GoalModels", "ReachedAmount", c => c.Double(nullable: false));
            AlterColumn("dbo.GoalModels", "TargetAmount", c => c.Double(nullable: false));
        }
    }
}
