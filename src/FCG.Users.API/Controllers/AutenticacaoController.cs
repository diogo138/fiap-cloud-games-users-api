using FCG.Users.Application.DTOs;
using FCG.Users.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Users.API.Controllers;

[ApiController]
[Route("api/autenticacao")]
public class AutenticacaoController : ControllerBase
{
    private readonly IAutenticacaoService _autenticacaoService;
    private readonly ILogger<AutenticacaoController> _logger;

    public AutenticacaoController(IAutenticacaoService autenticacaoService, ILogger<AutenticacaoController> logger)
    {
        _autenticacaoService = autenticacaoService;
        _logger = logger;
    }

    /// <summary>Autentica um usuário e retorna um token JWT.</summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Autenticar([FromBody] LoginDto dto)
    {
        var resultado = await _autenticacaoService.AutenticarAsync(dto);

        if (resultado is null)
        {
            _logger.LogWarning("Falha na autenticação para {Email}", dto.Email);
            return Unauthorized(new { status = 401, erro = "Credenciais inválidas." });
        }

        return Ok(resultado);
    }
}
