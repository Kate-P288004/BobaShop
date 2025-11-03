# ====== Build stage ======
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy csproj first to leverage docker layer caching
COPY BobaShop.Api/*.csproj BobaShop.Api/
COPY BobaShop.sln ./
RUN dotnet restore "BobaShop.Api/BobaShop.Api.csproj" --nologo

# copy the rest
COPY . .
WORKDIR /src/BobaShop.Api
RUN dotnet publish -c Release -o /app/publish --no-restore --nologo

# ====== Runtime stage ======
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Docker

EXPOSE 8080
ENTRYPOINT ["dotnet", "BobaShop.Api.dll"]
