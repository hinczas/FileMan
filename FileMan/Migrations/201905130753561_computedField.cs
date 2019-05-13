namespace Raf.FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class computedField : DbMigration
    {
        public override void Up()
        {
            //AddColumn("dbo.MasterFile", "Locked", c => c.Boolean(nullable: false));
            Sql("ALTER TABLE dbo.MasterFile ADD Locked AS CAST(CASE WHEN[UserLock] is null OR LEN([UserLock]) < 1 THEN 0 ELSE 1 END AS BIT)");
        }
        
        public override void Down()
        {
            DropColumn("dbo.MasterFile", "Locked");
        }
    }
}
