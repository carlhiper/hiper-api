namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class feedCreated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TeamFeedModels", "CreationDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TeamFeedModels", "CreationDate");
        }
    }
}
