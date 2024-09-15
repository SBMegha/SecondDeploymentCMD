﻿using System;
using Microsoft.EntityFrameworkCore;
using ConnectMyDoc_Domain_Layer.Entity;

namespace ConnectMyDoc_Data_Layer.Context
{
    public class PatientCMDDbContext : DbContext
    {
        public PatientCMDDbContext() { }
        public PatientCMDDbContext(DbContextOptions<PatientCMDDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=(localdb)\\mssqllocaldb;Initial Catalog=PatientCMDDb;Integrated Security=True;Encrypt=True");
            }
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<PatientAddress> PatientAddresses { get; set; }
        public DbSet<PatientGuardian> PatientGuardians { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Allow multiple patients to share the same guardian
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.PatientGuardian)
                .WithMany()  // Allows multiple patients to reference the same guardian
                .HasForeignKey(p => p.PatientGuardianId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);

            // Seed PatientAddress data
            modelBuilder.Entity<PatientAddress>().HasData(
                new PatientAddress
                {
                    PatientAddressId = 1,
                    StreetAddress = "123 Main St",
                    City = "New York",
                    State = "NY",
                    Country = "USA",
                    ZipCode = "10001",
                    CreatedDate = DateTime.Now,
                    CreatedBy = 1
                },
                new PatientAddress
                {
                    PatientAddressId = 2,
                    StreetAddress = "456 Second St",
                    City = "Los Angeles",
                    State = "CA",
                    Country = "USA",
                    ZipCode = "90001",
                    CreatedDate = DateTime.Now,
                    CreatedBy = 1
                }
            );

            // Seed PatientGuardian data
            modelBuilder.Entity<PatientGuardian>().HasData(
                new PatientGuardian
                {
                    PatientGuardianId = 1,
                    PatientGuardianName = "Jane Doe",
                    PatientGuardianPhoneNumber = "9876543210",
                    PatientGuardianRelationship = "Mother"
                },
                new PatientGuardian
                {
                    PatientGuardianId = 2,
                    PatientGuardianName = "Sam Smith",
                    PatientGuardianPhoneNumber = "9876543211",
                    PatientGuardianRelationship = "Father"
                }
            );

            // Seed Patient data
            modelBuilder.Entity<Patient>().HasData(
                new Patient
                {
                    PatientId = 1,
                    PatientName = "John Doe",
                    Email = "john.doe@example.com",
                    Phone = "1234567890",
                    Age = 25,
                    Dob = new DateTime(1998, 1, 1),
                    Gender = "Male",
                    PreferredStartTime = DateTime.Today.AddHours(9),
                    PreferredEndTime = DateTime.Today.AddHours(17),
                    CreatedDate = DateTime.Now,
                    CreatedBy = 1,
                    LastModifiedDate = DateTime.Now,
                    LastModifiedBy = 1,
                    PatientAddressId = 1,
                    PreferredDoctorId = 1,
                    PatientGuardianId = 1  // This patient has Jane Doe as the guardian
                },
                new Patient
                {
                    PatientId = 2,
                    PatientName = "Emily Clark",
                    Email = "emily.clark@example.com",
                    Phone = "1234567891",
                    Age = 30,
                    Dob = new DateTime(1993, 5, 15),
                    Gender = "Female",
                    PreferredStartTime = DateTime.Today.AddHours(8),
                    PreferredEndTime = DateTime.Today.AddHours(16),
                    CreatedDate = DateTime.Now,
                    CreatedBy = 2,
                    LastModifiedDate = DateTime.Now,
                    LastModifiedBy = 2,
                    PatientAddressId = 2,
                    PreferredDoctorId = 2,
                    PatientGuardianId = 2  // This patient has Sam Smith as the guardian
                }
            );

            base.OnModelCreating(modelBuilder);
        }



    }
}
