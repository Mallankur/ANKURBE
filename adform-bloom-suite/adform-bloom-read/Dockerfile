FROM docker.artifactory.adform.com/dotnet/aspnet:6.0.1-bullseye-slim
ENV GRPC_PORT=9696
ENV HTTP_PORT=80
ENV ASPNETCORE_URLS=http://+:${HTTP_PORT}

EXPOSE ${HTTP_PORT} 
EXPOSE ${GRPC_PORT}

COPY ["./output", "/app"]
WORKDIR /app

ENTRYPOINT ["dotnet", "Adform.Bloom.Read.Host.dll"]