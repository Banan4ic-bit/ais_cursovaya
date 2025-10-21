using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WebApplication1.Controllers
{
    public class TrainingRequestsController : Controller
    {
        private readonly AppDbContext _context;

        public TrainingRequestsController(AppDbContext context)
        {
            _context = context;
        }

        // ======== INDEX ========
        public async Task<IActionResult> Index(bool showAll = false)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var requestsQuery = _context.TrainingRequests
                .Include(r => r.Course)
                .Include(r => r.Org)
                .Include(r => r.Assignment)
                    .ThenInclude(a => a.Teacher)
                .AsQueryable();

            // показываем только актуальные заявки
            if (!showAll)
            {
                requestsQuery = requestsQuery.Where(r => r.EndDate >= today);
            }

            var requests = await requestsQuery
                .OrderBy(r => r.StartDate)
                .ToListAsync();

            ViewBag.ShowAll = showAll;
            return View(requests);
        }

        // ======== AJAX: получить список назначений курса ========
        [HttpGet]
        public async Task<IActionResult> GetAssignmentsByCourse(int courseId)
        {
            var assignments = await _context.TeacherAssignments
                .Include(a => a.Teacher)
                .Where(a => a.CourseId == courseId)
                .OrderBy(a => a.StartDate)
                .Select(a => new
                {
                    assignmentId = a.AssignmentId,
                    teacherName = a.Teacher.FullName,
                    startDate = a.StartDate.ToString("yyyy-MM-dd"),
                    endDate = a.EndDate.ToString("yyyy-MM-dd")
                })
                .ToListAsync();

            if (assignments.Count == 0)
                return Json(new { message = "Нет активных назначений для этого курса." });

            return Json(assignments);
        }

        // ======== AJAX: получить информацию о назначении ========
        [HttpGet]
        public async Task<IActionResult> GetAssignmentInfo(int assignmentId)
        {
            var assignment = await _context.TeacherAssignments
                .Include(a => a.Course)
                .Include(a => a.Teacher)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null)
                return NotFound();

            var totalEnrolled = await _context.TrainingRequests
                .Where(r => r.AssignmentId == assignmentId)
                .SumAsync(r => (int?)r.NumberOfPeople) ?? 0;

            var remaining = assignment.Course.Capacity.HasValue
                ? assignment.Course.Capacity.Value - totalEnrolled
                : 0;

            return Json(new
            {
                course = assignment.Course.Name,
                teacher = assignment.Teacher.FullName,
                startDate = assignment.StartDate.ToString("yyyy-MM-dd"),
                endDate = assignment.EndDate.ToString("yyyy-MM-dd"),
                days = assignment.Course.Days,
                capacity = assignment.Course.Capacity,
                totalEnrolled,
                remaining
            });
        }

        // ======== DETAILS ========
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var request = await _context.TrainingRequests
                .Include(r => r.Org)
                .Include(r => r.Course)
                .Include(r => r.Assignment)
                    .ThenInclude(a => a.Teacher)
                .Include(r => r.RequestParticipants)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
                return NotFound();

            // ✅ вычисляем вместимость и оставшиеся места
            var totalCapacity = request.Course.Capacity ?? 0;
            var currentCount = request.RequestParticipants.Count;
            var remaining = totalCapacity - currentCount;

            ViewBag.RemainingSlots = remaining;
            ViewBag.TotalCapacity = totalCapacity;

            return View(request);
        }

        // ======== CREATE GET ========
        public IActionResult Create()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Показываем только активные или будущие курсы
            var activeCourses = _context.Courses
                .Where(c => _context.TeacherAssignments.Any(a =>
                    a.CourseId == c.CourseId && a.EndDate >= today))
                .OrderBy(c => c.Name)
                .ToList();

            ViewData["CourseId"] = new SelectList(activeCourses, "CourseId", "Name");
            ViewData["OrgId"] = new SelectList(_context.Organizations.OrderBy(o => o.Name), "OrgId", "Name");

            return View();
        }

        // ======== CREATE POST ========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrainingRequest trainingRequest)
        {
            Console.WriteLine("=== POST /TrainingRequests/Create ===");
            Console.WriteLine($"OrgId={trainingRequest.OrgId}, CourseId={trainingRequest.CourseId}, AssignmentId={trainingRequest.AssignmentId}, People={trainingRequest.NumberOfPeople}");

            ModelState.Remove("Org");
            ModelState.Remove("Course");
            ModelState.Remove("Assignment");

            var assignment = await _context.TeacherAssignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AssignmentId == trainingRequest.AssignmentId);

            if (assignment == null)
            {
                ModelState.AddModelError("", "Выбранное назначение не найдено.");
            }
            else
            {
                trainingRequest.CourseId = assignment.CourseId;
                trainingRequest.StartDate = assignment.StartDate;
                trainingRequest.EndDate = assignment.EndDate;

                var existingPeople = await _context.TrainingRequests
                    .Where(r => r.AssignmentId == trainingRequest.AssignmentId)
                    .SumAsync(r => (int?)r.NumberOfPeople) ?? 0;

                if (assignment.Course.Capacity.HasValue &&
                    existingPeople + trainingRequest.NumberOfPeople > assignment.Course.Capacity.Value)
                {
                    ModelState.AddModelError("", $"Недостаточно мест. Осталось: {assignment.Course.Capacity.Value - existingPeople}");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Name", trainingRequest.CourseId);
                ViewData["OrgId"] = new SelectList(_context.Organizations, "OrgId", "Name", trainingRequest.OrgId);
                return View(trainingRequest);
            }

            trainingRequest.RequestDate = DateOnly.FromDateTime(DateTime.Now);
            _context.Add(trainingRequest);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Заявка успешно создана.";
            return RedirectToAction(nameof(Index));
        }

        // ======== EDIT (GET) ========
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var request = await _context.TrainingRequests
                .Include(r => r.Org)
                .Include(r => r.Course)
                .Include(r => r.Assignment)
                    .ThenInclude(a => a.Teacher)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
                return NotFound();

            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Name", request.CourseId);
            ViewData["OrgId"] = new SelectList(_context.Organizations, "OrgId", "Name", request.OrgId);

            var totalEnrolled = await _context.TrainingRequests
                .Where(r => r.AssignmentId == request.AssignmentId)
                .SumAsync(r => (int?)r.NumberOfPeople) ?? 0;

            var capacity = request.Course.Capacity ?? 0;
            var remaining = capacity > 0 ? capacity - totalEnrolled : 0;

            ViewBag.Capacity = capacity;
            ViewBag.TotalEnrolled = totalEnrolled;
            ViewBag.Remaining = remaining;

            return View(request);
        }

        // ======== EDIT (POST) ========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TrainingRequest trainingRequest)
        {
            if (id != trainingRequest.RequestId)
                return NotFound();

            ModelState.Remove("Org");
            ModelState.Remove("Course");
            ModelState.Remove("Assignment");

            var assignment = await _context.TeacherAssignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.AssignmentId == trainingRequest.AssignmentId);

            if (assignment == null)
            {
                ModelState.AddModelError("", "Назначение преподавателя не найдено.");
            }
            else
            {
                trainingRequest.StartDate = assignment.StartDate;
                trainingRequest.EndDate = assignment.EndDate;

                var existingPeople = await _context.TrainingRequests
                    .Where(r => r.AssignmentId == trainingRequest.AssignmentId && r.RequestId != trainingRequest.RequestId)
                    .SumAsync(r => (int?)r.NumberOfPeople) ?? 0;

                var available = assignment.Course.Capacity.HasValue
                    ? assignment.Course.Capacity.Value - existingPeople
                    : int.MaxValue;

                if (trainingRequest.NumberOfPeople > available)
                {
                    ModelState.AddModelError("", $"Недостаточно мест. Осталось только {available}.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Name", trainingRequest.CourseId);
                ViewData["OrgId"] = new SelectList(_context.Organizations, "OrgId", "Name", trainingRequest.OrgId);
                return View(trainingRequest);
            }

            try
            {
                _context.Update(trainingRequest);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Изменения успешно сохранены.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.TrainingRequests.Any(e => e.RequestId == trainingRequest.RequestId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Details), new { id = trainingRequest.RequestId });
        }
    }
}
