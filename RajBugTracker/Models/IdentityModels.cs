﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using RajBugTracker.Models.Classes;

namespace RajBugTracker.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
       
        public string DisplayName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Name { get; set; }
        [InverseProperty("Creator")]
        public virtual ICollection<Ticket> CreatedTickets { get; set; }
        [InverseProperty("AssignedUser")]
        public virtual ICollection<Ticket> AssignedTickets { get; set; }

        public ApplicationUser()
        {
            Projects = new HashSet<Project>();
        }

        public virtual ICollection<Project> Projects { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Project> Projects { get; set; }

        public System.Data.Entity.DbSet<RajBugTracker.Models.Classes.Ticket> Tickets { get; set; }

        public System.Data.Entity.DbSet<RajBugTracker.Models.Classes.TicketStatus> TicketStatuses { get; set; }

        public System.Data.Entity.DbSet<RajBugTracker.Models.Classes.TicketPriorty> TicketPriorties { get; set; }

        public System.Data.Entity.DbSet<RajBugTracker.Models.Classes.TicketType> TicketTypes { get; set; }
        
    }
}