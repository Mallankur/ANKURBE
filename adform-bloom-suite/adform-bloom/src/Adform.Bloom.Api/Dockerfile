#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0.1-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ENV PORT=80

FROM mcr.microsoft.com/dotnet/sdk:6.0.100-bullseye-slim AS build
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["src/Adform.Bloom.Api/Adform.Bloom.Api.csproj", "Adform.Bloom.Api/"]
COPY ["src/Adform.Bloom.Read/Adform.Bloom.Read.csproj", "Adform.Bloom.Read/"]
COPY ["src/Adform.Bloom.DataAccess/Adform.Bloom.DataAccess.csproj", "Adform.Bloom.DataAccess/"]
COPY ["src/Adform.Bloom.Infrastructure/Adform.Bloom.Infrastructure.csproj", "Adform.Bloom.Infrastructure/"]
COPY ["src/Adform.Bloom.Domain/Adform.Bloom.Domain.csproj", "Adform.Bloom.Domain/"]
COPY ["src/Adform.Bloom.Contracts/Adform.Bloom.Contracts.csproj", "Adform.Bloom.Contracts/"]
COPY ["src/Adform.Bloom.Mediatr.Extensions/Adform.Bloom.Mediatr.Extensions.csproj", "Adform.Bloom.Mediatr.Extensions/"]
COPY ["src/Adform.Bloom.Write/Adform.Bloom.Write.csproj", "Adform.Bloom.Write/"]
COPY ["src/Adform.Bloom.Messages/Adform.Bloom.Messages.csproj", "Adform.Bloom.Messages/"]
RUN dir #build
RUN dotnet restore "/src/Adform.Bloom.Api/Adform.Bloom.Api.csproj"
COPY ["./src", "."]
WORKDIR /src/Adform.Bloom.Api
RUN dotnet build "Adform.Bloom.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dir #publish
RUN dotnet publish "Adform.Bloom.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN dir #final
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Adform.Bloom.Api.dll"]
