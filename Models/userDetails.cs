using System.ComponentModel.DataAnnotations;

namespace CliniqonProject.Models
{
    public class userDetails
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public DateTime DOB { get; set; }

        [Required]
        [MaxLength(50)]
        public string Designation { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string ProfilePic { get; set; }

        [Required]
        [MaxLength(50)]
        public string Country { get; set; }

        [Required]
        [MaxLength(50)]
        public string FavoriteColor { get; set; }

        [Required]
        [MaxLength(100)]
        public string FavoriteActor { get; set; }

        [Required]
        public DateTime AddedDate { get; set; }

        [Required]
        public bool Status { get; set; }
        public string Password { get; set; }
        public int FriendId { get; set; }
        public string StatusFriend { get; set; }
    }
}
