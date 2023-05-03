# Bloom Read
This services consumes PSQL data that is sinked by DDP. It is exposed via gRPC and it is the read model on which Bloom leverages to add types to it's graphql schema. 

# How to use:

## Docker-Compose

By selecting the docker-compose orchestrator as StartUp project running the project the docker-compose.yml will set all the required dependencies.

## Project Launch

If the project lauch is set as StartUp project then the dependencies need to be handled and update the appsettings accordingly.

# Service

## Structure

- `Adform.Bloom.Read.Contracts` Contains the gRPC contracts.
- `Adform.Bloom.Read.DataAccess` Contains the repository layer.
- `Adform.Bloom.Read.Grpc` Contains gRPC service.
- `Adform.Bloom.Read.Infrastructure` Contains entities.
- `Adform.Bloom.Read.Query` Contains the queries and handlers.

## Deployment

- With each push the quality report is updated to https://sonarqube.adform.com/
- With each push from a feature branch automatic deploymend is triggered:
    - The release name and the ingress is set dynamically to {BRANCH_NAME}-adform-bloom-read
    - The TTL for feature deployment is set to 48h