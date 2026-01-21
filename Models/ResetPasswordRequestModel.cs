using System.ComponentModel.DataAnnotations;

namespace BackEndGamesTito.API.Models
{
    public class ResetPasswordRequestModel
    {
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O código de recuperação é obrigatório.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
