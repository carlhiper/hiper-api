using System.ComponentModel.DataAnnotations;

namespace Hiper.Api.Models
{
    public class ProfileModel
    {
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string FirstName { get; set; }

        public string NickName { get; set; }

        public string LastName { get; set; }

        public string Title { get; set; }

        public string Company { get; set; }

        public string Picture { get; set; }

        [Required]
        public string Email { get; set; }

        public bool IsApplicant { get; set; }

        public bool IsAdmin { get; set; }

        public bool EmailConfirmed { get; set; }


        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}