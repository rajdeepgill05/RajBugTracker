using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RajBugTracker.Models.Classes
{
    public class TicketNotification
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string UserName { get; set; }
        public string SentToId { get; set; }
        public string SentFromId { get; set; }
        public DateTimeOffset SendDate { get; set; }
        public string HasBeenSent { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }

    }
}