namespace AuthWithAdmin.Models
{
    public class Questions
    {
        public int id { get; set; }
        public string questionText { get; set; }
        public string answerText { get; set; }
        public string? nextStep { get; set; }
        public int categoryId { get; set; }

        public string? pdfPath { get; set; }

        //TRANSLATIONS
        public string? questionTextEN { get; set; }
        public string? answerTextEN { get; set; }
        public string? nextStepEN { get; set; }
        public string? pdfPathEN { get; set; }

        public string? questionTextAR { get; set; }
        public string? answerTextAR { get; set; }
        public string? nextStepAR { get; set; }
        public string? pdfPathAR { get; set; }
    }
}
