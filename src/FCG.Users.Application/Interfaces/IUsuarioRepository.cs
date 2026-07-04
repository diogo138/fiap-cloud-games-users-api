using FCG.Users.Domain.Entities;

namespace FCG.Users.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarPorEmailAsync(string email);
    Task<Usuario?> BuscarPorIdAsync(int id);
    Task<IEnumerable<Usuario>> ListarTodosAsync();
    Task AdicionarAsync(Usuario usuario);
    Task<bool> EmailExisteAsync(string email);
}
