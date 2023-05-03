# 16. role-cloning-feature-handling

Date: 2021-04-26

## Status

Accepted

## Context

We had a discussion on how we should approach `role` cloning from `features` perspective. When cloning a `role` we are capable of:

* changing `business account`
* changing the `features`/capabilities of a `role`

However, because of the first point we encounter a complex scenario:

* business account doesn't have access to the same `features` that the original `role` had.

## Decision

Front-end will keep simple UI (reuse the UX flow of `role` creation), in this way, at the moment of cloning a role and commiting the changes UI expects a detail error of the missing features or the lack of access to an specific feaute. With this we provide transparency to the users during the cloning process

## Consequences

Back-end erros must be enriched to provide the specific `features` that are not accesible for a `business account`.

