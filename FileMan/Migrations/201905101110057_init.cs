namespace Raf.FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FileRevision",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        MasterFileId = c.Long(),
                        Revision = c.Long(nullable: false),
                        Name = c.String(),
                        FullPath = c.String(),
                        Comment = c.String(),
                        Extension = c.String(),
                        Added = c.DateTime(),
                        Draft = c.String(),
                        Type = c.String(),
                        Icon = c.String(),
                        Md5hash = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MasterFile", t => t.MasterFileId)
                .Index(t => t.MasterFileId);
            
            CreateTable(
                "dbo.MasterFile",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Number = c.String(nullable: false),
                        Name = c.String(),
                        Path = c.String(),
                        Extension = c.String(),
                        Description = c.String(),
                        Comment = c.String(),
                        Changelog = c.String(),
                        Added = c.DateTime(),
                        Edited = c.DateTime(),
                        Issue = c.Long(),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: false)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Folder",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Pid = c.Long(),
                        Name = c.String(),
                        Path = c.String(),
                        Type = c.String(),
                        Description = c.String(),
                        Comment = c.String(),
                        Changelog = c.String(),
                        Added = c.DateTime(),
                        Edited = c.DateTime(),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Folder", t => t.Pid)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: false)
                .Index(t => t.Pid)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserSettingId = c.Long(nullable: false),
                        JoinDate = c.DateTime(nullable: false),
                        FirstName = c.String(),
                        Surname = c.String(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserSettings", t => t.UserSettingId, cascadeDelete: true)
                .Index(t => t.UserSettingId)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.UserSettings",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ShowChangelog = c.Boolean(nullable: false),
                        ShowUncategorisedRoot = c.Boolean(nullable: false),
                        UncategorisedVisible = c.Boolean(nullable: false),
                        TreeSearch = c.Boolean(nullable: false),
                        TreeSort = c.Boolean(nullable: false),
                        TreeDnD = c.Boolean(nullable: false),
                        TreeContext = c.Boolean(nullable: false),
                        ForceDelete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.FolderMasterFiles",
                c => new
                    {
                        Folder_Id = c.Long(nullable: false),
                        MasterFile_Id = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.Folder_Id, t.MasterFile_Id })
                .ForeignKey("dbo.Folder", t => t.Folder_Id, cascadeDelete: true)
                .ForeignKey("dbo.MasterFile", t => t.MasterFile_Id, cascadeDelete: true)
                .Index(t => t.Folder_Id)
                .Index(t => t.MasterFile_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.FileRevision", "MasterFileId", "dbo.MasterFile");
            DropForeignKey("dbo.MasterFile", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Folder", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "UserSettingId", "dbo.UserSettings");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Folder", "Pid", "dbo.Folder");
            DropForeignKey("dbo.FolderMasterFiles", "MasterFile_Id", "dbo.MasterFile");
            DropForeignKey("dbo.FolderMasterFiles", "Folder_Id", "dbo.Folder");
            DropIndex("dbo.FolderMasterFiles", new[] { "MasterFile_Id" });
            DropIndex("dbo.FolderMasterFiles", new[] { "Folder_Id" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUsers", new[] { "UserSettingId" });
            DropIndex("dbo.Folder", new[] { "UserId" });
            DropIndex("dbo.Folder", new[] { "Pid" });
            DropIndex("dbo.MasterFile", new[] { "UserId" });
            DropIndex("dbo.FileRevision", new[] { "MasterFileId" });
            DropTable("dbo.FolderMasterFiles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.UserSettings");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Folder");
            DropTable("dbo.MasterFile");
            DropTable("dbo.FileRevision");
        }
    }
}
