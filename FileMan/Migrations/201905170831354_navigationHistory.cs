namespace Raf.FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class navigationHistory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NavigationHistory",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Action = c.String(),
                        Controller = c.String(),
                        ItemId = c.Long(),
                        ItemIdStr = c.String(),
                        Search = c.String(),
                        Scope = c.Int(),
                        ParentId = c.Long(),
                        ParentIdStr = c.String(),
                        JSFunction = c.String(),
                        ActionDate = c.DateTime(nullable: false),
                        UserId = c.String(nullable: false),
                        SessionId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.NavigationHistory");
        }
    }
}
