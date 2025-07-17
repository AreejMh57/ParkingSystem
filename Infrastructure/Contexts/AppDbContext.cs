using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Reflection.Emit;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Contexts
{
    public class AppDbContext : IdentityDbContext<User>
    {
   
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Define all tables
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Garage> Garages { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<PaymentTransaction> Transactions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermission { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureEntities(modelBuilder);
            ConfigureRelationships(modelBuilder);
        }

        // Configure the basic properties for each entity
        private void ConfigureEntities(ModelBuilder modelBuilder)
        {
            // Configure Booking
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.BookingId);
               
            
                entity.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");
            });

            // Configure Garage
            modelBuilder.Entity<Garage>(entity =>
            {
                entity.HasKey(g => g.GarageId);
                entity.Property(g => g.PricePerHour).HasColumnType("decimal(18,2)");
                entity.Property(g => g.Name).HasMaxLength(100);
                entity.Property(g => g.Location).HasMaxLength(200);
            });

            // Configure Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.NotificationId);
                entity.Property(n => n.Channel).HasMaxLength(50);
                entity.Property(n => n.Message).HasMaxLength(500);
            });

            // Configure Permission

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(p => p.PermissionId);

            });
            // Configure RolePermission
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(r => new {r.PermissionId ,r.RoleId });

            });







            // Configure Sensor
            modelBuilder.Entity<Sensor>(entity =>
            {
                entity.HasKey(s => s.SensorId);
               
            });

            // Configure Token
            modelBuilder.Entity<Token>(entity =>
            {
                entity.HasKey(t => t.TokenId);
                entity.Property(t => t.Value).HasMaxLength(500);
            });

            // Configure Transaction
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(pt => pt.TransactionId);
                entity.Property(pt => pt.Amount).HasColumnType("decimal(18,2)");
          
            });

            // Configure Wallet
            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasKey(w => w.WalletId);
                entity.Property(w => w.Balance).HasColumnType("decimal(18,2)");

            }); 

            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
               
                entity.Property(u => u.Email).HasMaxLength(100);
                entity.Property(u => u.PasswordHash).HasMaxLength(500);
            });
        }

        // Configure all relationships
        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // 1. User -> Wallet (One-to-one)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Wallet)
                .WithOne(w => w.User)
                .HasForeignKey<User>(w => w.WalletId)
                .OnDelete(DeleteBehavior.Restrict);


            // 2.Wallet ->PaymentTransaction  (One-to-Many)
            modelBuilder.Entity<Wallet>()
                .HasMany(w => w.Transactions)
                .WithOne(pt => pt.Wallet)
                .HasForeignKey(pt => pt.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. User -> Bookings (One-to-Many)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Bookings)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Garage -> Bookings (One-to-Many)
            modelBuilder.Entity<Garage>()
                .HasMany(g => g.Bookings)
                .WithOne(b => b.Garage)
                .HasForeignKey(b => b.GarageId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. Booking -> Token (One-to-One)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Token)
                .WithOne(t => t.Booking)
                .HasForeignKey<Token>(t => t.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // 6. Booking -> PaymentTransaction (One-to-One)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.PaymentTransaction)
                .WithOne(pt => pt.Booking)
                .HasForeignKey<PaymentTransaction>(pt => pt.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // 7. User -> Notifications (One-to-Many)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 8. Booking -> Notifications (One-to-Many)
            modelBuilder.Entity<Booking>()
                .HasMany(b => b.Notifications)
                .WithOne(n => n.Booking)
                .HasForeignKey(n => n.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // 9. Garage -> Sensors (One-to-Many)
            modelBuilder.Entity<Garage>()
                .HasMany(g => g.Sensors)
                .WithOne(s => s.Garage)
                .HasForeignKey(s => s.GarageId)
                .OnDelete(DeleteBehavior.Restrict);

            // 9. Permission -> RolePermission (One-to-Many)
            modelBuilder.Entity<Permission>()
                .HasMany(p => p.RolePermissions)
                .WithOne(r => r.Permission)
                .HasForeignKey(r => r.PermissionId)
                .OnDelete(DeleteBehavior.Restrict);


        }



         
        // Automatically manage dates
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }

    // Base class for entities
    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
