namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dailychanges : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.TeamModels", "Emails");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TeamModels", "Emails", c => c.String());
        }
    }
}
