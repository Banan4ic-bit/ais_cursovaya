using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // 📊 Главная панель администратора
        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardVm
            {
                CoursesCount = await _context.Courses.CountAsync(),
                TeachersCount = await _context.Teachers.CountAsync(),
                OrganizationsCount = await _context.Organizations.CountAsync(),
                RequestsCount = await _context.TrainingRequests.CountAsync()
            };

            var today = DateOnly.FromDateTime(DateTime.Today);

            // ======= Ближайшие курсы =======
            var assignments = await _context.TeacherAssignments
                .Include(a => a.Course)
                .Include(a => a.Teacher)
                .Where(a => a.StartDate >= today)
                .OrderBy(a => a.StartDate)
                .Take(7)
                .ToListAsync();

            foreach (var a in assignments)
            {
                var enrolled = await _context.TrainingRequests
                    .Where(r => r.AssignmentId == a.AssignmentId)
                    .SumAsync(r => (int?)r.NumberOfPeople) ?? 0;

                var capacity = a.Course.Capacity ?? 0;
                var remaining = Math.Max(0, capacity - enrolled);

                vm.UpcomingAssignments.Add(new AdminDashboardVm.AssignmentUtilRow
                {
                    AssignmentId = a.AssignmentId,
                    CourseName = a.Course.Name,
                    TeacherName = a.Teacher.FullName,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Capacity = capacity,
                    Enrolled = enrolled,
                    Remaining = remaining
                });
            }

            // ======= Последние заявки (за последние 30 дней) =======
            var monthAgo = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));

            vm.RecentRequests = await _context.TrainingRequests
                .Include(r => r.Org)
                .Include(r => r.Course)
                .Include(r => r.Assignment)
                    .ThenInclude(a => a.Teacher)
                .Where(r => r.RequestDate != null && r.RequestDate >= monthAgo)
                .OrderByDescending(r => r.RequestDate)
                .Take(10)
                .Select(r => new AdminDashboardVm.RecentRequestRow
                {
                    RequestId = r.RequestId,
                    OrgName = r.Org.Name,
                    CourseName = r.Course.Name,
                    TeacherName = r.Assignment.Teacher.FullName,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    People = r.NumberOfPeople
                })
                .ToListAsync();

            return View(vm);
        }

        // 🕒 Просмотр прошедших курсов
        public async Task<IActionResult> PastCourses()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var pastCourses = await _context.TeacherAssignments
                .Include(a => a.Course)
                .Include(a => a.Teacher)
                .Where(a => a.EndDate < today)
                .OrderByDescending(a => a.EndDate)
                .ToListAsync();

            return View(pastCourses);
        }
    }
}
