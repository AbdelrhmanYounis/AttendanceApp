using AttendanceApp.Models;

namespace AttendanceApp.ViewModel
{
    public class AttendanceViewModel
    {
        public int attendanceId { get; set; }
        public string name { get; set; }
        public string day { get; set; }
        public string lastUpdate { get; set; }
        public string checkedIn { get; set; }
        public string? checkedOut { get; set; }
        public string? comment { get; set; } 
        public string location { get; set; }
        public string timer { get; set; }


    }
}
