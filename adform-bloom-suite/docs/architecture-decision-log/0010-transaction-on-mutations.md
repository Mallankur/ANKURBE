# 10. transaction-on-mutations

Date: 2020-04-30

## Status

Accepted

## Context

In some cases it is needed to execute 2 or more mutations in one batch for example create role and assign features.

## Decision

If transactional semantic is needed (e.g. create role and assign features must be executed in atomic way), then due the current design and limitations of graphql-dotnet a new mutation needs to be created. This new mutation under the hood should execute all operations in transactional way.

```
type mutation {
    createRoleAndAssignFeatures(
        role: Role!
        features: [Features!]
    ) : Boolean
}
```
If there is a need to batch mutations and at the same time transactional semantic is not needed, then no new mutation is required as these ones are not dependant on each other and the outcome of one mutation won't alter the outcome of other (even if one mutation in a batch fails it does not affect other mutations). For example it is posisble to remove many roles at once from UI. In this case we need to batch 2 or more mutations but we need no transaction. If it is not possible to remove 1 or more roles it is acceptable

```
type mutation {
    deleteRole(
        roleId: ID!
    ) : Boolean
    deleteRole(
        roleId: ID!
    ) : Boolean
}
```

See also [GraphQL Bulk Operations on .NET](https://adform.atlassian.net/wiki/spaces/IAM/pages/1290142123/2020-04-22+GraphQL+bulk+operations/)

## Consequences

Additional mutation will be added in cases where we need transactions for such batches.
    
