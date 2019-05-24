using System;
using System.Data.Entity;
using Hiper.Api.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Hiper.Api
{
    public class AppContext : IdentityDbContext<UserModel>
    {
        public AppContext()
            : base("AppContext")
        {
        }

        public DbSet<TeamModel> Teams { get; set; }
        public DbSet<GoalModel> Goals { get; set; }

        public DbSet<TeamFeedModel> Feeds { get; set; }

        public DbSet<GoalTypeModel> GoalType { get; set; }

        public DbSet<RepeatModel> Repeat { get; set; }

        public DbSet<SurveyModel> Surveys { get; set; }

        public DbSet<StatusGoalModel> StatusGoal { get; set; }

        public DbSet<UpdateTypeModel> UpdateType { get; set; }

        public DbSet<HipeModel> Hipes { get; set; }

        public DbSet<TeamTypesModel> TeamTypes { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException("modelBuilder");
            }
            base.OnModelCreating(modelBuilder);

//            modelBuilder.Entity<UserModel>().HasKey<int>(u => u.UserId);

            modelBuilder.Entity<GoalModel>().HasKey(g => g.GoalId);
            modelBuilder.Entity<TeamModel>().HasKey(l => l.TeamId);
            modelBuilder.Entity<GoalTypeModel>().HasKey(g => g.GoalTypeId);
            modelBuilder.Entity<StatusGoalModel>().HasKey(g => g.StatusGoalId);
            modelBuilder.Entity<TeamFeedModel>().HasKey(g => g.TeamFeedId);
            modelBuilder.Entity<UpdateTypeModel>().HasKey(g => g.UpdateTypeId);
            modelBuilder.Entity<RepeatModel>().HasKey(g => g.RepeatId);
            modelBuilder.Entity<SurveyModel>().HasKey(g => g.SurveyId);
            modelBuilder.Entity<HipeModel>().HasKey(g => g.HipeId);
            modelBuilder.Entity<TeamTypesModel>().HasKey(g => g.TeamTypeId);

//            modelBuilder.Entity<UserModel>()
//                .HasOptional<GoalModel>(g => g.Goal)
//                .WithMany(s => s.Participants)
//                .HasForeignKey(s => s.GoalId);
            //one-to-many 
//            modelBuilder.Entity<TeamFeedModel>()
//                        .HasOptional<UserModel>(s => s.User)
//                        .WithMany(s => s.)
//                        .HasForeignKey(s => s.UserId); ;

            modelBuilder.Entity<TeamModel>()
                .HasMany<UserModel>(s => s.Users)
                .WithMany(c => c.Teams)
                .Map(cs =>
                {
                    cs.MapLeftKey("UserRefId");
                    cs.MapRightKey("TeamRefId");
                    cs.ToTable("UserTeam");
                });

            modelBuilder.Entity<TeamModel>()
                .HasMany<UserModel>(s => s.Applicants)
                .WithMany(c => c.TeamsApplicants)
                .Map(cs =>
                {
                    cs.MapLeftKey("ApplicantRefId");
                    cs.MapRightKey("TeamApplicantsRefId");
                    cs.ToTable("ApplicantsTeam");
                });
            modelBuilder.Entity<GoalModel>()
                .HasMany<UserModel>(s => s.Participants)
                .WithMany(c => c.Goals)
                .Map(cs =>
                {
                    cs.MapLeftKey("UserRefId");
                    cs.MapRightKey("GoalRefId");
                    cs.ToTable("UserGoal");
                });
            modelBuilder.Entity<GoalModel>()
                .HasMany<SurveyModel>(s => s.Surveys)
                .WithMany(c => c.Goals)
                .Map(cs =>
                {
                    cs.MapLeftKey("SurveyRefId");
                    cs.MapRightKey("GoalSurveyRefId");
                    cs.ToTable("SurveyGoal");
                });
        }
    }
}