#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0.1-bullseye-slim AS base
COPY --from=docker.artifactory.adform.com/ca-certs:dev /etc/ssl/certs/ca-certificates.crt /etc/ssl/certs/ca-bundle.crt
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 9696

ENV GRPC_PORT=9696
ENV HTTP_PORT=80

ENV ASPNETCORE_URLS=http://+:${HTTP_PORT}

FROM mcr.microsoft.com/dotnet/sdk:6.0.100-bullseye-slim AS build
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["src/Adform.Bloom.Read.Host/Adform.Bloom.Read.Host.csproj", "Adform.Bloom.Read.Host/"]
COPY ["src/Adform.Bloom.Read.Contracts/Adform.Bloom.Read.Contracts.csproj", "Adform.Bloom.Read.Contracts/"]
COPY ["src/Adform.Bloom.Read.Application/Adform.Bloom.Read.Application.csproj", "Adform.Bloom.Read.Application/"]
COPY ["src/Adform.Bloom.Read.Infrastructure/Adform.Bloom.Read.Infrastructure.csproj", "Adform.Bloom.Read.Infrastructure/"]
COPY ["src/Adform.Bloom.Read.Domain/Adform.Bloom.Read.Domain.csproj", "Adform.Bloom.Read.Domain/"]
RUN dir #build
RUN dotnet restore "/src/Adform.Bloom.Read.Host/Adform.Bloom.Read.Host.csproj"
COPY ["./src", "."]
WORKDIR /src/Adform.Bloom.Read.Host
RUN dotnet build "Adform.Bloom.Read.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dir #publish
RUN dotnet publish "Adform.Bloom.Read.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN dir #final
COPY --from=publish  /app/publish .
ENTRYPOINT ["dotnet", "Adform.Bloom.Read.Host.dll"]