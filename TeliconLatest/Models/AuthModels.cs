using System.ComponentModel.DataAnnotations;

namespace TeliconLatest.Models
{
    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
    public class ForgetPassword
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
    public class TeliconUser
    {
        [Required]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }
        public string OldUserName { get; set; }
        public string OldRole { get; set; }
        [Required]
        public bool IsNew { get; set; }
        [Required]
        [Display(Name = "Active")]
        public bool IsApproved { get; set; }
        [Required]
        public string Role { get; set; }
        public ProfileInfo Profile { get; set; }
    }
    public class TeliconUserFull : TeliconUser
    {
        public int LocationID { get; set; }
        public ProfileFull ProfileFullInfo { get; set; }
    }
    public class LoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
    public class ProfileInfo
    {
        [Required]
        public string Phone { get; set; }
        [Display(Name = "Alternate Phone")]
        public string AltPhone { get; set; }
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
    }
    public class ProfileFull : ProfileInfo
    {
        public int ProfileID { get; set; }
        public string Role { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLocked { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public int LocationID { get; set; }
        public int AreaID { get; set; }
    }
}