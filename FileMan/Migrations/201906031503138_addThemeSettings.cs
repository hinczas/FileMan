namespace Raf.FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addThemeSettings : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserSettings", "Theme", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserSettings", "Theme");
        }
    }
}
