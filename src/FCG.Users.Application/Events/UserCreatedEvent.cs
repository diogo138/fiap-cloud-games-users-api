namespace FCG.Users.Application.Events;

public record UserCreatedEvent(
    int UserId,
    string Nome,
    string Email,
    DateTime DataCadastro
);
