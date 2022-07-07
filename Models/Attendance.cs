using System.ComponentModel.DataAnnotations;

namespace AttendanceApp.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string EmployeeId { get; set; }
        public virtual ApplicationUser Employee { get; set; }

        [Display(Name = "Last Update")]
        public DateTime LastUpdate { get; set; } = DateTime.Now; //DateTime.Now.ToString("MM / dd / yyyy hh: mm tt");//07-31-2018 08:15 PM
        [Required]
        public DateTime Day { get; set; }//DateTime.Now.ToString(“MM/dd/yyyy”)	07/31/2018
        [Required]
        [Display(Name = "Checked In")]
      //  public int StartHourId { get; set; }
        public TimeSpan  StartHour { get; set; }//DateTime.Now.ToString("hh:mm tt")		08:15 AM
        [Display(Name = "Checked Out")]
     //   public int? EndHourId { get; set; }
        public TimeSpan? EndHour { get; set; }//DateTime.Now.ToString(“hh:mm tt”)		08:15 PM
        public string? Comment { get; set; }
        [Required]
        [Display(Name = "Location")]
        public WorkFrom WorkFrom { get; set; }

    }
    public enum WorkFrom
    {
        Office,
        Home
    }
        
}
