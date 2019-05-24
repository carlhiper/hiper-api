using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Hiper.Api.Models
{
    public class UserModel : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string NickName { get; set; }

        public string Title { get; set; }

        public string Company { get; set; }

        public string Picture { get; set; }

        public string Language { get; set; }


        public DateTime RegDate { get; set; }
        public Object Rating { get; set; }
        public virtual ICollection<TeamModel> Teams { get; set; }

        public virtual ICollection<TeamModel> TeamsApplicants { get; set; }

        public virtual ICollection<GoalModel> Goals { get; set; }
    }
}