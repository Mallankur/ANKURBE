# 2. use-graph-database

Date: 2019-11

## Status

Accepted

use-graph-database [3. use-ongdb](0003-use-ongdb.md)

## Context

We need a database that will allow us to easily store and query graphs (specifically hierarchies). Our graph will
consist of policies, tenants, roles, permissions, users, features etc. In future we also plan to store ACL records.

## Decision

Use a native graph database.

## Consequences

**Pros**

Native graph databases are designed with the aim to store graph data and because of that they are more efficient and provide better tools than relational or other kind of databases.

**Cons**

We will need to learn a new technology.
