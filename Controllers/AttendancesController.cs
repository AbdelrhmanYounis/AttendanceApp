using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AttendanceApp.Data;
using AttendanceApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OfficeOpenXml;
using Newtonsoft.Json;
using AttendanceApp.ViewModel;
using Microsoft.AspNetCore.Authorization;

namespace AttendanceApp.Controllers
{
    [Authorize]
    public class AttendancesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AttendancesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Attendances
        public IActionResult Index()
        {
            var datasource = _context.Attendance.Include(a => a.Employee)
                            .OrderByDescending(a => a.Day).ThenByDescending(a => a.StartHour)
                            .ThenByDescending(a => a.EndHour).ToList();
            var user = _userManager.GetUserAsync(User).Result;
            string roleName = (user != null) ? _userManager.GetRolesAsync(user).Result.FirstOrDefault() : "";

            if (roleName.Equals("User"))
                datasource = datasource.Where(a => a.EmployeeId == user.Id).ToList();

            return View(datasource);
        }
         public IActionResult LoadData()
        {
            try
            {
                var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
                // Skiping number of Rows count
                var start = Request.Form["start"].FirstOrDefault();
                // Paging Length 10,20
                var length = Request.Form["length"].FirstOrDefault();
                // Sort Column Name
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                // Sort Column Direction ( asc ,desc)
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                // Search Value from (Search box)
                var searchValue = Request.Form["search[value]"].FirstOrDefault();

                //Paging Size (10,20,50,100)
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // Getting all Customer data
                var customerData =this.DataSource();

                //Sorting
                 if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                 {
                   // customerData = customerData.OrderBy(x => x.checkedIn);
                   /*  customerData = customerData.AsQueryable().OrderBy(sortColumn + " " + sortColumnDirection);*/
                      Func<AttendanceViewModel, object> orderByValue = sd => typeof(AttendanceViewModel).GetProperty(sortColumn).GetValue(sd);
                      customerData = sortColumnDirection=="asc"?
                        customerData.OrderBy(safetyData => orderByValue(safetyData)):
                        customerData.OrderByDescending(safetyData => orderByValue(safetyData));                    
                    
                }
                //Search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    customerData = customerData.Where(m =>
                    (m.location != null && m.location.StartsWith(searchValue)) ||
                    (m.name != null && m.name.Contains(searchValue)) ||
                    (m.comment != null && m.comment.Contains(searchValue)));               
                    
                }

                //total number of rows count 
                recordsTotal = customerData.Count();
                //Paging 
                var data = customerData.Skip(skip).Take(pageSize).ToList();
                //Returning Json Data
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

            }
            catch (Exception)
            {
                throw;
            }

        }
       
        public async Task<IActionResult> WorkActive()
        {
            var ActiveEmployee = await _context.Attendance.Include(a => a.Employee)
                            .Where(d =>d.Day.Year== DateTime.Now.Year&& d.Day.DayOfYear.Equals(DateTime.Now.DayOfYear)).ToListAsync();
            return View(ActiveEmployee);
        }
        // GET: Attendances/Create
        public IActionResult Create()
        {
            var datasource = from x in _context.Users
                             select new
                             {
                                 x.Id,
                                 DisplayField = String.Format("{0} {1}", x.FirstName, x.LastName)
                             };
            ViewData["EmployeeId"] = new SelectList(datasource, "Id", "DisplayField");

            return View();
        }
         [HttpPost]
        public async Task<IActionResult> Create( Attendance attendance)
        {
            if (attendance.EndHour != null && attendance.EndHour <= attendance.StartHour)
            {
                ModelState.AddModelError("EndHour", "Checked Out must be greater than Checked In.");
                return View(attendance);
            }

            if (attendance.EmployeeId == null)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user != null)
                    attendance.EmployeeId = user.Id;
                else
                    return View(attendance);
            }
            ModelState["EmployeeId"].ValidationState= ModelValidationState.Valid;
            ModelState["Employee"].ValidationState = ModelValidationState.Valid;
            ModelState["EndHour"].ValidationState = ModelValidationState.Valid;

            if (ModelState.IsValid)
            {
                _context.Add(attendance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var datasource = from x in _context.Users
                             select new
                             {
                                 x.Id,
                                 DisplayField = String.Format("{0} {1}", x.FirstName, x.LastName)
                             };
            ViewData["EmployeeId"] = new SelectList(datasource, "Id", "DisplayField");
            return View(attendance);
        }

        // GET: Attendances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Attendance == null)
            {
                return NotFound();
            }

            var attendance = await _context.Attendance.FindAsync(id);
            if (attendance == null)
            {
                return NotFound();
            }
            var datasource = from x in _context.Users
                             select new
                             {
                                 x.Id,
                                 DisplayField = String.Format("{0} {1}", x.FirstName, x.LastName)
                             };
            ViewData["EmployeeId"] = new SelectList(datasource, "Id", "DisplayField");
            return View(attendance);
        }

        // POST: Attendances/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Attendance attendance)
        {
            var datasource = from x in _context.Users
                             select new
                             {
                                 x.Id,
                                 DisplayField = String.Format("{0} {1}", x.FirstName, x.LastName)
                             };
            ViewData["EmployeeId"] = new SelectList(datasource, "Id", "DisplayField");
            if (attendance.EndHour != null && attendance.EndHour<=attendance.StartHour)
            {
                ModelState.AddModelError("EndHour", "Checked Out must be greater than Checked In.");
                return View(attendance);
            }
            if (id != attendance.AttendanceId)
            {
                return NotFound();
            }
            if (attendance.EmployeeId == null)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user != null)
                    attendance.EmployeeId = user.Id;
                else
                    return View(attendance);
            }

            ModelState["EmployeeId"].ValidationState = ModelValidationState.Valid;
            ModelState["Employee"].ValidationState = ModelValidationState.Valid;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(attendance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AttendanceExists(attendance.AttendanceId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(attendance);
        }

        // GET: Attendances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Attendance == null)
            {
                return NotFound();
            }

            var attendance = await _context.Attendance
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(m => m.AttendanceId == id);
            if (attendance == null)
            {
                return NotFound();
            }

            return View(attendance);
        }

        // POST: Attendances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Attendance == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Attendance'  is null.");
            }
            var attendance = await _context.Attendance.FindAsync(id);
            if (attendance != null)
            {
                _context.Attendance.Remove(attendance);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AttendanceExists(int id)
        {
          return (_context.Attendance?.Any(e => e.AttendanceId == id)).GetValueOrDefault();
        }
        public async Task<IActionResult> ExportExcel()
        {
           
           /* string y = "";
            foreach (var item in datasource)
            {
               y=item.checkedOut == null ? "" : $"{item.checkedOut?.Hours % 12}:{(item.checkedOut?.Minutes < 10 ? "0" : "")}{item.checkedOut?.Minutes} {(item.checkedOut?.Hours / 12 > 0 ? "PM" : "AM")}"
                              ;
            }
           */
            var stream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.Add("Sheet1");
                workSheet.Cells.LoadFromCollection(this.DataSource(), true);
                package.Save();
            }
            stream.Position = 0;

            string excelName = $"Contrato Employees -{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}.xlsx";
            // above I define the name of the file using the current datetime.

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName); // this will be the actual export.
        }
        public IEnumerable<AttendanceViewModel> DataSource()
        {
            var datasource = _context.Attendance.Include(a => a.Employee)
                            .OrderByDescending(a=>a.Day).ThenByDescending(a=>a.StartHour)
                            .ThenByDescending(a=>a.EndHour).ToList();
            var user = _userManager.GetUserAsync(User).Result;
            string roleName = (user != null) ? _userManager.GetRolesAsync(user).Result.FirstOrDefault() : "";

            if (roleName.Equals("User"))
                datasource = datasource.Where(a => a.EmployeeId == user.Id).ToList();
           

            
            IEnumerable<AttendanceViewModel> AVM=new List<AttendanceViewModel>();
           foreach (var ds in datasource)
            {
               var x = new AttendanceViewModel
               {
                   attendanceId = ds.AttendanceId,
                   name=$"{ds.Employee.FirstName} {ds.Employee.LastName}",
                   day=ds.Day.ToString("dddd, dd MMMM yyyy"),
                   lastUpdate = ds.LastUpdate.ToString("MM / dd / yyyy hh: mm tt"),
                   checkedIn = GetCheck(ds.StartHour),
                   checkedOut = GetCheck(ds.EndHour),
                   timer = GetWorkHour(ds.StartHour, ds.EndHour),
                   location = ds.WorkFrom.ToString(),
                   comment = ds.Comment==null?" ": ds.Comment,
               };
                AVM=AVM.Append(x);
            }
            return AVM;
            /*
          var data=(from x in _context.Attendance.Include(a => a.Employee)
                             select new
                             {
                                 Name = String.Format("{0} {1}", x.Employee.FirstName, x.Employee.LastName),
                                 Day = x.Day.ToString("dddd, dd MMMM yyyy"),
                                 LastUpdate = x.LastUpdate.ToString("MM / dd / yyyy hh: mm tt"),
                                 checkedIn = GetCheck(x.StartHour),
                                 checkedOut = GetCheck(x.EndHour),
                                 Timer = GetWorkHour(x.StartHour,x.EndHour),
                                 Location = x.WorkFrom,
                                 Comment = x.Comment,
                                 
                             });
            */
        }
        public string GetCheck(TimeSpan? check)
        {
            if(check !=null)
            return $"{check?.Hours % 12}:{(check?.Minutes < 10 ? "0" : "")}{check?.Minutes} {(check?.Hours / 12 > 0 ? "PM" : "AM")}";
                                
            return "";
        }
        public string GetWorkHour(TimeSpan checkedIn, TimeSpan? checkedOut)
        {
            if (checkedOut != null)
                return (checkedOut- checkedIn)?.ToString("hh':'mm");
            return "";
        }
    }
}
