namespace Raf.FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class checkOut : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MasterFile", "UserLock", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.MasterFile", "UserLock");
        }
    }
}
