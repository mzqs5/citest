using System.ComponentModel.DataAnnotations;

namespace IF.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}