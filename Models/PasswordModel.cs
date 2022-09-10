using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class PasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
