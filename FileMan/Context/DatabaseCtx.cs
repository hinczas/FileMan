using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using MySql.Data.Entity;
using FileMan.Models;

namespace FileMan.Context
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class DatabaseCtx : DbContext
    {
        public DatabaseCtx()
            : base("MySQLConnection")
        {
            Database.SetInitializer<DatabaseCtx>(null);
        }

        public virtual DbSet<Folder> Folder { get; set; }
        public virtual DbSet<MasterFile> MasterFile { get; set; }
        public virtual DbSet<FileRevision> FileRevision { get; set; }


        public static DatabaseCtx Create()
        {
            return new DatabaseCtx();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Folder>()
                        .HasOptional(c => c.Parent)
                        .WithMany()
                        .HasForeignKey(c => c.Pid);
        }
    }
}