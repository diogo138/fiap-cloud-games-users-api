FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/FCG.Users.API/FCG.Users.API.csproj", "src/FCG.Users.API/"]
COPY ["src/FCG.Users.Application/FCG.Users.Application.csproj", "src/FCG.Users.Application/"]
COPY ["src/FCG.Users.Domain/FCG.Users.Domain.csproj", "src/FCG.Users.Domain/"]
COPY ["src/FCG.Users.Infrastructure/FCG.Users.Infrastructure.csproj", "src/FCG.Users.Infrastructure/"]

RUN dotnet restore "src/FCG.Users.API/FCG.Users.API.csproj"

COPY . .

WORKDIR "/src/src/FCG.Users.API"
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FCG.Users.API.dll"]
