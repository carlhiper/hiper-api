namespace Hiper.Api.Models
{
    public class AdvancedStatisitcsViewModel
    {
        public string TeamMember { get; set; }


        public int TeamId { get; set; }

        public string TimePeriod { get; set; }

        public string PeriodOffset { get; set; }

        public bool IsAbs { get; set; }
    }
}