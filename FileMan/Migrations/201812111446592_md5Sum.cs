namespace FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class md5Sum : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FileRevision", "Md5hash", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FileRevision", "Md5hash");
        }
    }
}
