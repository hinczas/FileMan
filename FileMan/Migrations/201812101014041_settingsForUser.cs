namespace FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class settingsForUser : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "UserSettings",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ShowChangelog = c.Boolean(nullable: false),
                        ShowUncategorisedRoot = c.Boolean(nullable: false),
                        UncategorisedVisible = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)                ;
            
        }
        
        public override void Down()
        {
            DropTable("UserSettings");
        }
    }
}
