// --- Controllers/AccountController.cs

using BackEndGamesTito.API.Models;
using BackEndGamesTito.API.Service;
// Adicionar um repositório para gerenciar a lógica de dados
using BackEndGamesTito.API.Repositories;
using BCrypt.Net; // Biblioteca BCrypt para hashing de senhas
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
// --- ADICIONAR ELEMENTOS PARA CRIPTOGRAFIA --- //

using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;


// Usar o banco de dados com o DbUsuario e os atributos de classe Usuario
using DbUsuario = BackEndGamesTito.API.Data.Models.Usuario;


namespace BackEndGamesTito.API.Controllers
{
    // Criando as rotas para o controller da conta
    [ApiController]
    [Route("api/[controller]")] // controle de rotas é o próprio endpoint
    public class AccountController : ControllerBase 
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly EmailService _emailService; 
        private readonly SmsService _smsService; 

        public AccountController(UsuarioRepository usuarioRepository, EmailService emailService, SmsService smsService)
        {
            _usuarioRepository = usuarioRepository;
            _emailService = emailService;
            _smsService = smsService;
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
                string PassSHA256 = ComputeSha256Hash(model.PasswordHash);
                string EmailSHA256 = ComputeSha256Hash(model.Email);


                // Criando a string para a criptografia da senha  e hash(para recuperar senha)

                string PassCrip = PassSHA256 + EmailSHA256 + ApiKey;
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
                    StatusId = 2,
                    Telefone = model.Telefone
                };

                await _usuarioRepository.CreateUserAsync(novoUsuario);

                return Ok(
                    new
                    {
                        erro = false, // success = true
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

        /*        private string ComputeSha256Hash(string ramData)
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
        */
        [HttpPost("login")]

        public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
        {
            // 1. Busca o usuário no banco pelo email OU telefone
            var user = !string.IsNullOrWhiteSpace(model.Email)
                ? await _usuarioRepository.GetUserByEmailAsync(model.Email!)
                : !string.IsNullOrWhiteSpace(model.Telefone)
                    ? await _usuarioRepository.GetUserByPhoneAsync(model.Telefone!)
                    : null;

            if (user == null)
            {
                return Unauthorized(new
                {
                    erro = true,
                    message = "Usuário ou senha inválidos."
                });
            }

            // 2. Recria o hash de login exatamente como no registro
            string ApiKey = "mangaPara_todos_ComLeite_kkk";

            // Cria a senha aplicando SHA256
            string PassSHA256 = ComputeSha256Hash(model.PasswordHash);

            // Importante: usar o e-mail que está salvo no banco (user.Email)
            string EmailSHA256 = ComputeSha256Hash(user.Email ?? string.Empty);

            // Criando a string para a criptografia da senha  e hash(para recuperar senha)
            string PassCrip = PassSHA256 + EmailSHA256 + ApiKey;

            // 3. Verifica o hash usando o Bcrypt
            bool isPasswordValid;

            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(PassCrip, user.PasswordHash);
            }
            catch (Exception)
            {
                isPasswordValid = false;
            }

            if (!isPasswordValid)
            {
                return Unauthorized(new
                {
                    erro = true,
                    message = "Usuário ou senha inválidos."
                });
            }

            // 4. SUCESSO (NO FUTURO GERA UM 'JWT')
            return Ok(new
            {
                usuario = new
                {
                    usuarioId = user.UsuarioId,
                    statusId = user.StatusId,
                },
                erro = false,
                message = "Login realizado com sucesso!"
            });
        }

                    /*
         ********** -- MÉTODO DE HASHING DO SHA356  -- **********
         ********** -- Cria uma instância de SHA256 -- **********
         */

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Computa o hash do dado de entrada 'string'
                // e retorna o resultado com um 'array' de bytes
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Converte o 'array' de bytes em uma string hexadecimal
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }



        // 1. Endpoint para pedir a recuperação (Gera o Token)
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            var user = await _usuarioRepository.GetUserByEmailAsync(email);


            // Dica de segurança: Mesmo se o usuário não existir, retornamos OK
            // para não revelar quem é cliente e quem não é.
            if (user == null)
            {
                // Por segurança, não avisamos se o e-mail não existe (evita varredura de usuários)
                return Ok(new { message = "Se o e-mail existir, um código foi enviado." });
            }

            // Gera um código simples de 6 dígitos (ou um GUID)
            string token = new Random().Next(100000, 999999).ToString();

            // Salva no banco
            await _usuarioRepository.SaveResetTokenAsync(email, token);

            // AQUI VOCÊ ENVIARIA O E-MAIL DE VERDADE

            // --- A MÁGICA ACONTECE AQUI: ENVIO DO E-MAIL ---

            string assunto = "Recuperação de Senha - GamesTito";
            string mensagem = $@"
            <h3>Olá, {user.NomeCompleto}!</h3>
            <p>Você solicitou a troca de senha.</p>
            <p>Seu código de segurança é: <strong>{token}</strong></p>
            <p>Este código expira em 15 minutos.</p>
            <p>Se não foi você, ignore este e-mail.</p>";

            try
            {
                await _emailService.SendEmailAsync(email, assunto, mensagem);
                return Ok(new { message = "E-mail de recuperação enviado com sucesso!" });
            }
            catch (Exception ex)
            {
                // Logar o erro aqui seria o ideal
                return StatusCode(500, new { message = "Erro ao enviar e-mail. Tente novamente." });
            }
        }

        // 2. Endpoint para efetivar a troca
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel model)
        {
            try
            {
                // --- REPLICANDO A SUA LÓGICA DE CRIPTOGRAFIA (Importante ser igual ao Register) ---

                DateTime agora = DateTime.Now;
                string dataString = agora.ToString();
                string ApiKey = "mangaPara_todos_ComLeite_kkk"; // Cuidado com Hardcode!

                string PassSHA256 = ComputeSha256Hash(model.NewPassword);
                string EmailSHA256 = ComputeSha256Hash(model.Email);

                string PassCrip = PassSHA256 + EmailSHA256 + ApiKey;
                string HashCrip = EmailSHA256 + PassSHA256 + dataString + ApiKey;

                string PassBCrypt = BCrypt.Net.BCrypt.HashPassword(PassCrip);
                string HashBCrypt = BCrypt.Net.BCrypt.HashPassword(HashCrip);

                // --- FIM DA CRIPTOGRAFIA ---

                // Chama o repositório para validar o token e atualizar
                bool sucesso = await _usuarioRepository.UpdatePasswordWithTokenAsync(model.Email, model.Token, PassBCrypt, HashBCrypt);

                if (!sucesso)
                {
                    return BadRequest(new { erro = true, message = "Token inválido ou expirado." });
                }

                return Ok(new { erro = false, message = "Senha atualizada com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = true, message = "Erro ao atualizar senha.", detalhe = ex.Message });
            }
        }


        [HttpPost("forgot-password-sms")]
        public async Task<IActionResult> ForgotPasswordSms([FromBody] string telefone)
        {
            // 1. Busca o usuário pelo TELEFONE informado
            // O repositório vai procurar: Quem tem o número "+5511..."?
            var user = await _usuarioRepository.GetUserByPhoneAsync(telefone);

            if (user == null)
            {
                return BadRequest(new { message = "Telefone não encontrado em nossa base de dados." });
            }

            // 2. Gera Token
            string token = new Random().Next(100000, 999999).ToString();

            // 3. Salva o Token no banco
            // IMPORTANTE: Nosso método SaveResetTokenAsync usa o EMAIL para achar a linha no banco.
            // Como buscamos o usuário completo no passo 1, temos o email dele aqui: user.Email
            await _usuarioRepository.SaveResetTokenAsync(user.Email, token);

            // 4. Envia SMS
            string mensagem = $"GAMES TITO: Seu codigo de recuperacao e: {token}. Valido por 15 min.";

            try
            {
                // Envia para o telefone que ele digitou (que é o mesmo que está no banco)
                await _smsService.SendSmsAsync(user.Telefone, mensagem);
                return Ok(new { message = "SMS enviado com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro no envio de SMS.", detalhe = ex.Message });
            }
        }
    }



    }

