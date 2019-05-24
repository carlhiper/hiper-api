namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hipess : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.GoalModels", "Hipes", c => c.Double());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.GoalModels", "Hipes", c => c.Int());
        }
    }
}
