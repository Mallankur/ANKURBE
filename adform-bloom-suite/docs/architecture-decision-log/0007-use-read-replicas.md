# 7. use-read-replicas

Date: 2020-03-09

## Status

Accepted

## Context

Neo4j / OngDB distinguishes 2 types of nodes in the cluster i.e. core servers and read-replicas. The main goal of core servers is to provide HA whereas the main goal of read-replicas is to scale up read operations.

## Decision

We will use read replicas to scale read operations.

## Consequences

Theoretically we can scale up read operations as much as we want. On the other hand infrastructure becomes more complex while there are more servers to maintain. However, it should be noted that read-replicas are actually disposable. When read-replica is down, no data is lost. Besides, adding a new read-replica is straighforward.
