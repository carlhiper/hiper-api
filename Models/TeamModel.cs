using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hiper.Api.Models
{
    public class TeamModel
    {
        public int TeamId { get; set; }
        public String TeamName { get; set; }

        public String CompanyName { get; set; }

        public string OrganisationNumber { get; set; }

        public String BillingAddress { get; set; }

        public DateTime RegDate { get; set; }

        public int MaxUserPrevPerMonth { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }


        public virtual ICollection<UserModel> Users { get; set; }

        public virtual ICollection<UserModel> Applicants { get; set; }


        public string AdministratorId { get; set; }

        [ForeignKey("AdministratorId")]
        public virtual UserModel Administrator { get; set; }


        public int? TeamTypeId { get; set; }

        [ForeignKey("TeamTypeId")]
        public virtual TeamTypesModel TeamType { get; set; }

        public bool Active { get; set; }
    }

    public class JoinTeamViewModel
    {
        public String TeamName { get; set; }

        public String AdminEmail { get; set; }
        public string Password { get; set; }
    }

    public class TeamCreationViewModel
    {
        public String TeamName { get; set; }

        public String CompanyName { get; set; }

        public string OrganisationNumber { get; set; }

        public String BillingAddress { get; set; }

        public String TeamTypeCode { get; set; }

        public String[] Emails { get; set; }


        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}