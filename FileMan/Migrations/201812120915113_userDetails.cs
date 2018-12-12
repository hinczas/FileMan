namespace FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userDetails : DbMigration
    {
        public override void Up()
        {


            AddColumn("dbo.AspNetUsers", "JoinDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.AspNetUsers", "FirstName", c => c.String(unicode: false));
            AddColumn("dbo.AspNetUsers", "Surname", c => c.String(unicode: false));
            
        }
        
        public override void Down()
        {
           
        }
    }
}
