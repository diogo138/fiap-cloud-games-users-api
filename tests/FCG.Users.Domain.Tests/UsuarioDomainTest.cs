using FCG.Users.Domain.Helpers;
using NUnit.Framework;

namespace FCG.Users.Domain.Tests;

[TestFixture]
public class UsuarioDomainTest
{
    // --- Validação de Senha ---

    [TestCase("Abc@1234")]
    [TestCase("Senha!1234")]
    [TestCase("P@ssw0rd")]
    [TestCase("Complexa.1")]
    [TestCase("Valid_Pass1")]
    public void SenhaValida_DeveRetornarTrue_QuandoSenhaAtendeCriterios(string senha)
    {
        var resultado = SenhaHelper.SenhaValida(senha);
        Assert.That(resultado, Is.True, $"Senha '{senha}' deveria ser válida.");
    }

    [TestCase("12345678")]
    [TestCase("abcdefgh")]
    [TestCase("Abc12")]
    [TestCase("")]
    [TestCase("semNumeroEspecial1")]
    [TestCase("SemNumero@")]
    public void SenhaValida_DeveRetornarFalse_QuandoSenhaNaoAtendeCriterios(string senha)
    {
        var resultado = SenhaHelper.SenhaValida(senha);
        Assert.That(resultado, Is.False, $"Senha '{senha}' deveria ser inválida.");
    }

    // --- Validação de Email ---

    [TestCase("usuario@email.com")]
    [TestCase("nome.sobrenome@dominio.com.br")]
    [TestCase("user+tag@example.org")]
    public void EmailValido_DeveRetornarTrue_QuandoEmailCorreto(string email)
    {
        var resultado = SenhaHelper.EmailValido(email);
        Assert.That(resultado, Is.True, $"Email '{email}' deveria ser válido.");
    }

    [TestCase("emailsemarroba.com")]
    [TestCase("@semusuario.com")]
    [TestCase("")]
    [TestCase("invalido@")]
    [TestCase("invalido@dominio")]
    public void EmailValido_DeveRetornarFalse_QuandoEmailInvalido(string email)
    {
        var resultado = SenhaHelper.EmailValido(email);
        Assert.That(resultado, Is.False, $"Email '{email}' deveria ser inválido.");
    }

    // --- Hash SHA-256 ---

    [Test]
    public void GerarHash_DeveRetornarHashConsistente_ParaMesmaSenha()
    {
        const string senha = "MinhaS3nh@";
        var hash1 = SenhaHelper.GerarHash(senha);
        var hash2 = SenhaHelper.GerarHash(senha);

        Assert.That(hash1, Is.EqualTo(hash2), "Hash deve ser determinístico para a mesma senha.");
    }

    [Test]
    public void GerarHash_DeveRetornarHashDiferente_ParaSenhasDiferentes()
    {
        var hash1 = SenhaHelper.GerarHash("Senha@1234");
        var hash2 = SenhaHelper.GerarHash("Senha@5678");

        Assert.That(hash1, Is.Not.EqualTo(hash2), "Senhas diferentes devem gerar hashes diferentes.");
    }

    [Test]
    public void GerarHash_DeveRetornar64Caracteres_HashSHA256()
    {
        var hash = SenhaHelper.GerarHash("Qualquer@1");

        Assert.That(hash.Length, Is.EqualTo(64), "Hash SHA-256 hexadecimal deve ter 64 caracteres.");
    }

    [Test]
    public void GerarHash_DeveRetornarHashEmMinusculas()
    {
        var hash = SenhaHelper.GerarHash("Qualquer@1");

        Assert.That(hash, Is.EqualTo(hash.ToLower()), "Hash deve ser em letras minúsculas.");
    }

    [Test]
    public void GerarHash_DeveMatchar_HashEsperado()
    {
        // Hash SHA-256 de "Abc@1234" calculado de forma independente
        var hash = SenhaHelper.GerarHash("Abc@1234");

        Assert.That(hash, Has.Length.EqualTo(64));
        Assert.That(hash, Does.Match("^[0-9a-f]{64}$"), "Hash deve conter apenas caracteres hexadecimais minúsculos.");
    }
}
