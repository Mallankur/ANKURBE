# 9. use-data-loaders

Date: 2020-04-20

## Status

Accepted

## Context

For GraphQL queries which include nested types like: 

```
type Policy {
  id: ID!
  roles: [Role!]
}
```

returning N Policies will require 1 query to get all Policies and N queries to get Roles for each Policy. We need a solution that would allow us to batch together many queries (in this case Roles queries) into a single one.

## Decision

We will use data loaders from [GraphQL .NET](https://graphql-dotnet.github.io/docs/guides/dataloader/)

## Consequences

Similar operations will be batched together and executed with a single query to the database.
    