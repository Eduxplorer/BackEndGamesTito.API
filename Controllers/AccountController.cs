// --- Controllers/AccountController.cs

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using BackEndGamesTito.API.Models;
// Adicionar um repositório para gerenciar a lógica de dados

using System.Threading.Tasks;

using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

// --- ADICIONAR ELEMENTOS PARA CRIPTOGRAFIA --- //

using System.Security.Cryptography;
using System.Text;
using BCrypt.Net; // Biblioteca BCrypt para hashing de senhas

// Usar o banco de dados

// Usar o banco de dados com o DbUsuario e os atributos de classe Usuario
using DbUsuario = BackEndGamesTito.API.Data.Models.Usuario;
using BackEndGamesTito.Repositories;


namespace BackEndGamesTito.API.Controllers
{
    // Criando as rotas para o controller da conta
    [ApiController]
    [Route("api/[controller]")] // controle de rotas é o meu próprio endpoint
    public class AccountController : ControllerBase 
    {
        private readonly UsuarioRepository _usuarioRepository;

    ´public AccountController(UsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        [HttpPost("register")]

        public async Task<IActionResult> Register([FromBody] RegisterRequestModel model)
        {
            try
            {
                // Criando a criptografia 
                DateTime agora = DateTime.Now;
                // Converte a data em string
                string dataString = agora.ToString();
                // Palavra passe
                string ApiKey = "mangaPara_todos_ComLeite_kkk";

                // Cria a senha e email aplicando SHA256
                string PassSHA256 = ComputeSha256Hash(model.PassWordHash);
                string EmailSHA256 = ComputeSha256Hash(model.Email);


                // Criando a string para a criptografia da senha  e hash(para recuperar senha)

                string PassCrip = PassSHA256 + ApiKey;
                string HashCrip = EmailSHA256 + PassSHA256 + dataString + ApiKey;

                // Aplicando o BCrypt

                string PassBCrypt = BCrypt.Net.BCrypt.HashPassword(PassCrip);
                string HashBCrypt = BCrypt.Net.BCrypt.HashPassword(HashCrip);

                // Criando o 'array' com todos os dados do usuário para depois ser gravado

                var novoUsuario = new DbUsuario
                {
                    NomeCompleto = model.NomeCompleto,
                    Email = model.Email,
                    PasswordHash = PassBCrypt,
                    HashPass = HashBCrypt,
                    DataAtualizacao = DateTime.Now,
                    StatusId = 2
                };

                await _usuarioRepository.CreateUserAsync(novoUsuario);

                return Ok(
                    new
                    {
                        erro = false, // success = false
                        message = "Usuario cadastrado com sucesso!",
                        usuario = new
                        {
                            model.NomeCompleto,
                            model.Email,
                            model.PasswordHash,
                        }
                    }
                    );


            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // Erro de email duplicado pois o valor 'UNIQUE' está no campo de banco de dados
                return Conflict(new
                {
                    erro = true, // success = false,
                    message = "Este email já está em uso!"
                });

            }
            catch (Exception ex)
            {
                // return StatusCode(500, new { message = $"Erro: {ex.Message}" });
                return StatusCode(500, new
                {
                    erro = true, // success = false,
                    message = "Sistema indisponivel no momento tente mais tarde!",
                    codErro = $"Erro: {ex.Message}",
                });
            }
        }
          
        
        // **** Cria uma instância de SHA256 || MÉTODO DE HASHING DO SHA256 *****

        private string ComputeSha256Hash(string ramData)
        {

            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Computa o hash do dado de entrada 'string'
                // e retorna o resultado como um 'array' de bytes
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(ramData));

                // Converte o 'array' de bytes em uma string hexadecimal
                StringBuilder builder = new StringBuilder();

                for(int i = 0; i < bytes.Length; i++) 
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }

        }

    }
}
