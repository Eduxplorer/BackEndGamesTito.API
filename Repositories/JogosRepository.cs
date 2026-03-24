using BackEndGamesTito.API.Data.Models;
using Microsoft.Data.SqlClient;

namespace BackEndGamesTito.API.Repositories
{
    public class JogosRepository
    {
        private readonly string _connectionString = string.Empty;

        public JogosRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("String de conexão 'DefaultConnection' não encontrada");
        }


        // Métodos de visualização de jogos
        public async Task<Jogos?> searchGame(string nome)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var commandText = @"SELECT TOP 1 * FROM Jogos WHERE nome LIKE @nome";

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@nome", nome);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Jogos
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("JogosId")),
                                Nome = reader.IsDBNull(reader.GetOrdinal("Nome")) ? string.Empty : reader.GetString(reader.GetOrdinal("Nome")),
                                Descricao = reader.IsDBNull(reader.GetOrdinal("Descricao")) ? string.Empty : reader.GetString(reader.GetOrdinal("Descricao")),
                                Preco = reader.IsDBNull(reader.GetOrdinal("Preco")) ? 0m : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("Preco"))),
                                Lancamento = reader.IsDBNull(reader.GetOrdinal("Lancamento")) ? DateTime.MinValue : Convert.ToDateTime(reader.GetValue(reader.GetOrdinal("Lancamento")))
                            };
                        }
                    }
                }
                return null;
            }
        }

        public async Task<List<Jogos>> GetAllGames()
        {
            var games = new List<Jogos>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var commandText = @"SELECT * FROM Jogos";
                using (var command = new SqlCommand(commandText, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            games.Add(new Jogos
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("JogosId")),
                                Nome = reader.IsDBNull(reader.GetOrdinal("Nome")) ? string.Empty : reader.GetString(reader.GetOrdinal("Nome")),
                                Descricao = reader.IsDBNull(reader.GetOrdinal("Descricao")) ? string.Empty : reader.GetString(reader.GetOrdinal("Descricao")),
                                Preco = reader.IsDBNull(reader.GetOrdinal("Preco")) ? 0m : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("Preco"))),
                                Avaliacao = reader.IsDBNull(reader.GetOrdinal("Avaliacao")) ? 0m : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("Avaliacao"))),
                                Lancamento = reader.IsDBNull(reader.GetOrdinal("Lancamento")) ? DateTime.MinValue : Convert.ToDateTime(reader.GetValue(reader.GetOrdinal("Lancamento")))
                            });
                        }
                    }
                }
            }
            return games;
        }

        // Métodos de criação

        public async Task<Jogos> createGames(string nome, string descricao, decimal preco, DateTime lancamento)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var commandText = @"INSERT INTO Jogos (Nome, Descricao, Preco, Lancamento) 
                                    VALUES (@nome, @descricao, @preco, @lancamento);
                                    SELECT SCOPE_IDENTITY();";
                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@nome", nome);
                    command.Parameters.AddWithValue("@descricao", descricao);
                    command.Parameters.AddWithValue("@preco", preco);
                    command.Parameters.AddWithValue("@lancamento", lancamento);
                    var insertedId = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return new Jogos
                    {
                        Id = insertedId,
                        Nome = nome,
                        Descricao = descricao,
                        Preco = preco,
                        Lancamento = lancamento
                    };
                }
            }
        }
    }

}
