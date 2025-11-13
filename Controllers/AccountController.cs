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


namespace BackEndGamesTito.API.Controllers
{
    public class AccountController
    {
    }
}
