using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineQuranServer.Models
{
    public class SlotEx
    {
        public string timeSlot { get; set; }
        public string from_time{ get; set; }
        public string to_time { get; set; }

        public bool Mon { get; set; }
        public bool Tue { get; set; }
        public bool Wed { get; set; }
        public bool Thu { get; set; }
        public bool Fri { get; set; }
        public bool Sat { get; set; }
        public bool Sun { get; set; }
    }
}