﻿using AutoPartsServiceWebApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace AutoPartsServiceWebApi.Data
{
    public class AutoDbContext : DbContext
    {
        private readonly string _connectionString;

        public AutoDbContext(DbContextOptions<AutoDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!string.IsNullOrEmpty(_connectionString))
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
            else
            {
                base.OnConfiguring(optionsBuilder);
            }
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
        public DbSet<Offer> Offers { get; set; }


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

            modelBuilder.Entity<UserCommon>()
                .Property(uc => uc.Avatar)
                .IsRequired(false);

            modelBuilder.Entity<UserBusiness>()
                .Property(ub => ub.Avatar)
                .IsRequired(false);

            modelBuilder.Entity<Device>()
                .Property(d => d.UserBusinessId)
                .IsRequired(false);

            modelBuilder.Entity<Device>()
                .Property(d => d.UserCommonId)
                .IsRequired(false);

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

            modelBuilder.Entity<UserCommon>()
                .HasMany(ub => ub.Services)
                .WithOne(s => s.UserCommon)
                .HasForeignKey(s => s.UserCommonId);

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

            modelBuilder.Entity<Offer>()
                .HasOne(o => o.UserCommon)
                .WithMany(uc => uc.Offers)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Offer>()
                .HasOne(o => o.Request)
                .WithMany(r => r.Offers)
                .HasForeignKey(o => o.RequestId);

            modelBuilder.Entity<UserCommon>()
                .Property(uc => uc.Name)
                .IsRequired(false);
            modelBuilder.Entity<UserCommon>()
                .Property(uc => uc.Email)
                .IsRequired(false);
            modelBuilder.Entity<UserCommon>()
                .Property(uc => uc.Password)
                .IsRequired(false);
            modelBuilder.Entity<UserCommon>()
                .Property(uc => uc.Avatar)
                .IsRequired(false);
            modelBuilder.Entity<UserBusiness>()
                .Property(ub => ub.Email)
                .IsRequired(false);
            modelBuilder.Entity<UserBusiness>()
                .Property(ub => ub.Password)
                .IsRequired(false);
            modelBuilder.Entity<UserBusiness>()
                .Property(ub => ub.Rating)
                .IsRequired(false);
            modelBuilder.Entity<UserBusiness>()
                .Property(ub => ub.Avatar)
                .IsRequired(false);

            modelBuilder.Entity<Address>()
                .Property(a => a.Country)
                .IsRequired(false);
            modelBuilder.Entity<Address>()
                .Property(a => a.Region)
                .IsRequired(false);
            modelBuilder.Entity<Address>()
                .Property(a => a.Street)
                .IsRequired(false);

            modelBuilder.Entity<Service>()
                .HasMany(s => s.Reviews)
                .WithOne(r => r.Service)
                .HasForeignKey(r => r.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
