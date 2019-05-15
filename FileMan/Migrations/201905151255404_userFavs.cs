namespace Raf.FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userFavs : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.FolderMasterFiles", newName: "MasterFileFolders");
            DropPrimaryKey("dbo.MasterFileFolders");
            CreateTable(
                "dbo.Favourite",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ItemType = c.Int(nullable: false),
                        ItemId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ApplicationUserFavourites",
                c => new
                    {
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                        Favourite_Id = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.ApplicationUser_Id, t.Favourite_Id })
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id, cascadeDelete: true)
                .ForeignKey("dbo.Favourite", t => t.Favourite_Id, cascadeDelete: true)
                .Index(t => t.ApplicationUser_Id)
                .Index(t => t.Favourite_Id);
            
            AddPrimaryKey("dbo.MasterFileFolders", new[] { "MasterFile_Id", "Folder_Id" });
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ApplicationUserFavourites", "Favourite_Id", "dbo.Favourite");
            DropForeignKey("dbo.ApplicationUserFavourites", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.ApplicationUserFavourites", new[] { "Favourite_Id" });
            DropIndex("dbo.ApplicationUserFavourites", new[] { "ApplicationUser_Id" });
            DropPrimaryKey("dbo.MasterFileFolders");
            DropTable("dbo.ApplicationUserFavourites");
            DropTable("dbo.Favourite");
            AddPrimaryKey("dbo.MasterFileFolders", new[] { "Folder_Id", "MasterFile_Id" });
            RenameTable(name: "dbo.MasterFileFolders", newName: "FolderMasterFiles");
        }
    }
}
