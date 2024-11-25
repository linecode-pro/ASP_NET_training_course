using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace PromoCodeFactory.DataAccess.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        
        public DbSet<Role> Roles { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Preference> Preferences { get; set; }

        public DbSet<PromoCode> PromoCodes { get; set; }

        public DbSet<CustomerPreference> CustomerPreference { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Roles
            EntityTypeBuilder<Role> builderRole = modelBuilder.Entity<Role>();
            builderRole.ToTable("Roles");
            builderRole.Property(e => e.Name)
                   .HasMaxLength(50);
            builderRole.Property(e => e.Description)
                   .HasMaxLength(150);
            builderRole.HasIndex(e => e.Name)
                   .IsUnique();
            builderRole.HasKey(e => e.Id);



            // Employees
            EntityTypeBuilder<Employee> builderEmployee = modelBuilder.Entity<Employee>();
            builderEmployee.ToTable("Employees");
            builderEmployee.Property(e => e.FirstName)
                   .HasMaxLength(50)
                   .IsRequired();
            builderEmployee.Property(e => e.LastName)
                   .HasMaxLength(150)
                   .IsRequired();
            builderEmployee.Property(e => e.Email)
                   .HasMaxLength(60)
                   .IsRequired();



            // Customers
            EntityTypeBuilder<Customer> builderCustomer = modelBuilder.Entity<Customer>();
            builderCustomer.ToTable("Customers");
            builderCustomer.Property(c => c.FirstName)
                   .HasMaxLength(50)
                   .IsRequired();
            builderCustomer.Property(c => c.LastName)
                   .HasMaxLength(150);
            builderCustomer.Property(c => c.Email)
                   .HasMaxLength(60)
                   .IsRequired();



            // Preferences
            EntityTypeBuilder<Preference> builderPreference = modelBuilder.Entity<Preference>();
            builderPreference.ToTable("Preferences");
            builderPreference.Property(p => p.Name)
                   .HasMaxLength(50)
                   .IsRequired();
            builderPreference.HasIndex(p => p.Name)
                   .IsUnique();



            // PromoCodes
            EntityTypeBuilder<PromoCode> builderPromoCode = modelBuilder.Entity<PromoCode>();
            builderPromoCode.ToTable("PromoCodes");
            builderPromoCode.Property(p => p.Code)
                  .HasMaxLength(100)
                  .IsRequired();
            builderPromoCode.Property(p => p.ServiceInfo)
                  .HasMaxLength(100);

            // Настройка связи 'One-To-Many' к Customer
            builderPromoCode
                .HasOne(e => e.Customer)
                .WithMany(c => c.PromoCodes)
                .OnDelete(DeleteBehavior.Cascade);




            // CustomerPreferences
            EntityTypeBuilder<CustomerPreference> builderCustomerPreference = modelBuilder.Entity<CustomerPreference>();
            builderCustomerPreference.ToTable("CustomerPreferences");
            builderCustomerPreference.HasKey(e => new { e.CustomerId, e.PreferenceId });

            // Реализация связи 'Many-to-many': Customer <-> Preference
            builderCustomerPreference
                .HasOne(e => e.Customer)
                .WithMany(c => c.CustomerPreferences)
                .HasForeignKey(f => f.CustomerId);

            builderCustomerPreference
                .HasOne(e => e.Preference)
                .WithMany(c => c.CustomerPreferences)
                .HasForeignKey(f => f.PreferenceId);
        }
    }
}
