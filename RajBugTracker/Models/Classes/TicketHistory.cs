﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RajBugTracker.Models.Classes
{
    public class TicketHistory
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        public string Property { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public DateTimeOffset Changed { get; set; }

    }
}