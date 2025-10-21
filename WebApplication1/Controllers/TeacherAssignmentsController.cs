using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class TeacherAssignmentsController : Controller
    {
        private readonly AppDbContext _context;

        public TeacherAssignmentsController(AppDbContext context)
        {
            _context = context;
        }

        // ===================== INDEX (Актуальные назначения) =====================
        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var assignments = await _context.TeacherAssignments
                .Include(t => t.Course)
                .Include(t => t.Teacher)
                .Where(t => t.EndDate >= today)
                .OrderBy(t => t.StartDate)
                .ToListAsync();

            return View(assignments);
        }

        // ===================== ARCHIVE (Прошедшие назначения) =====================
        public async Task<IActionResult> Archive()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var pastAssignments = await _context.TeacherAssignments
                .Include(a => a.Course)
                .Include(a => a.Teacher)
                .Where(a => a.EndDate < today)
                .OrderByDescending(a => a.EndDate)
                .ToListAsync();

            return View(pastAssignments);
        }

        // ===================== DETAILS =====================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var assignment = await _context.TeacherAssignments
                .Include(t => t.Course)
                .Include(t => t.Teacher)
                .FirstOrDefaultAsync(m => m.AssignmentId == id);

            if (assignment == null) return NotFound();

            return View(assignment);
        }

        // ===================== CREATE (GET) =====================
        public IActionResult Create()
        {
            Console.WriteLine($"Courses available: {_context.Courses.Count(c => !c.IsArchived)}");
            Console.WriteLine($"Teachers available: {_context.Teachers.Count(t => !t.IsArchived)}");

            if (!_context.Courses.Any(c => !c.IsArchived) || !_context.Teachers.Any(t => !t.IsArchived))
            {
                TempData["ErrorMessage"] = "Нет активных курсов или преподавателей для назначения.";
                Console.WriteLine("⚠️ Нет активных курсов или преподавателей для назначения.");
                return RedirectToAction(nameof(Index));
            }

            ViewData["CourseId"] = new SelectList(
                _context.Courses.Where(c => !c.IsArchived).OrderBy(c => c.Name),
                "CourseId", "Name"
            );

            ViewData["TeacherId"] = new SelectList(
                _context.Teachers.Where(t => !t.IsArchived).OrderBy(t => t.FullName),
                "TeacherId", "FullName"
            );

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCourseDays(int id)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == id);
            if (course == null)
                return Json(0); // если курс не найден

            return Json(course.Days); // возвращаем количество дней
        }


        // ===================== CREATE (POST) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AssignmentId,CourseId,TeacherId,StartDate")] TeacherAssignment assignment)
        {
            Console.WriteLine("=== ▶ CREATE TeacherAssignment ===");
            Console.WriteLine($"CourseId={assignment.CourseId}, TeacherId={assignment.TeacherId}, Start={assignment.StartDate}");

            if (assignment.CourseId == 0)
            {
                ModelState.AddModelError("", "Не выбран курс.");
                Console.WriteLine("⚠️ Ошибка: не выбран курс");
            }
            if (assignment.TeacherId == 0)
            {
                ModelState.AddModelError("", "Не выбран преподаватель.");
                Console.WriteLine("⚠️ Ошибка: не выбран преподаватель");
            }

            if (assignment.StartDate == default)
            {
                ModelState.AddModelError("", "Дата начала обязательна.");
                Console.WriteLine("⚠️ Ошибка: не указана дата начала");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == assignment.CourseId);
                    if (course != null)
                    {
                        assignment.EndDate = assignment.StartDate.AddDays(course.Days - 1);
                        Console.WriteLine($"📅 Автоматически установлен EndDate = {assignment.EndDate}");
                    }

                    _context.Add(assignment);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Назначение преподавателя успешно добавлено.";
                    Console.WriteLine("✅ Назначение успешно сохранено.");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"💥 Ошибка при сохранении: {ex.Message}");
                    ModelState.AddModelError("", "Ошибка при сохранении данных.");
                }
            }
            else
            {
                Console.WriteLine("❌ ModelState INVALID:");
                foreach (var e in ModelState)
                {
                    foreach (var err in e.Value.Errors)
                        Console.WriteLine($"   → {e.Key}: {err.ErrorMessage}");
                }
            }

            ViewData["CourseId"] = new SelectList(
                _context.Courses.Where(c => !c.IsArchived), "CourseId", "Name", assignment.CourseId
            );
            ViewData["TeacherId"] = new SelectList(
                _context.Teachers.Where(t => !t.IsArchived), "TeacherId", "FullName", assignment.TeacherId
            );

            return View(assignment);
        }

        private bool TeacherAssignmentExists(int id)
        {
            return _context.TeacherAssignments.Any(e => e.AssignmentId == id);
        }
    }
}
