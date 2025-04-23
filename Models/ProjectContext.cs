using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using mvc.Models;
using mvc.Models.Authorize;

namespace mvc.Models
{
    public class ProjectContext : IdentityDbContext<ApplicationUser>
    {
        public ProjectContext(DbContextOptions<ProjectContext> options) : base(options)
        {
        }
        
        public DbSet<Business> Businesses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CategoryFeatures> CategoryFeatures { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<OpeningHour> OpeningHours { get; set; }
        public DbSet<Ad> Ads { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Checkout> Checkouts { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<BusinessFeatures> BusinessFeatures { get; set; }

       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           
            modelBuilder.Entity<Business>()
                .HasOne(b => b.Package)
                .WithMany(p => p.Businesses)
                .HasForeignKey(b => b.PackageId);
            
           
            modelBuilder.Entity<Business>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<Category>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<CategoryFeatures>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<Review>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<OpeningHour>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<Ad>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<Checkout>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<Conversation>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<ChatMessage>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<Favorite>().Property(e => e.Id).UseIdentityColumn();
            modelBuilder.Entity<BusinessFeatures>().Property(e => e.Id).UseIdentityColumn();
            
            
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Business)
                .WithMany()
                .HasForeignKey(f => f.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);
                
           
            modelBuilder.Entity<Package>()
                .Property(p => p.Id)
                .ValueGeneratedNever();
                
            
            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.BusinessId })
                .IsUnique();

         

           
            modelBuilder.Entity<Package>()
                .HasMany(p => p.Features)
                .WithOne(f => f.Package)
                .HasForeignKey(f => f.PackageId)
                .OnDelete(DeleteBehavior.Cascade);
    
         
        }
    }
}
