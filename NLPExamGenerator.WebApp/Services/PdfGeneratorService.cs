using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using NLPExamGenerator.WebApp.Models;
using System.Net.Http;

namespace NLPExamGenerator.WebApp.Services
{
	public interface IPdfGeneratorService
	{
		byte[] GenerateExamPdf(ExamResponse exam);
	}

	public class PdfGeneratorService : IPdfGeneratorService
	{
		public byte[] GenerateExamPdf(ExamResponse exam)
		{
			QuestPDF.Settings.License = LicenseType.Community;

			exam ??= new ExamResponse { Questions = new List<ExamQuestion>() };

			// Intentar descargar el logo provisto por el usuario
			byte[]? logoBytes = null;
			try
			{
				using var http = new HttpClient();
				logoBytes = http.GetByteArrayAsync("https://images.seeklogo.com/logo-png/17/1/unlam-universidad-nacional-de-la-matanza-logo-png_seeklogo-171597.png").Result;
			}
			catch
			{
				// ignorar si falla, el PDF se genera sin logo
			}

			var bytes = Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Size(PageSizes.A4);
					page.Margin(30);
					page.PageColor(Colors.White);

					page.Header().Column(header =>
					{
						header.Item().Row(row =>
						{
							if (logoBytes != null)
								row.ConstantItem(80).Image(logoBytes);

							row.RelativeItem().AlignRight().Text("Examen generado")
								.SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);
						});
						header.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
					});

					page.Content().Column(col =>
					{
						if (!string.IsNullOrWhiteSpace(exam.SourceSummary))
						{
							col.Item().Text($"Resumen: {exam.SourceSummary}")
								.FontSize(10).FontColor(Colors.Grey.Darken2);
							col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
						}

						var letters = new[] { "A", "B", "C", "D", "E", "F" };
						for (int idx = 0; idx < (exam.Questions?.Count ?? 0); idx++)
						{
							var q = exam.Questions[idx];
							col.Item().PaddingVertical(6).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(qcol =>
							{
								qcol.Item().Text($"{idx + 1}. {q.Question}")
									.Bold().FontSize(12);

								for (int i = 0; i < (q.Options?.Count ?? 0); i++)
								{
									var isCorrect = i == q.CorrectIndex;
									var text = $"{letters.ElementAtOrDefault(i) ?? ""}. {q.Options[i]}";
									qcol.Item().Text(text)
										.FontSize(11)
										.FontColor(isCorrect ? Colors.Green.Darken2 : Colors.Black);
								}

								if (!string.IsNullOrWhiteSpace(q.Explanation))
								{
									qcol.Item().PaddingTop(4).Text($"Explicación: {q.Explanation}")
										.FontSize(10).Italic().FontColor(Colors.Grey.Darken1);
								}
							});
						}
					});

					page.Footer().AlignRight().Text(x =>
					{
						x.Span("Página ");
						x.CurrentPageNumber();
						x.Span(" de ");
						x.TotalPages();
					});
				});
			}).GeneratePdf();

			return bytes;
		}
	}
}


