using Microsoft.AspNetCore.Mvc;
using NLPExamGenerator.WebApp.Models;
using NLPExamGenerator.WebApp.Services;

namespace PNLExamGenerator.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class PdfController : Controller
	{
		private readonly ILogger<PdfController> _logger;
		private readonly IPdfGeneratorService _pdfGeneratorService;

		public PdfController(ILogger<PdfController> logger, IPdfGeneratorService pdfGeneratorService)
		{
			_logger = logger;
			_pdfGeneratorService = pdfGeneratorService;
		}

		[HttpPost("Download")]
		public IActionResult Download([FromBody] ExamResponse exam)
		{
			try
			{
				var pdf = _pdfGeneratorService.GenerateExamPdf(exam);
				return File(pdf, "application/pdf", "Examen.pdf");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al generar PDF");
				return StatusCode(500, new { success = false, mensaje = "No se pudo generar el PDF" });
			}
		}
	}
}

