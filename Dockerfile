# Imagen base para runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Imagen para build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia archivo de proyecto y restaura dependencias
COPY ["EcommerceAPI.csproj", "./"]
RUN dotnet restore "EcommerceAPI.csproj"

# Copia el resto de archivos y construye
COPY . .
RUN dotnet build "EcommerceAPI.csproj" -c Release -o /app/build

# Publica la aplicación
FROM build AS publish
RUN dotnet publish "EcommerceAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EcommerceAPI.dll"]