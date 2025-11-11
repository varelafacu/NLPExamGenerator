using System.Text.Json.Serialization;

namespace NLPExamGenerator.Logica.Models
{
	public class ExamQuestion
	{
		[JsonPropertyName("question")]
		public string Question { get; set; } = string.Empty;

		[JsonPropertyName("options")]
		public List<string> Options { get; set; } = new();

		// Index 0-based of the correct option
		[JsonPropertyName("correctIndex")]
		public int CorrectIndex { get; set; }

		[JsonPropertyName("explanation")]
		public string Explanation { get; set; } = string.Empty;
	}

	public class ExamResponse
	{
		[JsonPropertyName("questions")]
		public List<ExamQuestion> Questions { get; set; } = new();

		[JsonPropertyName("sourceSummary")]
		public string? SourceSummary { get; set; }
	}

	public class OpenAIOptions
	{
		public string? ApiKey { get; set; }
		public string Model { get; set; } = "gpt-4o-mini";
		// Base URL compatible con API estilo OpenAI (e.g., https://api.openai.com/ o https://api.groq.com/openai/)
		public string BaseUrl { get; set; } = "https://api.openai.com/";
	}
}


