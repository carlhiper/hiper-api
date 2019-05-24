namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class feedHipesNull : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.GoalModels", "Hipes", c => c.Double());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.GoalModels", "Hipes", c => c.Double(nullable: false));
        }
    }
}
