using System.ComponentModel.DataAnnotations;

namespace CliniqonProject.Models
{
    public class LoginTbl
    {
        [Key]
        public int LoginID { get; set; }

        [Required]
        public int UserId { get; set; }
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; } 

        [Required]
        public bool Status { get; set; }
    }
}
