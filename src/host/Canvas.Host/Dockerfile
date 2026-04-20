FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5050

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

COPY Canvas.slnx .
COPY src/host/Canvas.Host/Canvas.Host.csproj src/host/Canvas.Host/
COPY src/shared/Canvas.Common/Canvas.Common.csproj src/shared/Canvas.Common/
COPY src/shared/Canvas.Infrastructure.Shared/Canvas.Infrastructure.Shared.csproj src/shared/Canvas.Infrastructure.Shared/
RUN dotnet restore src/host/Canvas.Host/Canvas.Host.csproj

COPY src/ src/
RUN dotnet publish src/host/Canvas.Host/Canvas.Host.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Canvas.Host.dll"]
