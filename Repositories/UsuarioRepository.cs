using BackEndGamesTito.API.Data.Models;
using BackEndGamesTito.API.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace BackEndGamesTito.Repositories
{
    public class UsuarioRepository
    {
        private readonly string _connectionString = string.Empty;

        public UsuarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("String de conexão 'DefaultConnection' não encontrada");
        }

        public async Task CreateUserAsync(Usuario user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var commandText = @"INSERT INTO dbo.Usuario (NomeCompleto, Email, PasswordHash, HashPass, DataAtualizacao, StatusId) VALUES @NomeCompleto, @Email, @PasswordHash, @HashPass, @DataAtualizacao, @StatusId";

                using (var command = new SqlCommand(commandText, connection)) {
                    command.Parameters.AddWithValue("@NomeCompleto", user.NomeCompleto);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("@HashPass", user.HashPass);
                    // Está linha da 'DataAtualizacao' entrada como objeto podendo ser um valor 'nulo'
                    command.Parameters.AddWithValue("@DataAtualizacao", (object)user.DataAtualizacao ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StatusId", user.StatusId);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<Usuario?> GetUserByEmailAsync(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var commandText = @"SELECT TOP 1 * FROM dbo.Usuario WHERE Email = @Email";

                using ( var command = new SqlCommand(commandText, connection))

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Usuario
                        {
                            UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
                            NomeCompleto = reader.GetString(reader.GetOrdinal("NomeCompleto")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                            HashPass = reader.GetString(reader.GetOrdinal("HashPass")),
                            DataCriacao = reader.GetDateTime(reader.GetOrdinal("DataCriacao")),
                            DataAtualizacao = reader.IsDBNull(reader.GetOrdinal("DataAtualizacao"))
                            ? null
                            : reader.GetDateTime(reader.GetOrdinal("DataAtualizacao")),
                            StatusId = reader.GetInt32(reader.GetOrdinal("StatusId"))
                        };
                    }
                }
                // Se não encontrar o usuário, retorna 'nulo'
                return null;
            }
        }
    }
}
