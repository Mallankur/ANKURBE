# 19. product-to-policy-mapping

Date: 2022-07-20

## Status

Accepted

## Context

Licensed Features need to be scoped per Product, which will result in scoping of Features, Roles and Permissions.

## Decision

To avoid deterioration on `BloomRuntime` performance and to keep the same set of relationships and nodes that exist in the current system, the way

## Consequences

A pathway needs to exist between `Licensed Feature -> Feature -> Permission <- Role <- Policy`. To assert such path ideally the system should naturally created, however while seeding it was noticed that some `Permissions` are not assigned to any `Template Role`. To amend the situation an admin `System Role` is to be created per `Product`.

Fixes on `BloomETL` needed. Genesis nodes for `Policy` need to be created that will represent the products in the system.

Fixes on `Bloom` possibility to query and filter `Licensed Features` by `Product`.

Fixes on `BloomRead` modify `BusinessAccount` view to return `Available Products` which is an array with static configuration per `Business Account` type.


