# ====== Build stage ======
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY BobaShop.Web/*.csproj BobaShop.Web/
COPY BobaShop.sln ./
RUN dotnet restore "BobaShop.Web/BobaShop.Web.csproj" --nologo

COPY . .
WORKDIR /src/BobaShop.Web
RUN dotnet publish -c Release -o /app/publish --no-restore --nologo

# ====== Runtime stage ======
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8081 \
    ASPNETCORE_ENVIRONMENT=Docker

EXPOSE 8081
ENTRYPOINT ["dotnet", "BobaShop.Web.dll"]
