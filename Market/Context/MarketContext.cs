using Microsoft.EntityFrameworkCore;

namespace Market.Context
{
    public class MarketContext : DbContext
    {
        public MarketContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbUser>().HasMany<DbBooking>().WithOne().HasForeignKey(x => x.BookerUserId);
            //modelBuilder.Entity<DbUser>().HasMany<DbBooking>().WithOne().HasForeignKey(x => x.OwnerUserId);

            modelBuilder.Entity<DbCategory>();

            modelBuilder.Entity<DbProduct>()
                .HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId);


            modelBuilder.Entity<DbBooking>()
    .HasOne(x => x.BookerUser)
    .WithMany()
    .HasForeignKey(x => x.BookerUserId);

        }

        public DbSet<DbUser> Users { get; set; }
        public DbSet<DbCategory> Categories { get; set; }
        public DbSet<DbProduct> Products { get; set; }

        public DbSet<DbBooking> Bookings { get; set; }
    }
}