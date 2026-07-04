using FCG.Users.Application.Interfaces;
using FCG.Users.Infrastructure.Data;

namespace FCG.Users.Infrastructure.Repositories;

public class UnidadeDeTrabalho : IUnidadeDeTrabalho
{
    private readonly UsuariosDbContext _context;

    public UnidadeDeTrabalho(UsuariosDbContext context)
    {
        _context = context;
    }

    public async Task<int> SalvarAsync() => await _context.SaveChangesAsync();
}
