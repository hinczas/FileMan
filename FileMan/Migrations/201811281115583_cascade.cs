namespace FileMan.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cascade : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("Item", "Pid", "Item");
            AddForeignKey("Item", "Pid", "Item", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("Item", "Pid", "Item");
            AddForeignKey("Item", "Pid", "Item", "Id");
        }
    }
}
