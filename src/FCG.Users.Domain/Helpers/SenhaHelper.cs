using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace FCG.Users.Domain.Helpers;

public static class SenhaHelper
{
    private static readonly Regex RegexSenha = new(
        @"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[@$!%*?&_#\-\.])[A-Za-z\d@$!%*?&_#\-\.]{8,}$",
        RegexOptions.Compiled);

    private static readonly Regex RegexEmail = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string GerarHash(string senha)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
        return Convert.ToHexString(bytes).ToLower();
    }

    public static bool SenhaValida(string senha) => RegexSenha.IsMatch(senha);

    public static bool EmailValido(string email) => RegexEmail.IsMatch(email);
}
