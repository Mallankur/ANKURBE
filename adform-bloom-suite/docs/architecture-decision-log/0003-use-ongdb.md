# 3. use-ongdb

Date: 2019-11

## Status

Accepted

use-graph-database [2. use-graph-database](0002-use-graph-database.md)

## Context

We need to select a graph database that will be used to build IAM.

## Decision

We tried:

- **Janus** – it was rejected due to poor .NET client library
- **Arrango** – we tried to make a proof of concept based on Arrango but it turned out that it is not so easy. Due to lack of time we decided not to coninue.

We also considered but didn't try to use them:
 
- **AgensGraph** – it is a graph database built based on Postgres. It supports Cypher what is good. However, it is not a “native” graph database.
- **TigerGraph** – from what we know it is expensive. According to benchmarks (from the vendor) it is also faster than Neo4j. Both things should be confirmed.
- **DGraph** – quite a new solution developed by former Google developers. According to benchmarks (from the vendor) it is faster than Neo4j. What is nice it has scalability feature in the community (open source) version. There is also an enterprise version which starts from 6000$ per year. In the case of Adform it rather would be 18000$ a year because we are interested in HA solution. After very initial analysis it seems the best alternative for ONgDB/Neo4j.
- **MongoDB** - it has support for graph data (see *$graphLookup*) since version 3.4 but it is not a “native” database.

So finally we decided to continue with the open source version of Neo4j (which is one of the most mature solutions in the market) called ONgDB. ONgDB is consistent with Neo4j v3.5 Enterprise. 

## Consequences

**Pros**

- Very mature/old technology.
- Good client libraries.
- Good documentation/tutorials.
- A community that answers questions quickly (we checked that in practice).
- Easy start
- Easy to learn query language i.e. Cypher.
- It is open-source.
- We have some Neo4j/ONgDB skills in the team.

**Cons**

- It is “only” a fork of Neo4j v3.4 Enterprise that was updated to v3.5 Enterprise and it has no official support from Neo4j guys.
- There is no guarantee of updates and patches for ONgDB. 
- No documentation dedicated to ONgDB. However, in practice Neo4j documentation is enough.
