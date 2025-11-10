using NLPExamGenerator.Entidades;
using NLPExamGenerator.Logica;
using System.Text.Json;

namespace NLPExamGenerator.WebApp.Models
{
    public static class ExamExtensions
    {
        public static List<ExamQuestionData> ToExamQuestionData(this List<NLPExamGenerator.Entidades.ExamQuestion> examQuestions)
        {
            return examQuestions.Select(eq => new ExamQuestionData
            {
                Question = eq.Question,
                Options = string.IsNullOrEmpty(eq.Options) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(eq.Options) ?? new List<string>(),
                CorrectIndex = eq.CorrectIndex,
                Explanation = eq.Explanation
            }).ToList();
        }

        public static List<ExamQuestionData> ToExamQuestionDataFromWeb(this List<Models.ExamQuestion> webQuestions)
        {
            return webQuestions.Select(wq => new ExamQuestionData
            {
                Question = wq.Question,
                Options = wq.Options,
                CorrectIndex = wq.CorrectIndex,
                Explanation = wq.Explanation
            }).ToList();
        }

        public static ExamResponse ToExamResponse(this Exam exam)
        {
            var examQuestions = exam.Questions.Select(eq => new Models.ExamQuestion
            {
                Question = eq.Question,
                Options = string.IsNullOrEmpty(eq.Options) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(eq.Options) ?? new List<string>(),
                CorrectIndex = eq.CorrectIndex,
                Explanation = eq.Explanation
            }).ToList();

            return new ExamResponse
            {
                Questions = examQuestions,
                SourceSummary = exam.SourceSummary
            };
        }
    }

    // Modelo para mostrar información del examen en las vistas
    public class ExamViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SourceSummary { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
    }

    // Modelo para crear un nuevo examen
    public class CreateExamViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SourceText { get; set; } = string.Empty;
        public ExamResponse? GeneratedExam { get; set; }
    }

    // Modelo para guardar un examen generado
    public class SaveExamRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SourceText { get; set; } = string.Empty;
        public ExamResponse Exam { get; set; } = new();
    }

    // Modelo para la vista detallada del examen
    public class ExamDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SourceSummary { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<ExamQuestion> Questions { get; set; } = new();
    }
}