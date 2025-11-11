using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NLPExamGenerator.Logica.Models;

namespace NLPExamGenerator.Logica.Services
{
	public interface IOpenAIService
	{
		Task<ExamResponse> GenerateExamAsync(string sourceText, int numQuestions = 10, CancellationToken cancellationToken = default);
	}

	public class OpenAIService : IOpenAIService
	{
		private readonly HttpClient _httpClient;
		private readonly OpenAIOptions _options;
		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false,
		};

		public OpenAIService(HttpClient httpClient, IOptions<OpenAIOptions> options)
		{
			_httpClient = httpClient;
			_options = options.Value;

			var baseUrl = string.IsNullOrWhiteSpace(_options.BaseUrl) ? "https://api.openai.com/" : _options.BaseUrl;
			if (!baseUrl.EndsWith("/")) baseUrl += "/";
			_httpClient.BaseAddress ??= new Uri(baseUrl);
			if (!string.IsNullOrWhiteSpace(_options.ApiKey))
			{
				_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
			}
		}

		public async Task<ExamResponse> GenerateExamAsync(string sourceText, int numQuestions = 10, CancellationToken cancellationToken = default)
		{
			// Limitar longitud para evitar payloads enormes
			const int maxChars = 60000;
			if (sourceText.Length > maxChars)
			{
				sourceText = sourceText[..maxChars];
			}

			var systemPrompt = "Eres un generador de exámenes. A partir de material de estudio, crea preguntas válidas, inequívocas y relevantes.";

			var userPrompt = $@"
Genera {numQuestions} preguntas de opción múltiple en español basadas estrictamente en el material a continuación.
Requisitos:
- 4 opciones por pregunta (A, B, C, D) todas plausibles.
- Solo 1 correcta. Indica el índice correcto (0-3).
- Explica brevemente por qué la respuesta es correcta.
- Si el material no da suficiente contexto para una pregunta, no la inventes (omite).

Devuelve EXCLUSIVAMENTE JSON válido con este esquema:
{{
  ""questions"": [
    {{
      ""question"": ""texto de la pregunta"",
      ""options"": [""A"", ""B"", ""C"", ""D""],
      ""correctIndex"": 0,
      ""explanation"": ""breve explicación""
    }}
  ],
  ""sourceSummary"": ""breve resumen del material""
}}

Material:\n{sourceText}";

			var request = new
			{
				model = string.IsNullOrWhiteSpace(_options.Model) ? "gpt-4o-mini" : _options.Model,
				messages = new object[]
				{
					new { role = "system", content = systemPrompt },
					new { role = "user", content = userPrompt }
				},
				temperature = 0.3,
				response_format = new { type = "json_object" }
			};

			using var content = new StringContent(JsonSerializer.Serialize(request, JsonOptions), Encoding.UTF8, "application/json");
			// El path es OpenAI-compatible. Para Groq, usar BaseUrl=https://api.groq.com/openai/
			using var response = await _httpClient.PostAsync("v1/chat/completions", content, cancellationToken);
			// Leer contenido siempre para poder diagnosticar errores del API
			var raw = await response.Content.ReadAsStringAsync(cancellationToken);
			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidOperationException($"OpenAI error {(int)response.StatusCode}: {raw}");
			}

			using var doc = JsonDocument.Parse(raw);

			// Ruta: choices[0].message.content
			var contentText = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
			if (string.IsNullOrWhiteSpace(contentText))
			{
				return new ExamResponse { Questions = new List<ExamQuestion>() };
			}

			// Parsear JSON del modelo
			var exam = JsonSerializer.Deserialize<ExamResponse>(contentText, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});

			return exam ?? new ExamResponse { Questions = new List<ExamQuestion>() };
		}
	}
}


