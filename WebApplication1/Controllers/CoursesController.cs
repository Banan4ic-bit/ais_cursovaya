using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class CoursesController : Controller
    {
        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        // ------------------ INDEX ------------------
        public async Task<IActionResult> Index()
        {
            return View(await _context.Courses.ToListAsync());
        }

        // ------------------ DETAILS ------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Не указан идентификатор курса.";
                return RedirectToAction(nameof(Index));
            }

            var course = await _context.Courses.FirstOrDefaultAsync(m => m.CourseId == id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Курс не найден.";
                return RedirectToAction(nameof(Index));
            }

            return View(course);
        }

        // ------------------ CREATE ------------------
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseId,Name,Type,Days,Capacity,Price")] Course course)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    course.PriceWithVat = course.Price * 1.2m;
                    _context.Add(course);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Курс «{course.Name}» успешно добавлен.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Ошибка при добавлении курса.";
                }
            }

            return View(course);
        }

        // ------------------ EDIT ------------------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Не указан идентификатор курса.";
                return RedirectToAction(nameof(Index));
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Курс не найден.";
                return RedirectToAction(nameof(Index));
            }

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CourseId,Name,Type,Days,Capacity,Price")] Course course)
        {
            if (id != course.CourseId)
            {
                TempData["ErrorMessage"] = "Некорректный идентификатор курса.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    course.PriceWithVat = course.Price * 1.2m;
                    _context.Update(course);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Изменения для курса «{course.Name}» успешно сохранены.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.CourseId))
                    {
                        TempData["ErrorMessage"] = "Курс не найден.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Ошибка при сохранении изменений.";
                }
            }

            return View(course);
        }

        // ------------------ DELETE ------------------
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Не указан идентификатор курса.";
                return RedirectToAction(nameof(Index));
            }

            var course = await _context.Courses.FirstOrDefaultAsync(m => m.CourseId == id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Курс не найден.";
                return RedirectToAction(nameof(Index));
            }

            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Курс не найден.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Курс «{course.Name}» успешно удалён.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Ошибка при удалении курса.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ------------------ EXISTS ------------------
        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
    }
}
