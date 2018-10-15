using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
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
                var tickets = db.Tickets.Where(t => t.AssignedUserId == userID).Include(t => t.Creator).Include(t => t.AssignedUser).Include(t => t.Project);
                return View("Index", tickets.ToList());
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
        public ActionResult Create([Bind(Include = "Id,Name,Description,TicketTypeId,TicketPriortyId,ProjectId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.CreatorId = User.Identity.GetUserId();
                ticket.TicketStatusId = 1;
                db.Tickets.Add(ticket);
                db.SaveChanges();
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
        public ActionResult Edit([Bind(Include = "Id,Name,Description,TicketTypeId,TicketPriortyId,ProjectId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                var Ticket = db.Tickets.FirstOrDefault(p => p.Id == ticket.Id);
                Ticket.Name = ticket.Name;
                Ticket.Description = ticket.Description;
                Ticket.Updated = DateTime.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AssignedUserId = new SelectList(db.Users, "Id", "DisplayName");
            ViewBag.CreatorId = new SelectList(db.Users, "Id", "DisplayName");
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
            ViewBag.TicketPriortyId = new SelectList(db.TicketPriorties, "Id", "Name", ticket.TicketPriortyId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);

            return View(ticket);
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
