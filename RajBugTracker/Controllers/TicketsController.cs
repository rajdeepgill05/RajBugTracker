using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using RajBugTracker.Helper;
using RajBugTracker.Models;
using RajBugTracker.Models.Classes;

namespace RajBugTracker.Controllers
{
    [Authorize]

    public class TicketsController : Controller
    {
        private ApplicationDbContext db { get; set; }
        private UserRoleHelper UserRoleHelper { get; set; }
        public TicketsController()
        {
            db = new ApplicationDbContext();
            UserRoleHelper = new UserRoleHelper();
        }

        // GET: Tickets
        public ActionResult Index()
            {
                var tickets = db.Tickets.Include(t => t.AssignedUser).Include(t => t.Creator).Include(t => t.TicketPriorty).Include(t => t.TicketType).Include(t => t.TicketStatus);
                return View(tickets.ToList());
            }

        //Get Tickets Assigned To the User
        public ActionResult AssignedTickets()
        {
            string userID = User.Identity.GetUserId();
            if (User.IsInRole("Submitter"))
            {
                var tickets = db.Tickets.Where(t => t.CreatorId == userID).Include(t => t.Creator).Include(t => t.AssignedUser).Include(t => t.Project);
                return View("Index", tickets.ToList());
            }
            if (User.IsInRole("Developer"))
            {
                var tickets = db.Tickets.Where(t => t.AssignedUserId == userID).Include(t => t.Creator).Include(t => t.AssignedUser).Include(t => t.Project).Include(t => t.Comments);
                return View("Index", tickets.ToList());
            }
            if (User.IsInRole("Project Manager"))
            {
                return View(db.Tickets.Include(t => t.TicketPriorty).Include(t => t.Project).Include(t => t.Comments).Include(t => t.TicketStatus).Include(t => t.TicketType).Where(p => p.AssignedUserId == userID).ToList());
            }


            return View("Index");
        }
        // Project Manger and Developer Tickets
        [Authorize(Roles = "Project Manager,Developer")]
        public ActionResult ProjectManagerOrDeveloperTickets()
        {
            string userId = User.Identity.GetUserId();
            var ProjectMangerOrDeveloperId = db.Users.Where(p => p.Id == userId).FirstOrDefault();
            var ProjectId = ProjectMangerOrDeveloperId.Projects.Select(p => p.Id).FirstOrDefault();
            var tickets = db.Tickets.Where(p => p.Id == ProjectId).ToList();
            return View("Index", tickets);
        }
        // Assign Tickets to the Users
        public ActionResult AssignUsers(int id)
        {
            var model = new ProjectAssignViewModel();

            model.Id = id;

            var tickets = db.Tickets.FirstOrDefault(p => p.Id == id);
            var users = db.Users.ToList();
            var userIdsAssignedToTicket = tickets.Users
                .Select(p => p.Id).ToList();

            model.UserList = new MultiSelectList(users, "Id", "Username", userIdsAssignedToTicket);

            return View(model);

        }
        [HttpPost]
        public ActionResult AssignUsers(ProjectAssignViewModel model)
        {

            //Step 1: Find the Project
            var ticket = db.Tickets.FirstOrDefault(p => p.Id == model.Id);



            //Step 2: Remove all assigned users from this project
            var assignedUsers = ticket.Users.ToList();

            foreach (var user in assignedUsers)
            {
                ticket.Users.Remove(user);
            }

            //Step3: Assign Users to the project
            if (model.SelectedUsers != null)
            {
                foreach (var userId in model.SelectedUsers)
                {
                    var user = db.Users.FirstOrDefault(p => p.Id == userId);

                    ticket.Users.Add(user);
                }

            }

            //Step4 : Save Changes to the Database
            db.SaveChanges();

            return RedirectToAction("Index");

        }

        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // GET: Tickets/Create
        [Authorize(Roles = "Submitter, Admin")]
        public ActionResult Create()
        {
            ViewBag.AssignedUserId = new SelectList(db.Users, "Id", "DisplayName");
            ViewBag.CreatorId = new SelectList(db.Users, "Id", "DisplayName");
           // ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
            ViewBag.TicketPriortyId = new SelectList(db.TicketPriorties, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Submitter, Admin")]

        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,TicketTypeId,TicketPriortyId,ProjectId")] Ticket ticket, HttpPostedFileBase File)
        {
            if (ModelState.IsValid)
            {
                ticket.CreatorId = User.Identity.GetUserId();
                ticket.TicketStatusId = 1;
                db.Tickets.Add(ticket);
                var tickets = db.Tickets
                    .Where(p => p.Id == ticket.Id)
                    .FirstOrDefault();
                var userId = User.Identity.GetUserId();
                var projects = db.Tickets.Where(p => p.Id == ticket.Id).ToList();

                var att = new  TicketAttachment();
                if (!FileUploadValidationHelper.IsWebFriendlyImage(File))
                {
                    ViewBag.ErrorMessage = "Please upload an image";
                }
                if (File == null)
                {
                    return HttpNotFound();
                }

                if ((User.IsInRole("Admin")) || (User.IsInRole("Project Manager") && projects.Any(p => p.Id = User.Identity.GetUserId())) ||
                    (User.IsInRole("Submitter") && ticket.CreatorId == userId) ||
                    (User.IsInRole("Developer") && ticket.AssignedUserId == userId))
                {
                    var fileName = Path.GetFileName(File.FileName);
                    File.SaveAs(Path.Combine(Server.MapPath("~/Helper/Uploads/"), fileName));
                    att.FileUrl = "~/Helper/Uploads/" + fileName;
                    att.TicketId = ticket.Id;
                    att.Created = DateTime.Now;
                    att.UserId = User.Identity.GetUserId();
                    db.TicketAttachments.Add(att);
                    db.SaveChanges();
                }

                else if (User.Identity.IsAuthenticated)
                {
                    ViewBag.ErrorMessage = "Only Authorized Users are Allowed TO create Comments";
                    return View("Details", ticket);
                }

                return RedirectToAction("Index");
            }

            //ViewBag.AssignedUserId = new SelectList(db.Users, "Id", "DisplayName");
            //ViewBag.CreatorId = new SelectList(db.Users, "Id", "DisplayName");
            //ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
            ViewBag.TicketPriortyId = new SelectList(db.TicketPriorties, "Id", "Name", ticket.TicketPriortyId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);

            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            ViewBag.AssignedUserId = new SelectList(db.Users, "Id", "Name", ticket.AssignedUserId);
            ViewBag.CreatorId = new SelectList(db.Users, "Id", "Name", ticket.CreatorId);
            ViewBag.TicketPriortyId = new SelectList(db.TicketPriorties, "Id", "Name", ticket.TicketPriortyId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,TicketTypeId,TicketPriortyId,ProjectId")] Ticket tickets)
        {
            if (ModelState.IsValid)
            {
                var changeDate = DateTimeOffset.Now;
                var changes = new List<TicketHistory>();
                var ticket = db.Tickets.First(p => p.Id == tickets.Id);
                ticket.Name = tickets.Name;
                ticket.Description = tickets.Description;
                ticket.TicketTypeId = tickets.TicketTypeId;
                ticket.Updated = changeDate;
                var originalValues = db.Entry(ticket).OriginalValues;
                var currentValues = db.Entry(ticket).CurrentValues;
                foreach (var property in originalValues.PropertyNames)
                {
                    var originalValue = originalValues[property]?.ToString();
                    var currentValue = currentValues[property]?.ToString();
                    if (originalValue != currentValue)
                    {
                        var history = new TicketHistory();
                        history.Changed = changeDate;
                        history.NewValue = currentValue;
                        history.OldValue = originalValue;
                        history.Property = property;
                        history.TicketId = ticket.Id;
                        history.UserId = User.Identity.GetUserId();
                        changes.Add(history);
                    }
                }
                db.TicketHistories.AddRange(changes);

                //
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AssignedUserId = new SelectList(db.Users, "Id", "DisplayName");
            ViewBag.CreatorId = new SelectList(db.Users, "Id", "DisplayName");
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
            ViewBag.TicketPriortyId = new SelectList(db.TicketPriorties, "Id", "Name", tickets.TicketPriortyId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", tickets.TicketTypeId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", tickets.ProjectId);

            return View(tickets);
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
            db.SaveChanges();
            return RedirectToAction("Index");


        }
        //Create Comments
        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager,Submitter,Developer")]
        
        public ActionResult CreateComment(int? id, string body)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var ticket = db.Tickets
                .Where(p => p.Id == id.Value)
                .FirstOrDefault();
            var userId = User.Identity.GetUserId();
            var projects = db.Tickets.Where(p => p.Id == ticket.Id).ToList();


            if (ticket == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                TempData["ErrorMessage"] = "Comment is required";
                return RedirectToAction("Details", new { id = id.Value });
            }

            if ((User.IsInRole("Admin")) || (User.IsInRole("Project Manager") && projects.Any(p => p.Id == id)) ||
                (User.IsInRole("Submitter") && ticket.CreatorId == userId) ||
                (User.IsInRole("Developer") && ticket.AssignedUserId == userId))
            {
                var comment = new TicketComment();
                comment.UserId = User.Identity.GetUserId();
                comment.TicketId = ticket.Id;
                comment.Created = DateTime.Now;
                comment.Comment = body;
                db.TicketComments.Add(comment);
                db.SaveChanges();

            }
            else if (User.Identity.IsAuthenticated)
            {
                ViewBag.ErrorMessage = "Only Authorized Users are Allowed TO create Comments";
                return View("Details", ticket);
            }



            return RedirectToAction("Details", new { id = id.Value });
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
