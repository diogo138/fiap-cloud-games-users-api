using FCG.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCG.Users.Infrastructure.Data;

public class UsuariosDbContext : DbContext
{
    public UsuariosDbContext(DbContextOptions<UsuariosDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Administrador> Administradores => Set<Administrador>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.NomeUsuario).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.HashSenha).IsRequired().HasMaxLength(64);
            entity.Property(u => u.DataCadastro).IsRequired();
            entity.Property(u => u.Ativo).IsRequired();
        });

        modelBuilder.Entity<Administrador>(entity =>
        {
            entity.HasKey(a => a.UsuarioId);

            entity.HasOne(a => a.Usuario)
                  .WithOne(u => u.Administrador)
                  .HasForeignKey<Administrador>(a => a.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
