using System.ComponentModel.DataAnnotations;

namespace FCG.Users.Application.DTOs;

public record UsuarioNovoDto(
    [Required(ErrorMessage = "NomeUsuario é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "NomeUsuario deve ter entre 3 e 100 caracteres")]
    string NomeUsuario,

    [Required(ErrorMessage = "Email é obrigatório")]
    string Email,

    [Required(ErrorMessage = "Senha é obrigatória")]
    string Senha
);
