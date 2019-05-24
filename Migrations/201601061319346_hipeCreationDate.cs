namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hipeCreationDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.HipeModels", "CreationDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.HipeModels", "CreationDate");
        }
    }
}
