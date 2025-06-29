using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AuthWithAdmin.Shared.ClientModels
{
    public class Questions
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "שאלה בשפה העברית היא שדה חובה")]
        [MinLength(5, ErrorMessage = "שאלה בשפה העברית חייבת להכיל לפחות 5 תווים")]
        [MaxLength(60, ErrorMessage = "שאלה בשפה העברית לא יכולה להכיל יותר מ-60 תווים")]
        public string questionText { get; set; } = string.Empty;

        [Required(ErrorMessage = "תשובה בשפה העברית היא שדה חובה")]
        [MinLength(10, ErrorMessage = "תשובה בשפה העברית חייבת להכיל לפחות 10 תווים")]
        public string answerText { get; set; } = string.Empty;

        public string? nextStep { get; set; }
        public string? pdfPath { get; set; }
        public string? pdfPathEN { get; set; }
        public string? pdfPathAR { get; set; }
        public int categoryId { get; set; }
        public Categories Category { get; set; }

        //TRANSLATION
        [Required(ErrorMessage = "שאלה בשפה האנגלית היא שדה חובה")]
        [MinLength(5, ErrorMessage = "השאלה בשפה האנגלית חייבת להכיל לפחות 5 תווים")]
        [MaxLength(60, ErrorMessage = "השאלה בשפה האנגלית לא יכולה להכיל יותר מ-60 תווים")]
        public string? questionTextEN { get; set; }
        [Required(ErrorMessage = "תשובה בשפה האנגלית היא שדה חובה")]
        [MinLength(10, ErrorMessage = "תשובה בשפה האנגלית חייבת להכיל לפחות 10 תווים")]
        public string? answerTextEN { get; set; }
        public string? nextStepEN { get; set; }


        [Required(ErrorMessage = "שאלה בשפה הערבית היא שדה חובה")]
        [MinLength(5, ErrorMessage = "שאלה בשפה הערבית חייבת להכיל לפחות 5 תווים")]
        [MaxLength(60, ErrorMessage = "שאלה בשפה הערבית לא יכולה להכיל יותר מ-60 תווים")]
        public string? questionTextAR { get; set; }
        [Required(ErrorMessage = "תשובה בשפה הערבית היא שדה חובה")]
        [MinLength(10, ErrorMessage = "תשובה בשפה הערבית חייבת להכיל לפחות 10 תווים")]
        public string? answerTextAR { get; set; }
        public string? nextStepAR { get; set; }
    }
}
