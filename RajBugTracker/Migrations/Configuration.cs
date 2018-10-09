namespace RajBugTracker.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using RajBugTracker.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<RajBugTracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(RajBugTracker.Models.ApplicationDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            if (!(context.Roles.Any(p => p.Name == "Admin")))
            {
                roleManager.Create(new IdentityRole("Admin"));
            }
            if (!context.Roles.Any(p => p.Name == "Project Manager"))
            {
                roleManager.Create(new IdentityRole("Project Manager"));
            }
            if (!context.Roles.Any(p => p.Name == "Developer"))
            {
                roleManager.Create(new IdentityRole("Developer"));
            }
            if (!context.Roles.Any(p => p.Name == "Submiter"))
            {
                roleManager.Create(new IdentityRole("Submiter"));
            }

            ApplicationUser adminUser;

            if (!context.Users.Any(p => p.UserName == "admin@bugtracker.com"))
            {
                adminUser = new ApplicationUser();
                adminUser.Email = "admin@bugtracker.com";
                adminUser.UserName = adminUser.Email;
                userManager.Create(adminUser, "P@ssword!");
            }
            else
            {
                adminUser = context.Users.First(p => p.UserName == "admin@bugtracker.com");
            }
            if (!userManager.IsInRole(adminUser.Id, "admin"))
            {
                userManager.AddToRole(adminUser.Id, "admin");
            }

        }
    }
}
