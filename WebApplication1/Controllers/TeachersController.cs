using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class TeachersController : Controller
    {
        private readonly AppDbContext _context;

        public TeachersController(AppDbContext context)
        {
            _context = context;
        }

        // ===================== INDEX (Активные преподаватели) =====================
        public async Task<IActionResult> Index()
        {
            var activeTeachers = await _context.Teachers
                .Where(t => !t.IsArchived)
                .OrderBy(t => t.FullName)
                .ToListAsync();

            return View(activeTeachers);
        }

        // ===================== ARCHIVE (Архив преподавателей) =====================
        public async Task<IActionResult> Archive()
        {
            var archivedTeachers = await _context.Teachers
                .Where(t => t.IsArchived)
                .OrderBy(t => t.FullName)
                .ToListAsync();

            return View(archivedTeachers);
        }

        // ===================== DETAILS =====================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Не указан идентификатор преподавателя.";
                return RedirectToAction(nameof(Index));
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(m => m.TeacherId == id);

            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Преподаватель не найден.";
                return RedirectToAction(nameof(Index));
            }

            return View(teacher);
        }

        // ===================== CREATE =====================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeacherId,FullName,Dob,Gender,Education,Category")] Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(teacher);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Преподаватель «{teacher.FullName}» успешно добавлен!";
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    TempData["ErrorMessage"] = "Ошибка при добавлении преподавателя.";
                }
            }

            return View(teacher);
        }

        // ===================== EDIT =====================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Не указан идентификатор преподавателя.";
                return RedirectToAction(nameof(Index));
            }

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Преподаватель не найден.";
                return RedirectToAction(nameof(Index));
            }

            return View(teacher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TeacherId,FullName,Dob,Gender,Education,Category,IsArchived")] Teacher teacher)
        {
            if (id != teacher.TeacherId)
            {
                TempData["ErrorMessage"] = "Некорректный идентификатор преподавателя.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacher);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Изменения для преподавателя «{teacher.FullName}» успешно сохранены!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Teachers.Any(e => e.TeacherId == teacher.TeacherId))
                    {
                        TempData["ErrorMessage"] = "Преподаватель не найден.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(teacher);
        }

        // ===================== ARCHIVE ACTION =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveTeacher(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Преподаватель не найден.";
                return RedirectToAction(nameof(Index));
            }

            teacher.IsArchived = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Преподаватель «{teacher.FullName}» перенесён в архив.";
            return RedirectToAction(nameof(Index));
        }

        // ===================== RESTORE ACTION =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreTeacher(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Преподаватель не найден.";
                return RedirectToAction(nameof(Archive));
            }

            teacher.IsArchived = false;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Преподаватель «{teacher.FullName}» восстановлен из архива.";
            return RedirectToAction(nameof(Archive));
        }

        // ===================== DELETE =====================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Не указан идентификатор преподавателя.";
                return RedirectToAction(nameof(Index));
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(m => m.TeacherId == id);

            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Преподаватель не найден.";
                return RedirectToAction(nameof(Index));
            }

            return View(teacher);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Преподаватель не найден.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Проверка: есть ли назначения
                bool hasAssignments = await _context.TeacherAssignments.AnyAsync(a => a.TeacherId == id);

                if (hasAssignments)
                {
                    TempData["ErrorMessage"] = "Нельзя удалить преподавателя, так как он имеет назначения.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Преподаватель «{teacher.FullName}» успешно удалён.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Ошибка при удалении преподавателя.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.TeacherId == id);
        }
    }
}
