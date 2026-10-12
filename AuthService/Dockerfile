ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

COPY ["AuthService.slnx", "./"]
COPY ["NuGet.Config", "./"]
COPY ["AuthService.Api/AuthService.Api.csproj", "AuthService.Api/"]
COPY ["AuthService.Application/AuthService.Application.csproj", "AuthService.Application/"]
COPY ["AuthService.Domain/AuthService.Domain.csproj", "AuthService.Domain/"]
COPY ["AuthService.Infrastructure/AuthService.Infrastructure.csproj", "AuthService.Infrastructure/"]
COPY ["AuthService.UnitTests/AuthService.UnitTests.csproj", "AuthService.UnitTests/"]

RUN dotnet restore "AuthService.slnx"

COPY . .
RUN dotnet publish "AuthService.Api/AuthService.Api.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
RUN rm -f appsettings.Development.json

ENTRYPOINT ["dotnet", "AuthService.Api.dll"]
