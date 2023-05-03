# 15. when-to-stream

Date: 2020-12-09

## Status

Accepted

user-read-model [14. user-read-model](0014-user-read-model.md)

## Context

We had a discussion if we should stream to the read model as much as it is possible or not.

## Decision

We will stick to the following rules/hints when deciding if an entity should be streamed to the read model.

Streaming is recommended if:

1. If we deal with entities from a separate domain.
1. When dealing with lists e.g. the list of all users visible to an administrator.
1. If we need filtering, sorting etc. and there is no API that provides these functionalities for given entity.
4. For performance reasons. Usually it is faster to make calculations if all data is in one place. Especially if we need to process a few entities that comes from different domains / APIs.
5. If we only read i.e. if we read and edit a given entity, then it may be better to use one API in both cases.

## Consequences

Some entities will be streamed and consumed via read model and some will be consumed via APIs.
