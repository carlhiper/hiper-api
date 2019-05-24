using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hiper.Api.Models
{
    public class TeamFeedModel
    {
        public int TeamFeedId { get; set; }


        public int? TeamId { get; set; }

        [ForeignKey("TeamId")]
        public virtual TeamModel Team { get; set; }


        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; }

        public int? GoalId { get; set; }

        [ForeignKey("GoalId")]
        public virtual GoalModel Goal { get; set; }

        public int? UpdateTypeId { get; set; }

        public int? Hipes { get; set; }

        [ForeignKey("UpdateTypeId")]
        public virtual UpdateTypeModel UpdateType { get; set; }

        public DateTime CreationDate { get; set; }

        public double? CurrentAmount { get; set; }
    }
}