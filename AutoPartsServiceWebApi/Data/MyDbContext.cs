using AutoPartsServiceWebApi.Models;
using Microsoft.EntityFrameworkCore;

    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Sms> SmsCodes { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PhoneNumber).IsRequired();
                entity.Property(e => e.RegistrationDate).IsRequired();
                entity.HasOne(u => u.Address)
                    .WithOne(a => a.User)
                    .HasForeignKey<Address>(a => a.UserId);
            });

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Name = "Ivan Krisko", Email = "ivan@example.com", PhoneNumber = "2443504", DeviceId = "device1", Role = Role.Buyer, RegistrationDate = DateTime.Now },
                new User { Id = 2, Name = "John Green", Email = "john@example.com", PhoneNumber = "5555678", DeviceId = "device2", Role = Role.Seller, RegistrationDate = DateTime.Now }
            );

            modelBuilder.Entity<Address>().HasData(
                new Address { Id = 1, City = "Hanoi", Country = "Vietnam", Street = "123 Main St", UserId = 1 },
                new Address { Id = 2, City = "Los Angeles", Country = "USA", Street = "456 Broadway", UserId = 2 }
            );
            modelBuilder.Entity<Sms>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PhoneNumber).IsRequired();
                entity.Property(e => e.Code).IsRequired();
                entity.HasOne(e => e.User)
                    .WithMany(u => u.SmsRecords)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }

    }