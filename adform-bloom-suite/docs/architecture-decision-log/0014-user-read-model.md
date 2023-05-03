# 14. user-read-model

Date: 2020-09-24

## Status

Accepted

user-read-model [15. when-to-stream](0015-when-to-stream.md)

## Context

Currently `Auth` handles the entity `User` and due the requirements of AAP, now known as Flow, project we are in the need for a read model of this entity for UI purposes.
It was decided that `Auth` will make the DDP contracts for this purpose, and we are required to do the following:

* Select a DDP compliant database
* Create a DDP sink for such database
* Consume the sinked data.


## Decision

Some decisions where made after the design meeting:

* Database selected for sinking DDP is PostgreSQL. As is complaint with DDP and allows fulltext search.
* A separate service is to be created to consume PostgreSQL data

Due the fact that:

1. The new service needs to be used by Bloom.
2. The new service needs to have low latency in the communication with bloom so GRPC is encouraged.
3. Bloom schema should be adjusted, `Subject` type is to be replaced with a new type `User`. This is due the fact that `Subject` is supposed to be a reference of the `User` entity only to be used by Bloom for business rules, therefore `User` should be the entity exposed at graphql level.

