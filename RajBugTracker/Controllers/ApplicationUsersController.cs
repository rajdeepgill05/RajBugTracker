﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RajBugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using RajBugTracker.Helper;

namespace RajBugTracker.Controllers
{
    [Authorize]
    public class ApplicationUsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ApplicationUsers
        public ActionResult Index()
        {
            var users = db.Users.Select(user => new UserListViewModel
            {
                Id = user.Id,
                Name = user.UserName,
                Email = user.Email,
                RoleName = db.Roles.FirstOrDefault(role => role.Id == user.Roles.FirstOrDefault().RoleId).Name
            }).ToList();

            return View(users);
        }


        public ActionResult ChangeRole(string id)
        {
            var model = new UserRoleViewModel();
            var userRoleHelper = new UserRoleHelper();

            model.Id = id;
            model.Name = db.Users.FirstOrDefault(p => p.Id == id).UserName;

            var roles = userRoleHelper.GetAllRoles();
            var userRoles = userRoleHelper.GetUserRoles(id);

            model.Roles = new MultiSelectList(roles, "Name", "UserName", userRoles);
            
            return View(model);
        }

        [HttpPost]
        public ActionResult ChangeRole(UserRoleViewModel model)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            //STEP 1: Find the user
            var user = userManager.FindById(model.Id);

            //STEP 2: Get UserRoles:
            var userRoles = userManager.GetRoles(user.Id);

            //STEP 3: Remove the roles from the user
            foreach (var role in userRoles)
            {
                userManager.RemoveFromRole(user.Id, role);
            }

            //STEP 4: Add roles to the user
            foreach (var role in model.SelectedRoles)
            {
                userManager.AddToRole(user.Id, role);
            };

            return RedirectToAction("Index");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
