namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userlang : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Language", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Language");
        }
    }
}
