using System;

namespace Hiper.Api.Models
{
    public class FilterViewModel
    {
        public string TeamMember { get; set; }
        public int TypeOfUpdate { get; set; }
        public int TypeOfGoal { get; set; }
        public DateTime TimeFrom { get; set; }
        public DateTime TimeTo { get; set; }

        public int Feedback { get; set; }

        public int Status { get; set; }


        public int TeamId { get; set; }

        public DateTime LastDate { get; set; }

        public int ScrollParam { get; set; }
    }
}