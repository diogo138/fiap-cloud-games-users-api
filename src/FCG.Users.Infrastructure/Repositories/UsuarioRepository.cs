using FCG.Users.Application.Interfaces;
using FCG.Users.Domain.Entities;
using FCG.Users.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG.Users.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly UsuariosDbContext _context;

    public UsuarioRepository(UsuariosDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> BuscarPorEmailAsync(string email) =>
        await _context.Usuarios
            .Include(u => u.Administrador)
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

    public async Task<Usuario?> BuscarPorIdAsync(int id) =>
        await _context.Usuarios
            .Include(u => u.Administrador)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<IEnumerable<Usuario>> ListarTodosAsync() =>
        await _context.Usuarios
            .Include(u => u.Administrador)
            .AsNoTracking()
            .ToListAsync();

    public async Task AdicionarAsync(Usuario usuario) =>
        await _context.Usuarios.AddAsync(usuario);

    public async Task<bool> EmailExisteAsync(string email) =>
        await _context.Usuarios.AnyAsync(u => u.Email == email.ToLowerInvariant());
}
