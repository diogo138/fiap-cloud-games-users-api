using FCG.Users.Application.DTOs;
using FCG.Users.Application.Events;
using FCG.Users.Application.Interfaces;
using FCG.Users.Application.Services;
using FCG.Users.Domain.Entities;
using FCG.Users.Domain.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FCG.Users.Application.Tests;

[TestFixture]
public class UsuarioServiceTest
{
    private Mock<IUsuarioRepository> _repositoryMock = null!;
    private Mock<IUnidadeDeTrabalho> _uowMock = null!;
    private Mock<IEventPublisher> _eventPublisherMock = null!;
    private Mock<ILogger<UsuarioService>> _loggerMock = null!;
    private UsuarioService _service = null!;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IUsuarioRepository>();
        _uowMock = new Mock<IUnidadeDeTrabalho>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _loggerMock = new Mock<ILogger<UsuarioService>>();

        _service = new UsuarioService(
            _repositoryMock.Object,
            _uowMock.Object,
            _eventPublisherMock.Object,
            _loggerMock.Object);
    }

    // --- CadastrarAsync ---

    [Test]
    public async Task CadastrarAsync_DeveRetornarArgumentException_QuandoEmailInvalido()
    {
        var dto = new UsuarioNovoDto("JoaoSilva", "email-invalido", "Senha@1234");

        var ex = Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.CadastrarAsync(dto));

        Assert.That(ex!.Message, Does.Contain("email"));
    }

    [Test]
    public async Task CadastrarAsync_DeveRetornarArgumentException_QuandoSenhaFraca()
    {
        var dto = new UsuarioNovoDto("JoaoSilva", "joao@email.com", "123456");

        var ex = Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.CadastrarAsync(dto));

        Assert.That(ex!.Message, Does.Contain("Senha"));
    }

    [Test]
    public async Task CadastrarAsync_DeveRetornarArgumentException_QuandoSenhaSemEspecial()
    {
        var dto = new UsuarioNovoDto("JoaoSilva", "joao@email.com", "Senha1234");

        var ex = Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.CadastrarAsync(dto));

        Assert.That(ex!.Message, Does.Contain("Senha"));
    }

    [Test]
    public async Task CadastrarAsync_DeveRetornarInvalidOperationException_QuandoEmailDuplicado()
    {
        var dto = new UsuarioNovoDto("JoaoSilva", "joao@email.com", "Senha@1234");

        _repositoryMock.Setup(r => r.EmailExisteAsync("joao@email.com"))
            .ReturnsAsync(true);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.CadastrarAsync(dto));

        Assert.That(ex!.Message, Does.Contain("joao@email.com"));
    }

    [Test]
    public async Task CadastrarAsync_DeveRetornarUsuarioResponseDto_QuandoDadosValidos()
    {
        var dto = new UsuarioNovoDto("JoaoSilva", "joao@email.com", "Senha@1234");

        _repositoryMock.Setup(r => r.EmailExisteAsync("joao@email.com"))
            .ReturnsAsync(false);

        _repositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<Usuario>()))
            .Callback<Usuario>(u => u.Id = 1)
            .Returns(Task.CompletedTask);

        _uowMock.Setup(u => u.SalvarAsync()).ReturnsAsync(1);

        _eventPublisherMock.Setup(e => e.PublishAsync(It.IsAny<UserCreatedEvent>()))
            .Returns(Task.CompletedTask);

        var resultado = await _service.CadastrarAsync(dto);

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado.NomeUsuario, Is.EqualTo("JoaoSilva"));
        Assert.That(resultado.Email, Is.EqualTo("joao@email.com"));
        Assert.That(resultado.Ativo, Is.True);
        Assert.That(resultado.Role, Is.EqualTo("Usuario"));
    }

    [Test]
    public async Task CadastrarAsync_DevePublicarUserCreatedEvent_QuandoCadastroSucesso()
    {
        var dto = new UsuarioNovoDto("JoaoSilva", "joao@email.com", "Senha@1234");

        _repositoryMock.Setup(r => r.EmailExisteAsync("joao@email.com"))
            .ReturnsAsync(false);

        _repositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<Usuario>()))
            .Callback<Usuario>(u => u.Id = 42)
            .Returns(Task.CompletedTask);

        _uowMock.Setup(u => u.SalvarAsync()).ReturnsAsync(1);

        _eventPublisherMock.Setup(e => e.PublishAsync(It.IsAny<UserCreatedEvent>()))
            .Returns(Task.CompletedTask);

        await _service.CadastrarAsync(dto);

        _eventPublisherMock.Verify(
            e => e.PublishAsync(It.Is<UserCreatedEvent>(ev =>
                ev.UserId == 42 &&
                ev.Email == "joao@email.com")),
            Times.Once);
    }
}

[TestFixture]
public class AutenticacaoServiceTest
{
    private Mock<IUsuarioRepository> _repositoryMock = null!;
    private Mock<ILogger<AutenticacaoService>> _loggerMock = null!;
    private AutenticacaoService _service = null!;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IUsuarioRepository>();
        _loggerMock = new Mock<ILogger<AutenticacaoService>>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Chave"] = "FCG-Fase2-JWT-SharedKey-2026-FIAP!@#$%^&",
                ["Jwt:Emissor"] = "FCG-UsersAPI",
                ["Jwt:Audiencia"] = "FCG-Microsservicos",
                ["Jwt:ExpiracaoHoras"] = "8"
            })
            .Build();

        _service = new AutenticacaoService(
            _repositoryMock.Object,
            configuration,
            _loggerMock.Object);
    }

    [Test]
    public async Task AutenticarAsync_DeveRetornarNull_QuandoUsuarioNaoEncontrado()
    {
        _repositoryMock.Setup(r => r.BuscarPorEmailAsync("naoexiste@email.com"))
            .ReturnsAsync((Usuario?)null);

        var resultado = await _service.AutenticarAsync(new LoginDto("naoexiste@email.com", "Qualquer@1"));

        Assert.That(resultado, Is.Null);
    }

    [Test]
    public async Task AutenticarAsync_DeveRetornarNull_QuandoSenhaIncorreta()
    {
        var usuario = new Usuario
        {
            Id = 1,
            NomeUsuario = "JoaoSilva",
            Email = "joao@email.com",
            HashSenha = SenhaHelper.GerarHash("Senha@Correta1"),
            Ativo = true
        };

        _repositoryMock.Setup(r => r.BuscarPorEmailAsync("joao@email.com"))
            .ReturnsAsync(usuario);

        var resultado = await _service.AutenticarAsync(new LoginDto("joao@email.com", "SenhaErrada@1"));

        Assert.That(resultado, Is.Null);
    }

    [Test]
    public async Task AutenticarAsync_DeveRetornarNull_QuandoUsuarioInativo()
    {
        var usuario = new Usuario
        {
            Id = 1,
            Email = "joao@email.com",
            HashSenha = SenhaHelper.GerarHash("Senha@1234"),
            Ativo = false
        };

        _repositoryMock.Setup(r => r.BuscarPorEmailAsync("joao@email.com"))
            .ReturnsAsync(usuario);

        var resultado = await _service.AutenticarAsync(new LoginDto("joao@email.com", "Senha@1234"));

        Assert.That(resultado, Is.Null);
    }

    [Test]
    public async Task AutenticarAsync_DeveRetornarTokenResponseDto_QuandoCredenciaisValidas()
    {
        const string senhaOriginal = "Senha@1234";
        var usuario = new Usuario
        {
            Id = 1,
            NomeUsuario = "JoaoSilva",
            Email = "joao@email.com",
            HashSenha = SenhaHelper.GerarHash(senhaOriginal),
            Ativo = true,
            Administrador = null
        };

        _repositoryMock.Setup(r => r.BuscarPorEmailAsync("joao@email.com"))
            .ReturnsAsync(usuario);

        var resultado = await _service.AutenticarAsync(new LoginDto("joao@email.com", senhaOriginal));

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.NomeUsuario, Is.EqualTo("JoaoSilva"));
        Assert.That(resultado.Email, Is.EqualTo("joao@email.com"));
        Assert.That(resultado.Role, Is.EqualTo("Usuario"));
        Assert.That(resultado.Token, Is.Not.Null.And.Not.Empty);
        Assert.That(resultado.Expiracao, Is.GreaterThan(DateTime.UtcNow));
    }

    [Test]
    public async Task AutenticarAsync_DeveRetornarRoleAdministrador_QuandoUsuarioAdmin()
    {
        const string senhaOriginal = "Admin@1234";
        var usuario = new Usuario
        {
            Id = 2,
            NomeUsuario = "AdminUser",
            Email = "admin@email.com",
            HashSenha = SenhaHelper.GerarHash(senhaOriginal),
            Ativo = true,
            Administrador = new Administrador { UsuarioId = 2 }
        };

        _repositoryMock.Setup(r => r.BuscarPorEmailAsync("admin@email.com"))
            .ReturnsAsync(usuario);

        var resultado = await _service.AutenticarAsync(new LoginDto("admin@email.com", senhaOriginal));

        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.Role, Is.EqualTo("Administrador"));
    }
}
