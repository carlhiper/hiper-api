namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userid : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.TeamModels");
            AddColumn("dbo.AspNetUsers", "UserId", c => c.Int(nullable: false));
            AlterColumn("dbo.TeamModels", "TeamId", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.TeamModels", "TeamId");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.TeamModels");
            AlterColumn("dbo.TeamModels", "TeamId", c => c.Long(nullable: false, identity: true));
            DropColumn("dbo.AspNetUsers", "UserId");
            AddPrimaryKey("dbo.TeamModels", "TeamId");
        }
    }
}
