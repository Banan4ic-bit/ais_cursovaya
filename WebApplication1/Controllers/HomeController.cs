using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // 🗓 Главная страница — актуальные курсы
        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // ✅ Загружаем только активные или будущие назначения
            var assignments = await _context.TeacherAssignments
                .Include(a => a.Course)
                .Include(a => a.Teacher)
                .Where(a => a.EndDate >= today)
                .OrderBy(a => a.StartDate)
                .ToListAsync();

            // Формируем список расписания с данными по заполненности
            var schedule = assignments.Select(a =>
            {
                var totalEnrolled = _context.TrainingRequests
                    .Where(r => r.AssignmentId == a.AssignmentId)
                    .Sum(r => (int?)r.NumberOfPeople) ?? 0;

                var capacity = a.Course.Capacity ?? 0;
                var remaining = capacity > 0 ? capacity - totalEnrolled : 0;

                return new
                {
                    a.AssignmentId,
                    a.CourseId,
                    CourseName = a.Course.Name,
                    TeacherName = a.Teacher.FullName,
                    a.StartDate,
                    a.EndDate,
                    Capacity = capacity,
                    TotalEnrolled = totalEnrolled,
                    Remaining = remaining
                };
            }).ToList();

            ViewBag.Schedule = schedule;
            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
