namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class orgnumber : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.TeamModels", "OrganisationNumber", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TeamModels", "OrganisationNumber", c => c.Long(nullable: false));
        }
    }
}
