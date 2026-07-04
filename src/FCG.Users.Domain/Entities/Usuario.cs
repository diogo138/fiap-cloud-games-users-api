namespace FCG.Users.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string NomeUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string HashSenha { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; }
    public bool Ativo { get; set; }
    public Administrador? Administrador { get; set; }
}
