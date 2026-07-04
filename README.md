# FCG Users API — FIAP Cloud Games Fase 2

Microsserviço de gerenciamento de usuários da plataforma **FIAP Cloud Games**. Responsável por cadastro, autenticação JWT e publicação de eventos de domínio via MassTransit/RabbitMQ.

---

## Tecnologias

| Camada | Tecnologia |
|--------|-----------|
| Runtime | .NET 9 / ASP.NET Core |
| ORM | Entity Framework Core 9 + SQL Server |
| Autenticação | JWT Bearer |
| Mensageria | MassTransit 8 + RabbitMQ |
| Testes | NUnit 4 + Moq |
| Containers | Docker + Docker Compose |
| Orquestração | Kubernetes |

---

## Arquitetura

O projeto segue **Clean Architecture** com separação estrita de camadas:

```
UsersAPI/
├── src/
│   ├── FCG.Users.Domain/        ← Entidades, regras de negócio, helpers
│   ├── FCG.Users.Application/   ← DTOs, interfaces, serviços de aplicação
│   ├── FCG.Users.Infrastructure/← EF Core, repositórios, mensageria
│   └── FCG.Users.API/           ← Controllers, middlewares, Program.cs
└── tests/
    ├── FCG.Users.Domain.Tests/
    └── FCG.Users.Application.Tests/
```

**Regra de dependência**: Domain ← Application ← Infrastructure ← API

---

## Endpoints

### Autenticação

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| POST | `/api/autenticacao` | Anônimo | Autentica e retorna JWT |

### Usuários

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| POST | `/api/usuarios` | Anônimo | Cadastra novo usuário |
| GET | `/api/usuarios` | Administrador | Lista todos os usuários |
| GET | `/api/usuarios/{id}` | Autenticado | Busca usuário por ID |
| PUT | `/api/usuarios/{id}` | Administrador | Atualiza nome e email |
| DELETE | `/api/usuarios/{id}` | Administrador | Desativa usuário (soft delete) |
| PUT | `/api/usuarios/{id}/admin` | Administrador | Concede privilégio de admin |
| DELETE | `/api/usuarios/{id}/admin` | Administrador | Revoga privilégio de admin |

---

## Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `ConnectionStrings__DefaultConnection` | String de conexão SQL Server | — |
| `Jwt__Chave` | Chave secreta para assinatura JWT | — |
| `Jwt__Emissor` | Emissor do token | `FCG-UsersAPI` |
| `Jwt__Audiencia` | Audiência do token | `FCG-Microsservicos` |
| `Jwt__ExpiracaoHoras` | Horas de validade do token | `8` |
| `RabbitMQ__Host` | Host do RabbitMQ | `rabbitmq` |
| `RabbitMQ__Username` | Usuário RabbitMQ | `guest` |
| `RabbitMQ__Password` | Senha RabbitMQ | `guest` |

---

## Como Rodar Localmente (Docker Compose)

**Pré-requisitos**: Docker Desktop instalado e em execução.

```bash
# Na raiz do projeto UsersAPI/
docker-compose up --build
```

A API estará disponível em:
- **API**: http://localhost:5001
- **Swagger**: http://localhost:5001/swagger
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

Para parar os containers:

```bash
docker-compose down
```

Para remover volumes (apaga dados do banco):

```bash
docker-compose down -v
```

---

## Como Rodar os Testes

**Pré-requisitos**: .NET 9 SDK instalado.

```bash
# Na raiz do projeto UsersAPI/
dotnet test

# Com relatório de cobertura
dotnet test --collect:"XPlat Code Coverage"

# Rodar apenas testes de domínio
dotnet test tests/FCG.Users.Domain.Tests/

# Rodar apenas testes de aplicação
dotnet test tests/FCG.Users.Application.Tests/
```

---

## Como Fazer Deploy no Kubernetes

**Pré-requisitos**: `kubectl` configurado para o cluster alvo.

### 1. Build e Push da imagem

```bash
docker build -t users-api:latest .
# Se usar registro privado:
docker tag users-api:latest SEU_REGISTRY/users-api:latest
docker push SEU_REGISTRY/users-api:latest
```

### 2. Aplicar os manifestos

Os manifestos usam o namespace `fcg` (crie-o antes, caso ainda não exista):

```bash
kubectl create namespace fcg --dry-run=client -o yaml | kubectl apply -f -

kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secret.yaml
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
```

### 3. Verificar o deploy

```bash
kubectl get pods -n fcg -l app=users-api
kubectl get services -n fcg users-api
kubectl logs -n fcg -l app=users-api --tail=50
```

### 4. Remover o deploy

```bash
kubectl delete -f k8s/
```

---

## Migrations EF Core

As migrations são aplicadas automaticamente no startup via `app.MigrateDatabase()`.

Para gerar uma nova migration manualmente:

```bash
cd src/FCG.Users.Infrastructure

dotnet ef migrations add NomeDaMigration \
  --startup-project ../FCG.Users.API \
  --context UsuariosDbContext

dotnet ef database update \
  --startup-project ../FCG.Users.API \
  --context UsuariosDbContext
```

---

## Eventos de Domínio

| Evento | Exchange/Queue | Publicado quando |
|--------|---------------|-----------------|
| `UserCreatedEvent` | Gerado pelo MassTransit | Novo usuário cadastrado com sucesso |

Payload do `UserCreatedEvent`:
```json
{
  "userId": 1,
  "nome": "João Silva",
  "email": "joao@email.com",
  "dataCadastro": "2026-06-09T12:00:00Z"
}
```

---

## Segurança

- Senhas armazenadas como hash **SHA-256** (compatível com Fase 1)
- Tokens JWT com expiração configurável (padrão 8h)
- Validações de email e complexidade de senha no serviço
- Secrets nunca commitados — use variáveis de ambiente ou Kubernetes Secrets
