# syntax=docker/dockerfile:1

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o csproj e restaura dependências (melhor cache)
COPY ./APIBarbearia.csproj ./
RUN dotnet restore ./APIBarbearia.csproj

# Copia o restante do código e publica
COPY . ./
RUN dotnet publish ./APIBarbearia.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Porta interna do container (vamos mapear para 7219 no compose)
EXPOSE 7219

COPY --from=build /app/publish ./

# O ASPNETCORE_URLS será definido no docker-compose.yml
ENTRYPOINT ["dotnet", "APIBarbearia.dll"]
