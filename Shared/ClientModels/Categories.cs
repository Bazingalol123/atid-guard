using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthWithAdmin.Shared.ClientModels
{
    public class Categories
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "שם הקטגוריה בעברית הוא שדה חובה")]
        [MinLength(2, ErrorMessage = "שם הקטגוריה בעברית חייב להכיל לפחות 2 תווים")]
        [MaxLength(50, ErrorMessage = "שם הקטגוריה בעברית לא יכול להכיל יותר מ-50 תווים")]
        public string categoryName { get; set; } = string.Empty;
        [Required(ErrorMessage = "שם הקטגוריה באנגלית הוא שדה חובה")]
        [MinLength(2, ErrorMessage = "שם הקטגוריה באנגלית חייב להכיל לפחות 2 תווים")]
        [MaxLength(50, ErrorMessage = "שם הקטגוריה באנגלית לא יכול להכיל יותר מ-50 תווים")]
        public string categoryNameEN { get; set; }
        [Required(ErrorMessage = "שם הקטגוריה בערבית הוא שדה חובה")]
        [MinLength(2, ErrorMessage = "שם הקטגוריה בערבית חייב להכיל לפחות 2 תווים")]
        [MaxLength(50, ErrorMessage = "שם הקטגוריה בערבית לא יכול להכיל יותר מ-50 תווים")]
        public string categoryNameAR { get; set; }

        public string? image { get; set; }
        public List<Questions> questions { get; set; } = new List<Questions>();
    }
}
