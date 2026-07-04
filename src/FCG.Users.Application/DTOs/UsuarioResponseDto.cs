namespace FCG.Users.Application.DTOs;

public record UsuarioResponseDto(
    int Id,
    string NomeUsuario,
    string Email,
    DateTime DataCadastro,
    bool Ativo,
    string Role
);
