namespace FCG.Users.Application.DTOs;

public record TokenResponseDto(
    string Token,
    string NomeUsuario,
    string Email,
    string Role,
    DateTime Expiracao
);
