using System.Collections.Generic;

namespace Hiper.Api.Models
{
    public class SurveyModel
    {
        public int SurveyId { get; set; }

        public string SurveyDescription { get; set; }

        public ICollection<GoalModel> Goals { get; set; }
    }
}