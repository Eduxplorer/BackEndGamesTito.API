// --- Controllers/AccountController.cs

using BackEndGamesTito.API.Data.Models;
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
using BackEndGamesTito.API.Models;


namespace BackEndGamesTito.API.Controllers
{
    // Criando as rotas para o controller dos jogos
    [ApiController]
    [Route("api/[controller]")] // controle de rotas é o próprio endpoint
    public class GamesController : ControllerBase
    {
        private readonly JogosRepository _jogosRepository;
        public GamesController(JogosRepository jogosRepository)
        {
            _jogosRepository = jogosRepository;
        }
        [HttpGet("game")]
        public async Task<ActionResult<Jogos>> SearchGame([FromQuery(Name = "nome")] string? nome)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nome))
                {
                    return BadRequest(new { message = "O parâmetro 'nome' é obrigatório na query string. Ex.: /api/games/game?nome=NomeDoJogo" });
                }
                var game = await _jogosRepository.searchGame(nome);
                if (game == null)
                {
                    return NotFound(new { message = "Nenhum jogo encontrado." });
                }
                return Ok(game);
            }
            catch (Exception ex)
            {
                // Logar o erro para análise futura
                Console.Error.WriteLine($"Erro ao buscar jogo: {ex}");
                return StatusCode(500, new { message = "Ocorreu um erro ao buscar o jogo.", detalhe = ex.Message });
            }
        }

        [HttpGet("games")]
        public async Task<ActionResult<IEnumerable<Jogos>>> GetJogos()
        {
            try
            {
                var games = await _jogosRepository.GetAllGames();
                return Ok(games);
            }
            catch (Exception ex)
            {
                // Logar o erro para análise futura
                Console.Error.WriteLine($"Erro ao buscar jogos: {ex}");
                return StatusCode(500, new { message = "Ocorreu um erro ao buscar os jogos.", detalhe = ex.Message });
            }
        }

    }
}
