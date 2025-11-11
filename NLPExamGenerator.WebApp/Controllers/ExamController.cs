using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using NLPExamGenerator.Logica.Services;
using NLPExamGenerator.Logica;
using NLPExamGenerator.Logica.Models;

namespace NLPExamGenerator.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ExamController : Controller
	{
		private readonly ILogger<ExamController> _logger;
		private readonly IOpenAIService _openAIService;
		private readonly IExamLogica _examLogica;

		public ExamController(ILogger<ExamController> logger, IOpenAIService openAIService, IExamLogica examLogica)
		{
			_logger = logger;
			_openAIService = openAIService;
			_examLogica = examLogica;
		}

		[HttpPost("Generate")]
		public async Task<IActionResult> Generate(List<IFormFile> archivoPdf)
		{
			try
			{
				if (archivoPdf == null || archivoPdf.Count == 0)
				{
					return BadRequest(new { success = false, mensaje = "No se recibieron archivos." });
				}

				long totalBytes = archivoPdf.Sum(f => f.Length);
				const long maxTotalBytes = 25 * 1024 * 1024; // 25 MB
				if (totalBytes > maxTotalBytes)
				{
					return BadRequest(new { success = false, mensaje = "El tamaño total supera 25 MB." });
				}

				var textParts = new List<string>();
				foreach (var file in archivoPdf)
				{
					if (file.Length == 0) continue;
					var isPdf = string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
								|| (string.Equals(file.ContentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase)
									&& file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));
					if (!isPdf)
					{
						return BadRequest(new { success = false, mensaje = $"Tipo inválido: {file.FileName}" });
					}

					try
					{
						using var stream = file.OpenReadStream();
						var content = PdfTextExtractor.ExtractText(stream);
						if (!string.IsNullOrWhiteSpace(content))
						{
							textParts.Add(content);
						}
					}
					catch (Exception exFile)
					{
						_logger.LogWarning(exFile, "No se pudo leer el PDF {File}", file.FileName);
					}
				}

				if (textParts.Count == 0)
				{
					return BadRequest(new { success = false, mensaje = "No se pudo extraer texto de los PDFs (¿escaneados/protegidos?)." });
				}

				var joined = string.Join("\n\n", textParts);
				var exam = await _openAIService.GenerateExamAsync(joined, numQuestions: 10);
				// Guardar automáticamente el examen generado
				try
				{
					int userId = GetCurrentUserId();
					if (userId > 0)
					{
						var examQuestions = exam.Questions.ToExamQuestionDataFromWeb();

						var savedExam = await _examLogica.CreateExamAsync(
							$"Examen generado - {DateTime.Now:dd/MM/yyyy HH:mm}",
							"Examen generado automáticamente desde PDF",
							joined,
							exam.SourceSummary ?? "",
							userId,
							examQuestions
						);

						_logger.LogInformation("Examen guardado automáticamente con ID: {ExamId}", savedExam.Id);
						return Json(new { success = true, exam, examId = savedExam.Id, saved = true });
					}
					else
					{
						_logger.LogWarning("No se pudo obtener el ID del usuario para guardar el examen");
						return Json(new { success = true, exam, saved = false, mensaje = "Examen generado pero no guardado - usuario no autenticado" });
					}
				}
				catch (Exception saveEx)
				{
					_logger.LogError(saveEx, "Error al guardar el examen automáticamente");
					return Json(new { success = true, exam, saved = false, mensaje = "Examen generado pero no se pudo guardar" });
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al generar examen");
				return StatusCode(500, new { success = false, mensaje = $"Ocurrió un error al procesar los PDFs: {ex.Message}" });
			}
		}
		
		[HttpPost("Save")]
		public async Task<IActionResult> SaveExam([FromBody] SaveExamRequest request)
		{
			try
			{
				// Obtener el userId de la sesión o autenticación
				int userId = GetCurrentUserId();
				
				if (userId == 0)
				{
					return BadRequest(new { success = false, mensaje = "Usuario no autenticado." });
				}

				var examQuestions = request.Exam.Questions.ToExamQuestionDataFromWeb();
				
				var savedExam = await _examLogica.CreateExamAsync(
					request.Title,
					request.Description,
					request.SourceText,
					request.Exam.SourceSummary ?? "",
					userId,
					examQuestions
				);

				return Json(new { success = true, examId = savedExam.Id });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al guardar examen");
				return StatusCode(500, new { success = false, mensaje = $"Error al guardar el examen: {ex.Message}" });
			}
		}

		[HttpGet("MyExams")]
		public async Task<IActionResult> GetMyExams()
		{
			try
			{
				int userId = GetCurrentUserId();
				
				if (userId == 0)
				{
					return BadRequest(new { success = false, mensaje = "Usuario no autenticado." });
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

				return Json(new { success = true, exams = examViewModels });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener exámenes del usuario");
				return StatusCode(500, new { success = false, mensaje = $"Error al obtener los exámenes: {ex.Message}" });
			}
		}

		[HttpGet("{examId}")]
		public async Task<IActionResult> GetExam(int examId)
		{
			try
			{
				var exam = await _examLogica.GetExamByIdAsync(examId);
				
				if (exam == null)
				{
					return NotFound(new { success = false, mensaje = "Examen no encontrado." });
				}

				var examResponse = exam.ToExamResponse();
				return Json(new { success = true, exam = examResponse });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener examen");
				return StatusCode(500, new { success = false, mensaje = $"Error al obtener el examen: {ex.Message}" });
			}
		}

		[HttpDelete("{examId}")]
		public async Task<IActionResult> DeleteExam(int examId)
		{
			try
			{
				int userId = GetCurrentUserId();
				
				if (userId == 0)
				{
					return BadRequest(new { success = false, mensaje = "Usuario no autenticado." });
				}

				bool deleted = await _examLogica.DeleteExamAsync(examId, userId);
				
				if (!deleted)
				{
					return NotFound(new { success = false, mensaje = "Examen no encontrado o no tienes permisos para eliminarlo." });
				}

				return Json(new { success = true, mensaje = "Examen eliminado correctamente." });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al eliminar examen");
				return StatusCode(500, new { success = false, mensaje = $"Error al eliminar el examen: {ex.Message}" });
			}
		}

		[HttpGet("Test/CheckSavedExams")]
		public async Task<IActionResult> CheckSavedExams()
		{
			try
			{
				var allExams = await _examLogica.GetRecentExamsAsync(20);
				var examsList = allExams.Select(e => new 
				{
					Id = e.Id,
					Title = e.Title,
					CreatedAt = e.CreatedAt,
					UserName = e.Usuario?.Nombre ?? "Sin usuario",
					QuestionCount = e.Questions?.Count ?? 0
				}).ToList();
				
				return Json(new { success = true, exams = examsList, count = examsList.Count });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener exámenes guardados");
				return StatusCode(500, new { success = false, mensaje = ex.Message });
			}
		}

		private int GetCurrentUserId()
		{
			// Obtener el ID del usuario desde los claims de autenticación
			var userIdClaim = User.FindFirst("UserId")?.Value;
			if (int.TryParse(userIdClaim, out var userId) && userId > 0)
			{
				return userId;
			}
			
			// Fallback: intentar obtener desde la sesión
			var sessionUserId = HttpContext.Session.GetString("UserId");
			if (int.TryParse(sessionUserId, out var sessionUser) && sessionUser > 0)
			{
				return sessionUser;
			}
			
			// Si no hay usuario autenticado, devolver 0
			return 0;
		}
	}
}

