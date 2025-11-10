using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLPExamGenerator.Logica;
using NLPExamGenerator.WebApp.Models;
using System.Security.Claims;

namespace NLPExamGenerator.WebApp.Controllers
{
    public class MyExamsController : Controller
    {
        private readonly IExamLogica _examLogica;
        private readonly ILogger<MyExamsController> _logger;

        public MyExamsController(IExamLogica examLogica, ILogger<MyExamsController> logger)
        {
            _examLogica = examLogica;
            _logger = logger;
        }

        // Vista principal de "Mis Exámenes"
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToAction("Index", "Home");
                }

                var exams = await _examLogica.GetExamsByUserIdAsync(userId);
                var examViewModels = exams.Select(e => new ExamViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    SourceSummary = e.SourceSummary,
                    CreatedAt = e.CreatedAt,
                    UserName = e.Usuario?.Nombre ?? "",
                    QuestionCount = e.Questions?.Count ?? 0
                }).ToList();

                return View(examViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener exámenes del usuario");
                TempData["Error"] = "Error al cargar los exámenes.";
                return View(new List<ExamViewModel>());
            }
        }

        // Vista detallada de un examen específico
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var exam = await _examLogica.GetExamByIdAsync(id);
                if (exam == null)
                {
                    TempData["Error"] = "Examen no encontrado.";
                    return RedirectToAction("Index");
                }

                var userId = GetCurrentUserId();
                if (exam.UserId != userId)
                {
                    return RedirectToAction("Index");
                }

                var examResponse = exam.ToExamResponse();
                var viewModel = new ExamDetailViewModel
                {
                    Id = exam.Id,
                    Title = exam.Title,
                    Description = exam.Description,
                    SourceSummary = exam.SourceSummary,
                    CreatedAt = exam.CreatedAt,
                    Questions = examResponse.Questions
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles del examen {ExamId}", id);
                TempData["Error"] = "Error al cargar el examen.";
                return RedirectToAction("Index");
            }
        }

        // Eliminar examen
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { success = false, mensaje = "Usuario no autenticado." });
                }

                var deleted = await _examLogica.DeleteExamAsync(id, userId);
                if (deleted)
                {
                    return Json(new { success = true, mensaje = "Examen eliminado correctamente." });
                }
                else
                {
                    return Json(new { success = false, mensaje = "No se pudo eliminar el examen." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar examen {ExamId}", id);
                return Json(new { success = false, mensaje = "Error al eliminar el examen." });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return 0;
        }
    }
}