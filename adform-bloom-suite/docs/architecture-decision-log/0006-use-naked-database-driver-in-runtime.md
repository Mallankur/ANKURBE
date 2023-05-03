# 6. use-naked-database-driver-in-runtime

Date: 2020-03-09

## Status

Accepted

## Context

Originally, to facilitate development we decided to use (.NET wrapper)[https://github.com/Readify/Neo4jClient] around Neo4j Driver. It has some nice features like the fluent API.  However, it also introduces some overhead. Additionally, the wrapper is bound to Neo4j Driver v3.5 whereas a new version is available. One of main differences is that .NET Driver v4 is asynchronous. Our tests also shown that the new drivers deals bettern with sharing load across nodes of the cluster i.e. during load tests v3.5 used only one core server of a cluster and the new driver used two.

## Decision

While Bloom Runtime must be as fast as it is possible we decided to use naked/pure Neo4j driver instead of the wrapper.

## Consequences

Bloom Runtime can take advantage of the newest Neo4j drivers. Development is a little bit more difficult but on the other hand Bloom Runtime has got just 1 query handler so it is not a big issue.