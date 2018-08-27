namespace OnlinePermissionSlips.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedSchoolID : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "SchoolID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "SchoolID", c => c.Int());
        }
    }
}
