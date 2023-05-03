# 8. use-rich-domain-model-and-hexagonal-architecture

Date: 2020-04-16

## Status

Accepted

## Context

We have more and more business logic (now it is mainly related to validation). Currently this logic is placed in command handlers. It means that domain logic code is mixed with infrastructure code to some extent. It is not a perfect solution while changes in infrastructure, like data access, can affect domain logic. Writing unit tests is also not so easy. In future when we have even more business logic situation will be even more difficult.

## Decision

We will have a dedicated project called Adform.Bloom.Domain that will only contain business logic and will have no references to any other project.

## Consequences

Domain logic will be separated from the infrastructure like data access and will be centralized in ONE place. Writing unit test of domain logic will be also very easy. Besides we will have clear separation of concerns.
