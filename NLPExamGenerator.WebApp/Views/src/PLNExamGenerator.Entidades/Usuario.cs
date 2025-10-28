namespace PLNExamGenerator.Entidades
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public bool Plus { get; set; }
    }
}
