namespace FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newModels : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "FileRevision",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        MasterFileId = c.Long(),
                        Revision = c.Long(nullable: false),
                        Name = c.String(unicode: false),
                        FullPath = c.String(unicode: false),
                        Added = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.Id)                
                .ForeignKey("MasterFile", t => t.MasterFileId)
                .Index(t => t.MasterFileId);
            
            CreateTable(
                "MasterFile",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        FolderId = c.Long(),
                        Number = c.String(nullable: false, unicode: false),
                        Name = c.String(unicode: false),
                        Path = c.String(unicode: false),
                        Extension = c.String(unicode: false),
                        Description = c.String(unicode: false),
                        Comment = c.String(unicode: false),
                        Changelog = c.String(unicode: false),
                        Added = c.DateTime(precision: 0),
                        Edited = c.DateTime(precision: 0),
                    })
                .PrimaryKey(t => t.Id)                
                .ForeignKey("Folder", t => t.FolderId)
                .Index(t => t.FolderId);
            
            CreateTable(
                "Folder",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Pid = c.Long(),
                        Name = c.String(unicode: false),
                        Path = c.String(unicode: false),
                        Type = c.String(unicode: false),
                        Description = c.String(unicode: false),
                        Comment = c.String(unicode: false),
                        Changelog = c.String(unicode: false),
                        Added = c.DateTime(precision: 0),
                        Edited = c.DateTime(precision: 0),
                        Folder_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id)                
                .ForeignKey("Folder", t => t.Folder_Id)
                .ForeignKey("Folder", t => t.Pid)
                .Index(t => t.Pid)
                .Index(t => t.Folder_Id);            
        }
        
        public override void Down()
        {
            DropForeignKey("Item", "Parent_Id", "Item");
            DropForeignKey("FileRevision", "MasterFileId", "MasterFile");
            DropForeignKey("MasterFile", "FolderId", "Folder");
            DropForeignKey("Folder", "Pid", "Folder");
            DropForeignKey("Folder", "Folder_Id", "Folder");
            DropIndex("Folder", new[] { "Folder_Id" });
            DropIndex("Folder", new[] { "Pid" });
            DropIndex("MasterFile", new[] { "FolderId" });
            DropIndex("FileRevision", new[] { "MasterFileId" });
            DropTable("Folder");
            DropTable("MasterFile");
            DropTable("FileRevision");
            RenameIndex(table: "dbo.Item", name: "IX_Parent_Id", newName: "IX_Item_Id");
            RenameColumn(table: "Item", name: "Parent_Id", newName: "Item_Id");
            CreateIndex("Item", "Pid");
            AddForeignKey("Item", "Pid", "Item", "Id", cascadeDelete: true);
        }
    }
}
