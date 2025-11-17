using System.ComponentModel.DataAnnotations;


namespace BackEndGamesTito.API.Models
{
    public class RegisterRequestModel
    {
        [Required(ErrorMessage = "O campo de Nome é obrigatório!")]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Email é obrigatório!")]
        [EmailAddress(ErrorMessage = "O Email informado não é válido!")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Senha é obrigatório!")]
        public string PasswordHash { get; set; } = string.Empty;
    }
}
