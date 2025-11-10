using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NLPExamGenerator.WebApp.Services;
using System.Linq;

namespace PNLExamGenerator.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ExamController : Controller
	{
		private readonly ILogger<ExamController> _logger;
		private readonly IOpenAIService _openAIService;

		public ExamController(ILogger<ExamController> logger, IOpenAIService openAIService)
		{
			_logger = logger;
			_openAIService = openAIService;
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
				return Json(new { success = true, exam });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al generar examen");
				return StatusCode(500, new { success = false, mensaje = $"Ocurrió un error al procesar los PDFs: {ex.Message}" });
			}
		}
	}
}

