namespace Hiper.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hipe : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.HipeModels",
                c => new
                    {
                        HipeId = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        GoalId = c.Int(),
                        FeedId = c.Int(),
                    })
                .PrimaryKey(t => t.HipeId)
                .ForeignKey("dbo.TeamFeedModels", t => t.FeedId)
                .ForeignKey("dbo.GoalModels", t => t.GoalId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.GoalId)
                .Index(t => t.FeedId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.HipeModels", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.HipeModels", "GoalId", "dbo.GoalModels");
            DropForeignKey("dbo.HipeModels", "FeedId", "dbo.TeamFeedModels");
            DropIndex("dbo.HipeModels", new[] { "FeedId" });
            DropIndex("dbo.HipeModels", new[] { "GoalId" });
            DropIndex("dbo.HipeModels", new[] { "UserId" });
            DropTable("dbo.HipeModels");
        }
    }
}
