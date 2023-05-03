# 12. permission-to-entitypermission

Date: 2020-04-30

## Status

Accepted

## Context

Permission entity type needs some constrains that allow to have some convetion over the naming of the entity, for that purpose some additional properties are required. The following shows the desired definition of the type.

```
type EntityPermission @key(fields: id){
    """
    Unique Id
    """
    id: ID
 
    """
    Full name: entity.function or entity.attribute.function e.g. Campaign.DELETE or Campagin.budget.CREATE
    """
    name: String!
 
    """
    Unique Entity name,
    Example: BuyerCampaign
    """
    entity: String!
 
    """
    Unique Entity name,
    Example: budget
    """
    attribute: String!
 
    """
    Example: CREATE
    """
    function: EntityFunction!
}
 
enum EntityFunction {
    CREATE
    READ
    WRITE
    DELETE
}
```

## Decision

The rename of the entity together with the changes described above need to take place.


## Consequences

Updates on our GraphQL schema and query handlers as we need to allow the posibility to retrieve this entity with all it's fields.

Some performace degradation is expected as the node size will grow and deserializing/serializing it also will consume a bit more.
    
