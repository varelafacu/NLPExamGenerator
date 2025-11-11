using NLPExamGenerator.Entidades;
using NLPExamGenerator.Entidades.EF;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace NLPExamGenerator.Logica
{
    public interface IExamLogica
    {
        Task<Exam> CreateExamAsync(string title, string description, string sourceText, string sourceSummary, int userId, List<ExamQuestionData> questions);
        Task<Exam?> GetExamByIdAsync(int examId);
        Task<List<Exam>> GetExamsByUserIdAsync(int userId);
        Task<bool> DeleteExamAsync(int examId, int userId);
        Task<List<Exam>> GetRecentExamsAsync(int count = 10);
    }

    public class ExamLogica : IExamLogica
    {
        private readonly NLPExamGeneratorContext _context;

        public ExamLogica(NLPExamGeneratorContext context)
        {
            _context = context;
        }

        public async Task<Exam> CreateExamAsync(string title, string description, string sourceText, string sourceSummary, int userId, List<ExamQuestionData> questions)
        {
            var exam = new Exam
            {
                Title = title,
                Description = description,
                SourceText = sourceText,
                SourceSummary = sourceSummary,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            _context.Exam.Add(exam);
            await _context.SaveChangesAsync();

            // Agregar las preguntas
            foreach (var questionData in questions)
            {
                var examQuestion = new ExamQuestion
                {
                    ExamId = exam.Id,
                    Question = questionData.Question,
                    Options = JsonSerializer.Serialize(questionData.Options),
                    CorrectIndex = questionData.CorrectIndex,
                    Explanation = questionData.Explanation
                };

                _context.ExamQuestion.Add(examQuestion);
            }

            await _context.SaveChangesAsync();

            return exam;
        }

        public async Task<Exam?> GetExamByIdAsync(int examId)
        {
            return await _context.Exam
                .Include(e => e.Questions)
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(e => e.Id == examId);
        }

        public async Task<List<Exam>> GetExamsByUserIdAsync(int userId)
        {
            return await _context.Exam
                .Where(e => e.UserId == userId)
                .Include(e => e.Questions)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> DeleteExamAsync(int examId, int userId)
        {
            var exam = await _context.Exam
                .FirstOrDefaultAsync(e => e.Id == examId && e.UserId == userId);

            if (exam == null)
                return false;

            _context.Exam.Remove(exam);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Exam>> GetRecentExamsAsync(int count = 10)
        {
            return await _context.Exam
                .Include(e => e.Usuario)
                .OrderByDescending(e => e.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }

    // Clase auxiliar para transferir datos de preguntas
    public class ExamQuestionData
    {
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }
}