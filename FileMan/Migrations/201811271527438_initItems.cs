namespace FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initItems : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Item",
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
                        Item_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Item", t => t.Item_Id)
                .ForeignKey("dbo.Item", t => t.Pid)
                .Index(t => t.Pid)
                .Index(t => t.Item_Id);
            
        }
        
        public override void Down()
        {                      
            DropForeignKey("dbo.Item", "Pid", "dbo.Item");
            DropForeignKey("dbo.Item", "Item_Id", "dbo.Item");
            DropIndex("dbo.Item", new[] { "Item_Id" });
            DropIndex("dbo.Item", new[] { "Pid" });
            DropTable("dbo.Item");            
        }
    }
}
