FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["HongarijePlanner.Api/HongarijePlanner.Api.csproj", "HongarijePlanner.Api/"]
RUN dotnet restore "HongarijePlanner.Api/HongarijePlanner.Api.csproj"
COPY . .
WORKDIR "/src/HongarijePlanner.Api"
RUN dotnet build "HongarijePlanner.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HongarijePlanner.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HongarijePlanner.Api.dll"]
