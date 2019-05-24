namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class applicants : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ApplicantsTeam",
                c => new
                    {
                        ApplicantRefId = c.Int(nullable: false),
                        TeamApplicantsRefId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ApplicantRefId, t.TeamApplicantsRefId })
                .ForeignKey("dbo.TeamModels", t => t.ApplicantRefId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.TeamApplicantsRefId, cascadeDelete: true)
                .Index(t => t.ApplicantRefId)
                .Index(t => t.TeamApplicantsRefId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ApplicantsTeam", "TeamApplicantsRefId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ApplicantsTeam", "ApplicantRefId", "dbo.TeamModels");
            DropIndex("dbo.ApplicantsTeam", new[] { "TeamApplicantsRefId" });
            DropIndex("dbo.ApplicantsTeam", new[] { "ApplicantRefId" });
            DropTable("dbo.ApplicantsTeam");
        }
    }
}
