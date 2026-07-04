using System.ComponentModel.DataAnnotations;

namespace FCG.Users.Application.DTOs;

public record LoginDto(
    [Required(ErrorMessage = "Email é obrigatório")]
    string Email,

    [Required(ErrorMessage = "Senha é obrigatória")]
    string Senha
);
