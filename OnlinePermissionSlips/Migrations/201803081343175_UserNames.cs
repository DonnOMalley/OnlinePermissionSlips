namespace OnlinePermissionSlips.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserNames : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "FirstName", c => c.String());
            AddColumn("dbo.AspNetUsers", "MiddleName", c => c.String());
            AddColumn("dbo.AspNetUsers", "LastName", c => c.String());
            AlterColumn("dbo.AspNetUsers", "SchoolID", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "SchoolID", c => c.Int(nullable: false));
            DropColumn("dbo.AspNetUsers", "LastName");
            DropColumn("dbo.AspNetUsers", "MiddleName");
            DropColumn("dbo.AspNetUsers", "FirstName");
        }
    }
}
