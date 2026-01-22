namespace BackEndGamesTito.API.Data.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string HashPass { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public int StatusId { get; set; }

        // O ponto de interrogação (?) indica que aceita NULL
        public string? Telefone { get; set; }
        public string? TelegramChatId { get; set; } 
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
    }
}