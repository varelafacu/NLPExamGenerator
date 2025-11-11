namespace NLPExamGenerator.Entidades
{
    public class Exam
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SourceText { get; set; } = string.Empty;
        public string SourceSummary { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }

        // Navegación
        public Usuario Usuario { get; set; } = null!;
        public ICollection<ExamQuestion> Questions { get; set; } = new List<ExamQuestion>();
    }
}