namespace FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fileFolderMultiRel : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("Folder", "Folder_Id", "Folder");
            DropForeignKey("MasterFile", "FolderId", "Folder");
            DropIndex("MasterFile", new[] { "FolderId" });
            DropIndex("Folder", new[] { "Folder_Id" });
            CreateTable(
                "FolderMasterFiles",
                c => new
                    {
                        Folder_Id = c.Long(nullable: false),
                        MasterFile_Id = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.Folder_Id, t.MasterFile_Id })                
                .ForeignKey("Folder", t => t.Folder_Id, cascadeDelete: true)
                .ForeignKey("MasterFile", t => t.MasterFile_Id, cascadeDelete: true)
                .Index(t => t.Folder_Id)
                .Index(t => t.MasterFile_Id);
            
            DropColumn("MasterFile", "FolderId");
            DropColumn("Folder", "Folder_Id");
        }
        
        public override void Down()
        {
            AddColumn("Folder", "Folder_Id", c => c.Long());
            AddColumn("MasterFile", "FolderId", c => c.Long());
            DropForeignKey("FolderMasterFiles", "MasterFile_Id", "MasterFile");
            DropForeignKey("FolderMasterFiles", "Folder_Id", "Folder");
            DropIndex("FolderMasterFiles", new[] { "MasterFile_Id" });
            DropIndex("FolderMasterFiles", new[] { "Folder_Id" });
            DropTable("FolderMasterFiles");
            CreateIndex("Folder", "Folder_Id");
            CreateIndex("MasterFile", "FolderId");
            AddForeignKey("MasterFile", "FolderId", "Folder", "Id");
            AddForeignKey("Folder", "Folder_Id", "Folder", "Id");
        }
    }
}
