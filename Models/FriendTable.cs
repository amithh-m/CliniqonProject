namespace CliniqonProject.Models
{
    public class FriendTable
    {
        public int Id { get; set; } 
        public int UserId { get; set; } 
        public int FriendId { get; set; } 
        public string Status { get; set; } 
        public DateTime RequestDate { get; set; } 
        public DateTime? AcceptedDate { get; set; } 
        public DateTime? BlockedDate { get; set; }
    }
}
