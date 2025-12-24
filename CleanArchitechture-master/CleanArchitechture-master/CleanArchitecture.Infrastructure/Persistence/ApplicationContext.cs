using CleanArchitecture.Domain.Entites;
using CleanArchitecture.Entites.Entites;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Persistence
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Giả sử tên Model của bạn là Friend.
            // Và tên bảng thực tế trong SQL Server là 'Friendships'
            //modelBuilder.Entity<Friend>().ToTable("Friendships");

            // Hoặc nếu tên bảng thực tế là 'Friends' và schema là 'dbo'
            modelBuilder.Entity<Product>().ToTable("Products", "dbo");
            modelBuilder.Entity<User>().ToTable("Users", "dbo");
            modelBuilder.Entity<Review>().ToTable("Reviews", "dbo");
            modelBuilder.Entity<Comment>().ToTable("Comments", "dbo");
            modelBuilder.Entity<Friend>().ToTable("Friends", "dbo");
            modelBuilder.Entity<UserDetail>().ToTable("UserDetails", "dbo");

            modelBuilder.Entity<UserDetail>()
        .HasKey(ud => ud.UserId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserDetail)
                .WithOne(ud => ud.User)
                .HasForeignKey<UserDetail>(ud => ud.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa User thì xóa luôn Detail
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<UserDetail> UserDetails { get; set; }
    }
}
