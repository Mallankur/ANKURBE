# Adform.Bloom.Runtime

This service uses graph databases (OngDB) for representing the levels of hierarchy and access for authorization purposes. 
This service is consumed by `Adform.Bloom.Runtime.Client`. 

# How to use:

## Docker-Compose

By selecting the docker-compose orchestrator as StartUp project running the project the docker-compose.yml will set all the required dependencies.

## Project Launch

If the project lauch is set as StartUp project then the dependencies need to be handled and update the appsettings accordingly.

# Service

## Structure

- `Adform.Bloom.Runtime.Domain` Contains the core entities.
- `Adform.Bloom.Runtime.Application` Contains the abstractions and CQRS. Depends on Domain layer.
- `Adform.Bloom.Runtime.Infrastruture` Contains the implementation of Application layer. Depends on Application layer.
- `Adform.Bloom.Runtime.Host` Contains the REST and GraphQL service. Depends on Intrastructure layer.

## Deployment

- With each push the quality report is updated to https://sonarqube.adform.com/
- With each push from a feature branch automatic deploymend is triggered:
    - The release name and the ingress is set dynamically to {BRANCH_NAME}-adform-bloom-runtime
    - The TTL for feature deployment is set to 48h

## Notes

Currently the service exposes REST, however to optimize a bit further gRPC could be used for this purpose.

# Client 

## Structure

- `Adform.Bloom.Runtime.Contracts` Contains the request/response and service abstractions.
- `Adform.Bloom.Runtime.Client` Contains the HTTP Runtime Client.