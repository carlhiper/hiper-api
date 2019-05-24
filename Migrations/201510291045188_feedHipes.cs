namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class feedHipes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GoalModels", "Hipes", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GoalModels", "Hipes");
        }
    }
}
