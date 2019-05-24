namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class amountFeed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TeamFeedModels", "CurrentAmount", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TeamFeedModels", "CurrentAmount");
        }
    }
}
