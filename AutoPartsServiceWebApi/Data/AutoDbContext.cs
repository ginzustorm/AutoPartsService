using AutoPartsServiceWebApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace AutoPartsServiceWebApi.Data
{
    public class AutoDbContext : DbContext
    {
        public AutoDbContext(DbContextOptions<AutoDbContext> options)
            : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; }
        public DbSet<UserCommon> UserCommons { get; set; }
        public DbSet<UserBusiness> UserBusinesses { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Sms> Smses { get; set; }
        public DbSet<LoginSms> LoginSmses { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<DocumentUser> Documents { get; set; }
        public DbSet<Review> Reviews { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserCommon>()
                .HasOne(a => a.Address)
                .WithOne()
                .HasForeignKey<Address>(a => a.Id);

            modelBuilder.Entity<UserCommon>()
                .HasMany(uc => uc.Devices)
                .WithOne(d => d.UserCommon)
                .HasForeignKey(d => d.UserCommonId);

            modelBuilder.Entity<UserBusiness>()
                .HasMany(ub => ub.Devices)
                .WithOne(d => d.UserBusiness)
                .HasForeignKey(d => d.UserBusinessId);

            modelBuilder.Entity<UserCommon>()
                .HasOne(uc => uc.Request)
                .WithOne(r => r.UserCommon)
                .HasForeignKey<Request>(r => r.UserCommonId);

            modelBuilder.Entity<UserCommon>()
                .HasMany(uc => uc.SmsList)
                .WithOne(s => s.UserCommon)
                .HasForeignKey(s => s.UserCommonId);

            modelBuilder.Entity<UserBusiness>()
                .HasMany(ub => ub.Services)
                .WithOne(s => s.UserBusiness)
                .HasForeignKey(s => s.UserBusinessId);

            modelBuilder.Entity<UserBusiness>()
                .HasMany(ub => ub.Reviews)
                .WithOne(r => r.UserBusiness)
                .HasForeignKey(r => r.UserBusinessId);

            modelBuilder.Entity<UserBusiness>()
                .HasMany(ub => ub.SmsList)
                .WithOne(s => s.UserBusiness)
                .HasForeignKey(s => s.UserBusinessId);

            modelBuilder.Entity<UserCommon>()
                .HasMany(uc => uc.Cars)
                .WithOne(c => c.UserCommon)
                .HasForeignKey(c => c.UserCommonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserCommon>()
                .HasMany(uc => uc.Documents)
                .WithOne(d => d.UserCommon)
                .HasForeignKey(d => d.UserCommonId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
