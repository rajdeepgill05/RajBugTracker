using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace RajBugTracker.Models.Classes
{
    public class Ticket
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? Updated { get; set; }

        public int ProjectId { get; set; }

        public virtual Project Project { get; set; }

        public int TicketTypeId { get; set; }

        public virtual TicketType TicketType { get; set; }

        public int TicketPriortyId { get; set; }

        public virtual TicketPriorty TicketPriorty { get; set; }

        public string CreatorId { get; set; }

        public virtual ApplicationUser Creator { get; set; }

        public int TicketStatusId { get; set; }

        public virtual TicketStatus TicketStatus { get; set; }

        public string AssignedUserId { get; set; }

        public virtual ApplicationUser AssignedUser { get; set; }

        public Ticket()
        {
            Users = new HashSet<ApplicationUser>();

            this.Created = DateTime.Now;
            Comments = new HashSet<TicketComment>();
            Attachments = new HashSet<TicketAttachment>();
            Histories = new HashSet<TicketHistory>();
        }
        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<TicketComment> Comments { get; set; }
        public virtual ICollection<TicketAttachment> Attachments { get; set; }
        public virtual ICollection<TicketHistory> Histories { get; set; }



    }
}