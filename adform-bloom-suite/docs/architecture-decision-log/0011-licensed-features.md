# 11. licensed-features

Date: 2020-04-30

## Status

Accepted

## Context

Additional entity called Licensed Features is needed to represent the "module" or classification of a Feature. This new entity is not bound to the Tenants directly and current scope is limited to UI at AAP.

## Decision

There were two posible solutions for such:

* Add this as a property in the feature entity and handle the grouping on the FrontEnd.
* Add this entity as a new node in the graph, and handle the grouping on the BackEnd.

Due the fact that names of this entity might update and the possibility of overlapping on Features (i.e. one Licensed Feature can contain Features of another one) we selected the second option.

## Consequences

Additional node, label and relationships will be added to our graph, there shouldn't be a major impact as this wont be retrieved on BloomRuntime.

Updates on our GraphQL schema and query handlers as we need to allow the posibility to retrieve such entity.
    
