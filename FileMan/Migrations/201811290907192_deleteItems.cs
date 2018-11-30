namespace FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class deleteItems : DbMigration
    {
        public override void Up()
        {
            DropTable("Item");
        }
        
        public override void Down()
        {
            CreateTable(
                "Item",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Pid = c.Long(),
                        Name = c.String(unicode: false),
                        Path = c.String(unicode: false),
                        FullPath = c.String(unicode: false),
                        Type = c.String(unicode: false),
                        Extension = c.String(unicode: false),
                        Description = c.String(unicode: false),
                        Comment = c.String(unicode: false),
                        Changelog = c.String(unicode: false),
                        Added = c.DateTime(precision: 0),
                        Edited = c.DateTime(precision: 0),
                        Parent_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id)                ;
            
            CreateIndex("Item", "Parent_Id");
            AddForeignKey("Item", "Parent_Id", "Item", "Id");
        }
    }
}
