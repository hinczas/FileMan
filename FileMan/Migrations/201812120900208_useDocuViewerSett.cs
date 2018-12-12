namespace FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class useDocuViewerSett : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserSettings", "UseDocuViewer", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserSettings", "UseDocuViewer");
        }
    }
}
