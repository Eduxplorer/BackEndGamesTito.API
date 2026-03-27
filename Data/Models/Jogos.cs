namespace BackEndGamesTito.API.Data.Models
{
    public class Jogos
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Avaliacao { get; set; }
        public decimal Preco { get; set; }
        public DateTime Lancamento { get; set; }
        public string Imagem { get; set; } = string.Empty;
    }
}
