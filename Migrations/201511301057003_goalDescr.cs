namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class goalDescr : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GoalModels", "Description", c => c.String());
            DropColumn("dbo.GoalModels", "Desctiption");
        }
        
        public override void Down()
        {
            AddColumn("dbo.GoalModels", "Desctiption", c => c.String());
            DropColumn("dbo.GoalModels", "Description");
        }
    }
}
