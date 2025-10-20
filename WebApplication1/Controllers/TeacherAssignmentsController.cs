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

        // ===================== INDEX =====================
        public async Task<IActionResult> Index()
        {
            var assignments = _context.TeacherAssignments
                .Include(t => t.Course)
                .Include(t => t.Teacher)
                .OrderBy(t => t.StartDate);

            return View(await assignments.ToListAsync());
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
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Name");
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "TeacherId", "FullName");
            return View();
        }

        // ===================== CREATE (POST) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AssignmentId,CourseId,TeacherId,StartDate,EndDate")] TeacherAssignment assignment)
        {
            // Проверка корректности дат
            if (assignment.EndDate < assignment.StartDate)
            {
                ModelState.AddModelError("", "Дата окончания не может быть раньше даты начала.");
            }

            // Проверка на пересечение назначений для того же преподавателя
            var overlap = await _context.TeacherAssignments.AnyAsync(a =>
                a.TeacherId == assignment.TeacherId &&
                ((assignment.StartDate >= a.StartDate && assignment.StartDate <= a.EndDate) ||
                 (assignment.EndDate >= a.StartDate && assignment.EndDate <= a.EndDate)));

            if (overlap)
            {
                ModelState.AddModelError("", "Этот преподаватель уже назначен на другой курс в указанный период.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(assignment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Назначение преподавателя успешно добавлено.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Name", assignment.CourseId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "TeacherId", "FullName", assignment.TeacherId);
            return View(assignment);
        }

        // ===================== EDIT (GET) =====================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var assignment = await _context.TeacherAssignments.FindAsync(id);
            if (assignment == null) return NotFound();

            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Name", assignment.CourseId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "TeacherId", "FullName", assignment.TeacherId);
            return View(assignment);
        }

        // ===================== EDIT (POST) =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AssignmentId,CourseId,TeacherId,StartDate,EndDate")] TeacherAssignment assignment)
        {
            if (id != assignment.AssignmentId) return NotFound();

            if (assignment.EndDate < assignment.StartDate)
            {
                ModelState.AddModelError("", "Дата окончания не может быть раньше даты начала.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(assignment);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Изменения успешно сохранены.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherAssignmentExists(assignment.AssignmentId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "Name", assignment.CourseId);
            ViewData["TeacherId"] = new SelectList(_context.Teachers, "TeacherId", "FullName", assignment.TeacherId);
            return View(assignment);
        }

        // ===================== DELETE (GET) =====================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var assignment = await _context.TeacherAssignments
                .Include(t => t.Course)
                .Include(t => t.Teacher)
                .FirstOrDefaultAsync(m => m.AssignmentId == id);

            if (assignment == null) return NotFound();

            return View(assignment);
        }

        // ===================== DELETE (POST) =====================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignment = await _context.TeacherAssignments.FindAsync(id);
            if (assignment != null)
            {
                _context.TeacherAssignments.Remove(assignment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Назначение успешно удалено.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TeacherAssignmentExists(int id)
        {
            return _context.TeacherAssignments.Any(e => e.AssignmentId == id);
        }
    }
}
