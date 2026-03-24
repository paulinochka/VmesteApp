using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VmesteApp.DB.Models;

namespace VmesteApp
{
    public class CalendarDay
    {
        public int DayNumber { get; set; }
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public List<Events> DayEvents { get; set; }
    }
}
