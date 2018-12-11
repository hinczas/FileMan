namespace FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fileIcon : DbMigration
    {
        public override void Up()
        {
            AddColumn("FileRevision", "Icon", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("FileRevision", "Icon");
        }
    }
}
