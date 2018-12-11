namespace FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class linkSettings : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "UserSettingId", c => c.Long());
            CreateIndex("dbo.AspNetUsers", "UserSettingId");
            AddForeignKey("dbo.AspNetUsers", "UserSettingId", "dbo.UserSettings");            
        }
        
        public override void Down()
        {
        }
    }
}
