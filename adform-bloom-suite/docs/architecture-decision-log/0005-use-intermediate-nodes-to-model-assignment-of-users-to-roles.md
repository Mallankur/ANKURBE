# 5. use-intermediate-nodes-to-model-assignment-of-users-to-roles

Date: 2020-01

## Status

Accepted

## Context

Let’s think about the following example:

- *John Smit* works for *IKEA PL* and *IKEA EN*.

- Both *IKEA PL* and IKEA EN inherit roles from *IKE*.

- *IKEA* defines 2 roles e.g. *USER* and *ADMI*.

- *John Smith* is *ADMIN* for *IKEA PL* and *IKEA EN*.

Now we want to assign *John Smith* to *ADMIN* but only for *IKEA PL*. It can be implemented in a few ways:

1. We connect *John Smith* node with *ADMIN* node and we add a label to an edge that denotes that it is an assignment in the context of *IKEA PL*.

1. We connect *John Smith* node with *ADMIN* node and we add a property to an edge e.g. a context that will store the context of an assignment i.e. *IKEA PL*.

1. We make a copy of *ADMIN* node for reach tenant tree. Then we connect *John Smith* node with *ADMIN* copy for *IKEA PL*.

1. We add an additional intermediate node representing *ADMIN* role for each tenant. Then we connect John Smith with this node for *IKEA PL.*

Now let’s consider the pros and cons of all solutions:

1. It is very intuitive. The problem is that Neo4j ha limit of around 64 thousands of labels for edges. We anticipate having more tenants.

1. This one is also simple but here the main problem is that properties cannot be indexed.

1. The main drawback here is a necessity to maintain a potentially very large number of copies of roles. Even if we implement a kind of lazy binding there is still a problem with the propagation of changes.

1. This solution is less intuitive but has no issues of solutions 1, 2 and 3 and it is why it was implemented.

## Decision

It was decided to introduce intermediate nodes of type *Group*. The side effect is that there is no direct relationship between *Subject* and *Tenant* nodes.

## Consequences

We can maintain the context of assignment of users to roles in efficient way.

