using BackEndGamesTito.API.Data.Models;
using BackEndGamesTito.API.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace BackEndGamesTito.API.Repositories
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
                var commandText = @"INSERT INTO dbo.Usuario (NomeCompleto, Email, PassWordHash, HashPass, DataAtualizacao, StatusId) VALUES (@NomeCompleto, @Email, @PasswordHash, @HashPass, @DataAtualizacao, @StatusId)";

                using (var command = new SqlCommand(commandText, connection)) {
                    command.Parameters.AddWithValue("@NomeCompleto", user.NomeCompleto);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("@HashPass", user.HashPass);
                    // Está linha da 'DataAtualizacao' entrada como objeto podendo ser um valor 'nulo'
                    command.Parameters.AddWithValue("@DataAtualizacao", (object)user.DataAtualizacao ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StatusId", user.StatusId);
                    command.Parameters.AddWithValue("@Telefone", (object)user.Telefone ?? DBNull.Value);
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
                {

                    command.Parameters.AddWithValue("@Email", email);
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

                            // Mapeamento de DataAtualizacao (Nullable)
                            DataAtualizacao = reader.IsDBNull(reader.GetOrdinal("DataAtualizacao"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("DataAtualizacao")),

                            StatusId = reader.GetInt32(reader.GetOrdinal("StatusId")),

       
                            // Como é string, verificamos se é nulo. Se não for, pegamos a string.
                            Telefone = reader.IsDBNull(reader.GetOrdinal("Telefone"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("Telefone")),

                            // Dica Sênior: Já que estamos aqui, vamos mapear o Token também
                            // Caso você precise debugar ou validar algo no futuro
                            ResetToken = reader.IsDBNull(reader.GetOrdinal("ResetToken"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("ResetToken")),

                            ResetTokenExpiry = reader.IsDBNull(reader.GetOrdinal("ResetTokenExpiry"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("ResetTokenExpiry"))
                        };
                    }
                }
                }
                // Se não encontrar o usuário, retorna 'nulo'
                return null;
            }
        }

        // Novo método: Busca usuário pelo telefone exato
        public async Task<Usuario?> GetUserByPhoneAsync(string telefone)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                // Mudamos o WHERE para filtrar por Telefone
                var commandText = @"SELECT TOP 1 * FROM dbo.Usuario WHERE Telefone = @Telefone";

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@Telefone", telefone);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Usuario
                            {
                                UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
                                NomeCompleto = reader.GetString(reader.GetOrdinal("NomeCompleto")),
                                Email = reader.GetString(reader.GetOrdinal("Email")), // Importante pegar o e-mail!
                                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                                HashPass = reader.GetString(reader.GetOrdinal("HashPass")),
                                DataCriacao = reader.GetDateTime(reader.GetOrdinal("DataCriacao")),

                                DataAtualizacao = reader.IsDBNull(reader.GetOrdinal("DataAtualizacao"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("DataAtualizacao")),

                                StatusId = reader.GetInt32(reader.GetOrdinal("StatusId")),

                                Telefone = reader.IsDBNull(reader.GetOrdinal("Telefone"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("Telefone")),

                                ResetToken = reader.IsDBNull(reader.GetOrdinal("ResetToken"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("ResetToken")),

                                ResetTokenExpiry = reader.IsDBNull(reader.GetOrdinal("ResetTokenExpiry"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("ResetTokenExpiry"))
                            };
                        }
                    }
                }
                return null;
            }
        }


        // Método para salvar o token de recuperação (Simulando o envio)
        public async Task SaveResetTokenAsync(string email, string token)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                // O token vale por 15 minutos
                var commandText = @"UPDATE dbo.Usuario 
                            SET ResetToken = @Token, 
                                ResetTokenExpiry = DATEADD(minute, 15, GETDATE()) 
                            WHERE Email = @Email";

                using (var command = new SqlCommand(commandText, connection))
                {
                    command.Parameters.AddWithValue("@Token", token);
                    command.Parameters.AddWithValue("@Email", email);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // Método que valida o token e ATUALIZA a senha
        public async Task<bool> UpdatePasswordWithTokenAsync(string email, string token, string newPasswordHash, string newHashPass)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Verifica se o email e o token batem E se o token ainda não expirou
                var checkTokenQuery = @"SELECT Count(1) FROM dbo.Usuario 
                                WHERE Email = @Email 
                                AND ResetToken = @Token 
                                AND ResetTokenExpiry > GETDATE()";

                using (var checkCmd = new SqlCommand(checkTokenQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@Email", email);
                    checkCmd.Parameters.AddWithValue("@Token", token);

                    int exists = (int)await checkCmd.ExecuteScalarAsync();

                    if (exists == 0) return false; // Token inválido ou expirado
                }

                // Se o token é válido, atualiza a senha e limpa o token usado
                var updateQuery = @"UPDATE dbo.Usuario 
                            SET PasswordHash = @PasswordHash, 
                                HashPass = @HashPass, 
                                ResetToken = NULL, 
                                ResetTokenExpiry = NULL,
                                DataAtualizacao = GETDATE()
                            WHERE Email = @Email";

                using (var updateCmd = new SqlCommand(updateQuery, connection))
                {
                    updateCmd.Parameters.AddWithValue("@PasswordHash", newPasswordHash);
                    updateCmd.Parameters.AddWithValue("@HashPass", newHashPass);
                    updateCmd.Parameters.AddWithValue("@Email", email);

                    await updateCmd.ExecuteNonQueryAsync();
                    return true;
                }
            }
        }
    }


}
