# Adform Bloom

This service is responsible for read and write of the authorization graph. It uses  `Adform.Bloom.Client` to enhance the identity of the user. 

# How to use:

## Docker-Compose

By selecting the docker-compose orchestrator as StartUp project running the project the docker-compose.yml will set all the required dependencies.

## Project Launch

If the project lauch is set as StartUp project then the dependencies need to be handled and update the appsettings accordingly.

# Service

## Dependencies

- `Adform.Bloom.Runtime`
- `Adform.Bloom.ReadModel`

## Structure

- `Adform.Bloom.Api` Contains the REST/GraphQL service.
- `Adform.Bloom.Contracts` Contains the contracts for mapping.
- `Adform.Bloom.DataAccess` Contains the repositories and decorators.
- `Adform.Bloom.Domain` Contains the interfaces and value objects.
- `Adform.Bloom.Messages` Contains the events and commands.
- `Adform.Bloom.Infrastructure` Contains the models.
- `Adform.Bloom.Mediatr.Extensions` Contains the decorators for mediator.
- `Adform.Bloom.Read` Contains the queries and handlers.
- `Adform.Bloom.Write` Contains the command and handlers.
- `Adform.Bloom.Seeder` Contains seeder for performance tests.

## Deployment

- With each push the quality report is updated to https://sonarqube.adform.com/
- With each push from a feature branch automatic deploymend is triggered:
    - The release name and the ingress is set dynamically to {BRANCH_NAME}-adform-bloom
    - The TTL for feature deployment is set to 48h

## Notes

Currently the service exposes REST, however to optimize a bit further gRPC could be used.