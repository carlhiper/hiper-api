using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hiper.Api.Models
{
    public class HipeModel
    {
        public int HipeId { get; set; }
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; }

        public int? GoalId { get; set; }

        [ForeignKey("GoalId")]
        public virtual GoalModel Goal { get; set; }

        public int? FeedId { get; set; }

        [ForeignKey("FeedId")]
        public virtual TeamFeedModel Feed { get; set; }

        public DateTime CreationDate { get; set; }
    }
}