namespace Raf.FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixedFavs : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ApplicationUserFavourites", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.ApplicationUserFavourites", "Favourite_Id", "dbo.Favourite");
            DropIndex("dbo.ApplicationUserFavourites", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.ApplicationUserFavourites", new[] { "Favourite_Id" });
            AddColumn("dbo.Favourite", "UserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Favourite", "UserId");
            AddForeignKey("dbo.Favourite", "UserId", "dbo.AspNetUsers", "Id");
            DropTable("dbo.ApplicationUserFavourites");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ApplicationUserFavourites",
                c => new
                    {
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                        Favourite_Id = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.ApplicationUser_Id, t.Favourite_Id });
            
            DropForeignKey("dbo.Favourite", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Favourite", new[] { "UserId" });
            DropColumn("dbo.Favourite", "UserId");
            CreateIndex("dbo.ApplicationUserFavourites", "Favourite_Id");
            CreateIndex("dbo.ApplicationUserFavourites", "ApplicationUser_Id");
            AddForeignKey("dbo.ApplicationUserFavourites", "Favourite_Id", "dbo.Favourite", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ApplicationUserFavourites", "ApplicationUser_Id", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
