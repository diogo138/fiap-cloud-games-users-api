using FCG.Users.Application.DTOs;
using FCG.Users.Application.Events;
using FCG.Users.Application.Interfaces;
using FCG.Users.Domain.Entities;
using FCG.Users.Domain.Helpers;
using Microsoft.Extensions.Logging;

namespace FCG.Users.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnidadeDeTrabalho _uow;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        IUnidadeDeTrabalho uow,
        IEventPublisher eventPublisher,
        ILogger<UsuarioService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _uow = uow;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<UsuarioResponseDto> CadastrarAsync(UsuarioNovoDto dto)
    {
        _logger.LogInformation("Iniciando cadastro de usuário: {Email}", dto.Email);

        if (!SenhaHelper.EmailValido(dto.Email))
            throw new ArgumentException("Formato de email inválido.");

        if (!SenhaHelper.SenhaValida(dto.Senha))
            throw new ArgumentException(
                "Senha deve ter no mínimo 8 caracteres, contendo ao menos 1 letra, 1 número e 1 caractere especial.");

        if (await _usuarioRepository.EmailExisteAsync(dto.Email))
            throw new InvalidOperationException($"Já existe um usuário com o email '{dto.Email}'.");

        var usuario = new Usuario
        {
            NomeUsuario = dto.NomeUsuario,
            Email = dto.Email.ToLowerInvariant(),
            HashSenha = SenhaHelper.GerarHash(dto.Senha),
            DataCadastro = DateTime.UtcNow,
            Ativo = true
        };

        await _usuarioRepository.AdicionarAsync(usuario);
        await _uow.SalvarAsync();

        _logger.LogInformation("Usuário cadastrado com sucesso. Id: {Id}", usuario.Id);

        await _eventPublisher.PublishAsync(new UserCreatedEvent(
            usuario.Id,
            usuario.NomeUsuario,
            usuario.Email,
            usuario.DataCadastro));

        return MapearParaDto(usuario);
    }

    public async Task<UsuarioResponseDto?> BuscarPorIdAsync(int id)
    {
        var usuario = await _usuarioRepository.BuscarPorIdAsync(id);
        return usuario is null ? null : MapearParaDto(usuario);
    }

    public async Task<IEnumerable<UsuarioResponseDto>> ListarTodosAsync()
    {
        var usuarios = await _usuarioRepository.ListarTodosAsync();
        return usuarios.Select(MapearParaDto);
    }

    public async Task<UsuarioResponseDto> AtualizarAsync(int id, UsuarioAtualizarDto dto)
    {
        var usuario = await _usuarioRepository.BuscarPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Usuário com id {id} não encontrado.");

        if (!SenhaHelper.EmailValido(dto.Email))
            throw new ArgumentException("Formato de email inválido.");

        var emailEmUso = await _usuarioRepository.BuscarPorEmailAsync(dto.Email);
        if (emailEmUso is not null && emailEmUso.Id != id)
            throw new InvalidOperationException($"O email '{dto.Email}' já está em uso por outro usuário.");

        usuario.NomeUsuario = dto.NomeUsuario;
        usuario.Email = dto.Email.ToLowerInvariant();

        await _uow.SalvarAsync();

        _logger.LogInformation("Usuário {Id} atualizado.", id);
        return MapearParaDto(usuario);
    }

    public async Task DesativarAsync(int id)
    {
        var usuario = await _usuarioRepository.BuscarPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Usuário com id {id} não encontrado.");

        usuario.Ativo = false;
        await _uow.SalvarAsync();

        _logger.LogInformation("Usuário {Id} desativado.", id);
    }

    public async Task ConcederAdminAsync(int id)
    {
        var usuario = await _usuarioRepository.BuscarPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Usuário com id {id} não encontrado.");

        if (usuario.Administrador is not null)
            throw new InvalidOperationException($"Usuário {id} já é administrador.");

        usuario.Administrador = new Administrador { UsuarioId = id };
        await _uow.SalvarAsync();

        _logger.LogInformation("Privilégio de administrador concedido ao usuário {Id}.", id);
    }

    public async Task RevogarAdminAsync(int id)
    {
        var usuario = await _usuarioRepository.BuscarPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Usuário com id {id} não encontrado.");

        if (usuario.Administrador is null)
            throw new InvalidOperationException($"Usuário {id} não é administrador.");

        usuario.Administrador = null;
        await _uow.SalvarAsync();

        _logger.LogInformation("Privilégio de administrador revogado do usuário {Id}.", id);
    }

    private static UsuarioResponseDto MapearParaDto(Usuario u) =>
        new(u.Id, u.NomeUsuario, u.Email, u.DataCadastro, u.Ativo,
            u.Administrador is not null ? "Administrador" : "Usuario");
}
