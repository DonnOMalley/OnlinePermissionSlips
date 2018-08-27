namespace OnlinePermissionSlips.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SchoolID : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "SchoolID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "SchoolID");
        }
    }
}
