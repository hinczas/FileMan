namespace FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class revisionType : DbMigration
    {
        public override void Up()
        {
            AddColumn("FileRevision", "Type", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("FileRevision", "Type");
        }
    }
}
