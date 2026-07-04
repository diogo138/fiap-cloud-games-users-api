using FCG.Users.Application.DTOs;
using FCG.Users.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Users.API.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(IUsuarioService usuarioService, ILogger<UsuariosController> logger)
    {
        _usuarioService = usuarioService;
        _logger = logger;
    }

    /// <summary>Cadastra um novo usuário.</summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar([FromBody] UsuarioNovoDto dto)
    {
        var resultado = await _usuarioService.CadastrarAsync(dto);
        return CreatedAtAction(nameof(BuscarPorId), new { id = resultado.Id }, resultado);
    }

    /// <summary>Lista todos os usuários. Requer papel Administrador.</summary>
    [HttpGet]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(IEnumerable<UsuarioResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodos()
    {
        var usuarios = await _usuarioService.ListarTodosAsync();
        return Ok(usuarios);
    }

    /// <summary>Retorna um usuário pelo ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var usuario = await _usuarioService.BuscarPorIdAsync(id);

        if (usuario is null)
            return NotFound(new { status = 404, erro = $"Usuário com id {id} não encontrado." });

        return Ok(usuario);
    }

    /// <summary>Atualiza nome e email de um usuário. Requer papel Administrador.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] UsuarioAtualizarDto dto)
    {
        var resultado = await _usuarioService.AtualizarAsync(id, dto);
        return Ok(resultado);
    }

    /// <summary>Desativa (soft delete) um usuário. Requer papel Administrador.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(int id)
    {
        await _usuarioService.DesativarAsync(id);
        return NoContent();
    }

    /// <summary>Concede privilégios de administrador a um usuário. Requer papel Administrador.</summary>
    [HttpPut("{id:int}/admin")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConcederAdmin(int id)
    {
        await _usuarioService.ConcederAdminAsync(id);
        return NoContent();
    }

    /// <summary>Revoga privilégios de administrador de um usuário. Requer papel Administrador.</summary>
    [HttpDelete("{id:int}/admin")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevogarAdmin(int id)
    {
        await _usuarioService.RevogarAdminAsync(id);
        return NoContent();
    }
}
