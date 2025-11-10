namespace NLPExamGenerator.Entidades
{
    public class ExamQuestion
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Options { get; set; } = string.Empty; // JSON string con las opciones
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public int ExamId { get; set; }

        // Navegación
        public Exam Exam { get; set; } = null!;
    }
}