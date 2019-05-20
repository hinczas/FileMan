namespace Raf.FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class navUrl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NavigationHistory", "Url", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.NavigationHistory", "Url");
        }
    }
}
