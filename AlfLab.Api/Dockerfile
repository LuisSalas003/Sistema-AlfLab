# ETAPA 1: Construcción
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# 1. Copiamos solo los archivos necesarios para restaurar la solución
COPY . ./

# 2. Restauramos las dependencias usando tu archivo de Solución principal
RUN dotnet restore "Proyecto_AlfLab.sln"

# 3. Compilamos y publicamos específicamente la capa de la API
RUN dotnet publish "Alflab.Api/Alflab.Api.csproj" -c Release -o out

# ETAPA 2: Ejecución
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Traemos el código compilado
COPY --from=build /app/out .

# ¡LA LLAVE MAESTRA! Obligamos a .NET a escuchar en el puerto 80
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "Alflab.Api.dll"]