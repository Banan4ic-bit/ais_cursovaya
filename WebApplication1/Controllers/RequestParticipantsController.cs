using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class RequestParticipantsController : Controller
    {
        private readonly AppDbContext _context;

        public RequestParticipantsController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Отображение участников конкретной заявки
        public async Task<IActionResult> Index(int? requestId)
        {
            if (requestId == null)
                return NotFound();

            var request = await _context.TrainingRequests
                .Include(r => r.Org)
                .Include(r => r.Course)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
                return NotFound();

            ViewBag.Request = request;

            var participants = await _context.RequestParticipants
                .Where(p => p.RequestId == requestId)
                .ToListAsync();

            return View(participants);
        }

        // ✅ GET: создание участника для конкретной заявки
        public IActionResult Create(int? requestId)
        {
            if (requestId == null)
                return NotFound();

            var request = _context.TrainingRequests
                .Include(r => r.Org)
                .Include(r => r.Course)
                .FirstOrDefault(r => r.RequestId == requestId);

            if (request == null)
                return NotFound();

            ViewBag.RequestId = requestId;
            ViewBag.RequestInfo = $"{request.Org.Name} — {request.Course.Name}";
            return View(new RequestParticipant { RequestId = requestId.Value });
        }

        // ✅ POST: сохранение нового участника
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ParticipantId,RequestId,FullName,Position")] RequestParticipant participant)
        {
            Console.WriteLine("=== POST /RequestParticipants/Create ===");
            Console.WriteLine($"RequestId = {participant.RequestId}, FullName = {participant.FullName}, Position = {participant.Position}");

            // Убираем навигационное свойство из проверки валидации
            ModelState.Remove("Request");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState INVALID!");
                foreach (var e in ModelState)
                {
                    foreach (var err in e.Value.Errors)
                    {
                        Console.WriteLine($"Поле: {e.Key} — Ошибка: {err.ErrorMessage}");
                    }
                }

                ViewBag.RequestId = participant.RequestId;
                return View(participant);
            }

            try
            {
                _context.Add(participant);
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ Участник успешно добавлен в базу!");

                TempData["SuccessMessage"] = $"Участник {participant.FullName} добавлен.";
                return RedirectToAction("Details", "TrainingRequests", new { id = participant.RequestId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Ошибка при сохранении: {ex.Message}");
                throw;
            }
        }



        // ✅ GET: редактирование участника
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var participant = await _context.RequestParticipants
                .Include(p => p.Request)
                    .ThenInclude(r => r.Org)
                .Include(p => p.Request)
                    .ThenInclude(r => r.Course)
                .FirstOrDefaultAsync(p => p.ParticipantId == id);

            if (participant == null)
                return NotFound();

            ViewBag.RequestInfo = $"{participant.Request.Org.Name} — {participant.Request.Course.Name}";
            return View(participant);
        }

        // ✅ POST: редактирование участника
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ParticipantId,RequestId,FullName,Position")] RequestParticipant participant)
        {
            if (id != participant.ParticipantId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.RequestInfo = $"Заявка №{participant.RequestId}";
                return View(participant);
            }

            try
            {
                _context.Update(participant);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Изменения участника {participant.FullName} сохранены.";
                return RedirectToAction("Details", "TrainingRequests", new { id = participant.RequestId });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.RequestParticipants.Any(e => e.ParticipantId == participant.ParticipantId))
                    return NotFound();
                else
                    throw;
            }
        }

        // ✅ Удаление участника (с возвратом в детали заявки)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var participant = await _context.RequestParticipants.FindAsync(id);
            if (participant != null)
            {
                int requestId = participant.RequestId;
                _context.RequestParticipants.Remove(participant);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Участник {participant.FullName} удалён.";
                return RedirectToAction("Details", "TrainingRequests", new { id = requestId });
            }

            return RedirectToAction("Index", "TrainingRequests");
        }
    }
}
