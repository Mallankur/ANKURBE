# adform-bloom-suite

## Setup

To clone all the solution plus submodules:

```shell
git clone --recursive
```

if clone without submodules in order to load them:

```shell 
git submodule update --init
```

## How To

### Seeding

Initialize the `docker-compose.yml` at the root of the project. Once all the services are running, execute the `Seed.csproj` which will seed the databases with the data for tests, which contains significant structures to represent multiple scenarios in `Bloom`.

### Environments

**NOTE:**

Is important to be aware in which environment the suite is being executed, as mutations will be perform over the database designated for the environment.

1.- The environment is defined at `docker-compose.yml` at `ASPNETCORE_ENVIRONMENT` by default is set to local environment or `Development` but it can be adjusted to `d1`, `pp`, `pr`. 
2.- After adjusting the `docker-compose.ymm` the `appsettings.{environment}.json` with the appropriate database secrets