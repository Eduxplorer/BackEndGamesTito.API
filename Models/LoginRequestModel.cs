// Um arquivo de requisição
// Criar algo que seja sutil para quando eu faça a requisição a requisição venha

// como requisição meu cliente tem que fornecer o email e a senha para minha api preparar 

using System.ComponentModel.DataAnnotations; // Objeto nativo do C# para validação de dados por isso não precisamos colocar parentses na chamada do método ComponentModel.DataAnnotations
using System.Collections.Generic;

namespace BackEndGamesTito.API.Models
{
    public class LoginRequestModel : IValidatableObject
    {
        // Email não é obrigatório por si só: o usuário pode enviar email OU telefone
        [EmailAddress(ErrorMessage = "O Email informado não é válido.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Formato de telefone inválido")]
        public string? Telefone { get; set; }

        [Required(ErrorMessage = "O campo senha é obrigatório.")]
        public string PasswordHash { get; set; } = string.Empty; // Por questões de segurança preciso pedir a senha do usuário para a requisição

        // Validação customizada: exigir pelo menos Email ou Telefone
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(Telefone))
            {
                yield return new ValidationResult("Informe o email ou o telefone.", new[] { nameof(Email), nameof(Telefone) });
            }
        }
    }
}

// Toda entrega de API depende de uma requisição 