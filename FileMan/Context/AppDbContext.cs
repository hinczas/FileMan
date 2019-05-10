using FileMan.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace FileMan.Context
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public virtual DbSet<Folder> Folder { get; set; }
        public virtual DbSet<MasterFile> MasterFile { get; set; }
        public virtual DbSet<FileRevision> FileRevision { get; set; }
        public virtual DbSet<UserSetting> UserSetting { get; set; }

        public static AppDbContext Create()
        {
            return new AppDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Folder>()
                        .HasOptional(c => c.Parent)
                        .WithMany(r => r.Children)
                        .HasForeignKey(c => c.Pid);

            modelBuilder.Entity<Folder>()
                        .HasRequired(c => c.User)
                        .WithMany(t => t.Categories)
                        .Map(m => m.MapKey("UserId"));

            modelBuilder.Entity<MasterFile>()
                        .HasRequired(c => c.User)
                        .WithMany(t => t.Documents)
                        .Map(m => m.MapKey("UserId"));

            base.OnModelCreating(modelBuilder);

        }

    }
}