using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hiper.Api.Models
{
    public class GoalModel
    {
        public int GoalId { get; set; }
        public String Description { get; set; }

        public String Title { get; set; }

        public int? GoalTypeId { get; set; }

        [ForeignKey("GoalTypeId")]
        public virtual GoalTypeModel Type { get; set; }

        public double? TargetAmount { get; set; }

        public double? ReachedAmount { get; set; }

        public int? RepeatId { get; set; }

        [ForeignKey("RepeatId")]
        public virtual RepeatModel Repeat { get; set; }

        public DateTime DeadLine { get; set; }

        public int? StatusOfGoalId { get; set; }

        [ForeignKey("StatusOfGoalId")]
        public virtual StatusGoalModel StatusOfGoal { get; set; }

        public virtual ICollection<UserModel> Participants { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ClosedDate { get; set; }

        public virtual ICollection<SurveyModel> Surveys { get; set; }

        public double? Hipes { get; set; }

        public bool IsTeamGoal { get; set; }

        public int? TeamId { get; set; }

        [ForeignKey("TeamId")]
        public virtual TeamModel Team { get; set; }
    }
}