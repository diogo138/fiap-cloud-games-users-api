namespace FCG.Users.Domain.Entities;

public class Administrador
{
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
}
