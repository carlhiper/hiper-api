namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class shareProfile : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "ShareProfile", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "ShareProfile");
        }
    }
}
