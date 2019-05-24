namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class daily : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GoalModels", "ReachedAmount", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GoalModels", "ReachedAmount");
        }
    }
}
