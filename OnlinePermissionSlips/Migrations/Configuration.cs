namespace OnlinePermissionSlips.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;
	using OnlinePermissionSlips.Models;
	using Microsoft.AspNet.Identity.EntityFramework;

	internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = false;
		}

		protected override void Seed(ApplicationDbContext context)
		{
			//  This method will be called after migrating to the latest version.

			//  You can use the DbSet<T>.AddOrUpdate() helper extension method 
			//  to avoid creating duplicate seed data.

			context.Roles.AddOrUpdate(new IdentityRole("SystemAdmin"));
			context.Roles.AddOrUpdate(new IdentityRole("SchoolAdmin"));
			context.Roles.AddOrUpdate(new IdentityRole("Teacher"));
			context.Roles.AddOrUpdate(new IdentityRole("Guardian"));
		}
	}
}
