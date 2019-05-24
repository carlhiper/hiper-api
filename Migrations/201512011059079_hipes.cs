namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hipes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TeamFeedModels", "Hipes", c => c.Int());
           
        }
        
        public override void Down()
        {
          
            DropColumn("dbo.TeamFeedModels", "Hipes");
        }
    }
}
