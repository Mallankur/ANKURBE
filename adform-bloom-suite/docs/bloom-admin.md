# Bloom Administration API

This service is responsible of all the mutations to the graph and the visibility management on the queries.

## Flavors

<figure class="thumbnails">
    <img src="assets/img/graphql-admin.png" alt="GraphQL" title="GraphQL">
    <img src="assets/img/swagger-admin.png" alt="Swagger" title="Swagger">
</figure>

### GraphQL

GraphQL server allows Query and Mutation over all the entities (Tenant, Subject, Policy, Role, Permission, Feature, Licensed Features).

#### Query [WIP]

**Payload**:



#### Mutation [WIP]

**Payload**:

```json
mutation CreatePermission($permission: PermissionInput!) {
  createPermission(permission: $permission) {
    name,
    description
  }
}
```

**Response**:

```json
{
  "data": {
    "createPermission": {
      "name": "demo",
      "description": null
    }
  }
}
```