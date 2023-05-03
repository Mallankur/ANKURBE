#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0.1-bullseye-slim AS base
COPY --from=docker.artifactory.adform.com/ca-certs:dev /etc/ssl/certs/ca-certificates.crt /etc/ssl/certs/ca-bundle.crt
WORKDIR /app
EXPOSE 80
EXPOSE 443

ENV PORT=80
#ENV ASPNETCORE_URLS=http://+:${PORT}

FROM mcr.microsoft.com/dotnet/sdk:6.0.100-bullseye-slim AS build
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["src/Adform.Bloom.Runtime.Host/Adform.Bloom.Runtime.Host.csproj", "Adform.Bloom.Runtime.Host/"]
COPY ["src/Adform.Bloom.Runtime.Infrastructure/Adform.Bloom.Runtime.Infrastructure.csproj", "Adform.Bloom.Runtime.Infrastructure/"]
COPY ["src/Adform.Bloom.Runtime.Application/Adform.Bloom.Runtime.Application.csproj", "Adform.Bloom.Runtime.Application/"]
COPY ["src/Adform.Bloom.Runtime.Domain/Adform.Bloom.Runtime.Domain.csproj", "Adform.Bloom.Runtime.Domain/"]
RUN dir #build
RUN dotnet restore "/src/Adform.Bloom.Runtime.Host/Adform.Bloom.Runtime.Host.csproj"
COPY ["./src", "."]
WORKDIR /src/Adform.Bloom.Runtime.Host
RUN dotnet build "Adform.Bloom.Runtime.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dir #publish
RUN dotnet publish "Adform.Bloom.Runtime.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN dir #final
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Adform.Bloom.Runtime.Host.dll"]