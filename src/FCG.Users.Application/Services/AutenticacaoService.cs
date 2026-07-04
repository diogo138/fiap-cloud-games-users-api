using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FCG.Users.Application.DTOs;
using FCG.Users.Application.Interfaces;
using FCG.Users.Domain.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace FCG.Users.Application.Services;

public class AutenticacaoService : IAutenticacaoService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AutenticacaoService> _logger;

    public AutenticacaoService(
        IUsuarioRepository usuarioRepository,
        IConfiguration configuration,
        ILogger<AutenticacaoService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<TokenResponseDto?> AutenticarAsync(LoginDto dto)
    {
        _logger.LogInformation("Tentativa de autenticação para: {Email}", dto.Email);

        var usuario = await _usuarioRepository.BuscarPorEmailAsync(dto.Email.ToLowerInvariant());
        if (usuario is null || !usuario.Ativo)
        {
            _logger.LogWarning("Usuário não encontrado ou inativo: {Email}", dto.Email);
            return null;
        }

        var hashSenha = SenhaHelper.GerarHash(dto.Senha);
        if (!string.Equals(usuario.HashSenha, hashSenha, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Senha incorreta para: {Email}", dto.Email);
            return null;
        }

        var role = usuario.Administrador is not null ? "Administrador" : "Usuario";
        var expiracao = GerarExpiracao();
        var token = GerarToken(usuario.Id, usuario.Email, role, expiracao);

        _logger.LogInformation("Autenticação bem-sucedida para: {Email}", dto.Email);

        return new TokenResponseDto(token, usuario.NomeUsuario, usuario.Email, role, expiracao);
    }

    private string GerarToken(int userId, string email, string role, DateTime expiracao)
    {
        var chave = _configuration["Jwt:Chave"]
            ?? throw new InvalidOperationException("Chave JWT não configurada.");
        var emissor = _configuration["Jwt:Emissor"];
        var audiencia = _configuration["Jwt:Audiencia"];

        var keyBytes = Encoding.UTF8.GetBytes(chave);
        var securityKey = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: emissor,
            audience: audiencia,
            claims: claims,
            expires: expiracao,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    private DateTime GerarExpiracao()
    {
        var horas = int.TryParse(_configuration["Jwt:ExpiracaoHoras"], out var h) ? h : 8;
        return DateTime.UtcNow.AddHours(horas);
    }
}
