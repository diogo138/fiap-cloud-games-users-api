using FCG.Users.Application.DTOs;

namespace FCG.Users.Application.Interfaces;

public interface IAutenticacaoService
{
    Task<TokenResponseDto?> AutenticarAsync(LoginDto dto);
}
