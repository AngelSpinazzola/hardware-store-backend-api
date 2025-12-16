# Imagen base para runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Imagen para build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia la solución y los archivos de proyecto
COPY ["EcommerceAPI.sln", "./"]
COPY ["src/HardwareStore.Domain/HardwareStore.Domain.csproj", "src/HardwareStore.Domain/"]
COPY ["src/HardwareStore.Application/HardwareStore.Application.csproj", "src/HardwareStore.Application/"]
COPY ["src/HardwareStore.Infrastructure/HardwareStore.Infrastructure.csproj", "src/HardwareStore.Infrastructure/"]
COPY ["src/HardwareStore.API/HardwareStore.API.csproj", "src/HardwareStore.API/"]

# Restaura dependencias
RUN dotnet restore "EcommerceAPI.sln"

# Copia el resto del código
COPY . .

# Build del proyecto
WORKDIR "/src/src/HardwareStore.API"
RUN dotnet build "HardwareStore.API.csproj" -c Release -o /app/build

# Publica la aplicación
FROM build AS publish
RUN dotnet publish "HardwareStore.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HardwareStore.API.dll"]
