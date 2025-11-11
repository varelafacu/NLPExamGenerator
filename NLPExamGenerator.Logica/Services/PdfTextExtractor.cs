using System.Text;
using UglyToad.PdfPig;

namespace NLPExamGenerator.Logica.Services
{
	public static class PdfTextExtractor
	{
		public static string ExtractText(Stream pdfStream)
		{
			using var document = PdfDocument.Open(pdfStream);
			var builder = new StringBuilder();
			foreach (var page in document.GetPages())
			{
				builder.AppendLine(page.Text);
				builder.AppendLine();
			}
			return builder.ToString();
		}
	}
}


