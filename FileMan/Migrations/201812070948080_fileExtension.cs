namespace FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fileExtension : DbMigration
    {
        public override void Up()
        {
            AddColumn("FileRevision", "Extension", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("FileRevision", "Extension");
        }
    }
}
