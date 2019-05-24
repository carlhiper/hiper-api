namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class shareOff : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "ShareProfile");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "ShareProfile", c => c.Boolean(nullable: false));
        }
    }
}
