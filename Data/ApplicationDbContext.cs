using Microsoft.EntityFrameworkCore;
using CliniqonProject.Models;

namespace CliniqonProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<LoginTbl> LoginTbl { get; set; }
        public DbSet<UserTbl> UserTbl { get; set; }
        public DbSet<FriendTable> FriendTable { get; set; }
    }
}
