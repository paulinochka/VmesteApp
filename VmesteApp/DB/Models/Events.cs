using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VmesteApp.DB.Models
{
    public class Events
    {
        public int eventId { get; set; }
        public int familyId { get; set; }
        public int userId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public DateTime eventDate { get; set; }
        public DateTime eventTime { get; set; }
        public string category { get; set; }
        public bool isPrivate { get; set; }

    }
}
