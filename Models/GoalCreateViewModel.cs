using System;
using System.Collections.Generic;

namespace Hiper.Api.Models
{
    public class GoalCreateViewModel
    {
        public int GoalId { get; set; }
        public String Description { get; set; }

        public String Title { get; set; }

        public int? GoalTypeId { get; set; }


        public double? TargetAmount { get; set; }

        public double? ReachedAmount { get; set; }

        public int? RepeatId { get; set; }


        public DateTime DeadLine { get; set; }

        public int? StatusOfGoalId { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ClosedDate { get; set; }

        public ICollection<SurveyModel> Surveys { get; set; }

        public double? Hipes { get; set; }

        public string[] TeamUsers { get; set; }

        public int? TeamId { get; set; }
        public bool IsTeamGoal { get; set; }
    }
}