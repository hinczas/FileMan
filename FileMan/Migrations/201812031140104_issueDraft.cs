namespace FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class issueDraft : DbMigration
    {
        public override void Up()
        {
            AddColumn("FileRevision", "Draft", c => c.String(unicode: false));
            AddColumn("MasterFile", "Issue", c => c.Long());
        }
        
        public override void Down()
        {
            DropColumn("MasterFile", "Issue");
            DropColumn("FileRevision", "Draft");
        }
    }
}
