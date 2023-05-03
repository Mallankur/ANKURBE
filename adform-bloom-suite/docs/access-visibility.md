
# Access and Visibility

Example:

1. Visibility

A `Subject` can request `Subject[]` that exist under `Tenant[]` to which he has `Visibility`.
NOTE: This level of hierarchy can only be of 1 level for security and the role of the user must be `Local Admin` or `Adform Admin`.
```graphql
{
    subjects(pagination: Pagination){
        data 
        {
            id
        }
    }
}
```

2. Access

When evaluating an `Subject` the nested types need to show the direct `Access` not the `Visibility` for security purposes.

```graphql
{
    subject(id: ID){
        id
        name
        //roles that user id assigned to
        roles(pagination:Pagination){
            data 
            {
                id
                permissions
                {
                    
                }
            }
        }
        //tenants that user is assigned to via a role
        tenants(pagination:Pagination){
            data 
            {
                id
            }
        }
    }
}
```




> **Query**  
>
> * In a query the `Actor` and `Subject` are the same.
> * A query can be reach by a `Subject`

> **Mutation**
> * In a mutation the `Actor` and `Subject` are different in some scenarios for security reasons.
> * `Actor`=!`Subject` in operations like assignments as the `Actor` cannot modify it's own `Permission`.
> * `Actor`==`Subject` in operations like update of nodes like `Subject`

# Entities

---

<details>
<summary>Policy</summary>

> `Policies` are used to group `roles`, they allow to reduce the space of search.

## Query

### `policy(id:ID)`

Get a `Policy` by id.

#### Validation

`Policy` can be listed by `AdformAdmin` only.

#### Nested types

* `Roles`: Display the `Roles` that are contained in a `Policy`.

### `policies(pagination: Pagination)`

Get a list of `Policies`, it allows pagination, sorting and filtering.

#### Validation

`Policies` can be listed by `AdformAdmin` only.

#### Nested types

* `Roles`: Display the `Roles` that are contained in a `Policy`.

---

## Mutation

### `createPolicy` 

Creates a new `Policy` in the graph. To place it on different levels the `parentId` field is used.

#### Validation

Only `AdformAdmin` can create a `Policy`.

### `deletePolicy`

Deletes a `Policy` from the graph using the `id` field.

#### Validation

Only `AdformAdmin` can delete a `Policy`.

</details>

---

<details>
<summary>Role</summary>

>`Roles` are categorized as:
> * `Custom Role`: Belong and are own by a `Tenant`. They are cloned from a `Template Role`, and they are managed by the `Tenant` who owns them.
> * `Template Role`: Belong and are own by a `Adform:Tenant`. They are also known as `System Role` and they are managed by the `Adform:Tenant`.
> * `Ghost Role (coming soon)`: Belong and are own by a `Tenant`. They are used to represent the belonging of a `Subject` to a `Tenant` (Business account invitation).
>
>There are special `Roles` that modify the behaviour of the system:
> * `AdformAdmin`: Is the role that has the higher visibility and accessibility of all, meaning that a `Subject` assigned to this `Role` can perform all the mutations and queries in the graph.
> * `LocalAdmin`: Is the role that is assigned to the owner of a `Tenant` it allows to query `Resources` that belong to the `Tenant` and mutate them as well.

## Query

### `role(id:ID)`

Get a `Role` by id.

#### Validation

- `Template Role` can be visible by a `Subject` always.
- `Custom Role` can be visible by a `Subject` if:
    - The `Subject` is `AdformAdmin`.
    - The `Subject` is in the `Tenant` who owns the `Custom Role`.
    - The `Subject` is in the `Tenant` who is a children of a `Tenant` that owns the `Custom Role`.

#### Nested types

* `Permission`: Display the `Permissions` that are contained in a `Role`.
* `Users`: Display the `Users` that are assigned to a `Role`.
* `BusinessAccount`: Display the `BusinessAccount` that owns a `Role`.

### `roles(pagination: Pagination)`

Get a list of `Roles`, it allows pagination, sorting and filtering.

#### Validation

- `Template Role` can be visible by a `Subject` always.
- `Custom Role` can be visible by a `Subject` if:
    - The `Subject` is `AdformAdmin`.
    - The `Subject` is in the `Tenant` who owns the `Custom Role`.
    - The `Subject` is in the `Tenant` who is a children of a `Tenant` that owns the `Custom Role`.

#### Nested types

* `Permissions`: Display the `Permissions` that are contained in a `Role`.
* `Users`: Display the `Users` that are assigned to a `Role`.
* `BusinessAccount`: Display the `BusinessAccount` that owns a `Role`.

---

## Mutation

### `createRole`

Creates a new `Role` in the graph. To place it on different `Policy` the `policyId` field is used.

#### Validation

- `Template Role` can be created by a `Subject` if is an `AdformAdmin`.
- `Custom Role` can be created by a `Subject` if:
    - The `Subject` is `AdformAdmin`. 
    - The `Subject` is `LocalAdmin` of the `Tenant` that is specified.
    - The `Subject` is `LocalAdmin` of the `Tenant` that is parent of the `Tenant` is specified.

### `deleteRole`

Deletes a `Role` from the graph using the `id` field.

#### Validation

- `Template Role` can be deleted by a `Subject` if is an `AdformAdmin`.
- `Custom Role` can be deleted by a `Subject` if:
    - The `Subject` is `AdformAdmin`.
    - The `Subject` is `LocalAdmin` of the `Tenant` that owns the `Role`.
    - The `Subject` is `LocalAdmin` of the `Tenant` that is parent of the `Tenant` is specified.

### `assignPermissionToRole` [Deprecation] this is done by `updateRole` via `features`

Assigns a `Permission` to a `Role`. 

#### Validation

- A `Subject` that is an `AdformAdmin` can assign a `Permission` to a `Role`.

### `updateRole`

It updates a `Role` in the graph and its assigned `Permissions` via `Features`.

- `Template Role` can be updated by a `Subject` if is an `AdformAdmin`.
- `Custom Role` can be updated by a `Subject` if:
    - The `Subject` is `AdformAdmin`.
    - The `Subject` is `LocalAdmin` of the `Tenant` that owns the `Role`.
    - The `Subject` is `LocalAdmin` of the `Tenant` that is parent of the `Tenant` is specified. If this `Tenant` contains `Features` that will update the `Role`.

</details>

---

<details>
<summary>Permission</summary>

## Query

### `permission(id:ID)`

Get a `Permission` by id.

#### Validation

`Permission` can be listed by `AdformAdmin`.

#### Nested typed

* `Feature`: Display the `Feature` in which a `Permission` is contained.

### `permissions(pagination:Pagination)`

Get a list of `Permissions`, it allows pagination, sorting and filtering.

#### Validation

`Permissions` can be listed by `AdformAdmin`.

#### Nested typed

* `Feature`: Display the `Feature` in which a `Permission` is contained.

---

## Mutation

### `createPermission`

Creates a new `Permission` in the graph.

#### Validation

Only `AdformAdmin` can create a `Permission`.

### `deletePolicy`

Deletes a `Permission` from the graph using the `id` field.

#### Validation

Only `AdformAdmin` can delete a `Permission`.

</details>

---

<details>
<summary>Feature</summary>

## Query

### `feature(id:ID)`

Get a `Feature` by id.

#### Validation

A `Feature` can be listed if:
* `Subject` is an `AdformAdmin`.
* `Subject` is a `LocalAdmin` of a `Tenant` that owns the `Feature`.

#### Nested typed

* `BusinessAccount`: Display the `BusinessAccount` in which the `Feature` is Assigned.
* `LicensedFeatures`: Display the `LicensedFeature` in which the `Feature` is contained.
* `Feature`: Display the `Feature` co-dependency.
* `Permissions`: Display the `Permissions` that are contained in a `Feature`.

### `features(pagination:Pagination)`

Get a list of `Features`, it allows pagination, sorting and filtering.

#### Validation

A `Feature` can be listed if:
* `Subject` is an `AdformAdmin`.
* `Subject` is a `LocalAdmin` of a `Tenant` that owns the `Feature`.

#### Nested typed

* `BusinessAccount`: Display the `BusinessAccount` in which the `Feature` is Assigned.
* `LicensedFeatures`: Display the `LicensedFeature` in which the `Feature` is contained.
* `Feature`: Display the `Feature` co-dependency.
* `Permissions`: Display the `Permissions` that are contained in a `Feature`.

## Mutation

### `createFeature`

Creates a new `Feature` in the graph.

#### Validation

Only `AdformAdmin` can create a `Feature`.

### `deleteFeature`

Deletes a `Feature` from the graph using the `id` field.

#### Validation

Only `AdformAdmin` can delete a `Feature`.

### `assignPermissionToFeature`

Assigns a `Permission` to a `Feature`.

#### Validation

Only `AdformAdmin` can assign `Permission` a `Feature`.

### `assignFeatureCoDependency`

Assigns a `Feature` to a `Feature` as co-dependency.

#### Validation

Only `AdformAdmin` can assign co-dependency to a `Feature`.

> Note: `Feature` is to be created by a seed script.
> 
</details>

---

<details>
<summary>LicensedFeature</summary>

## Query

### `licensedFeature(id:ID)`

Get a `LicensedFeature` by id.

#### Validation

A `LicensedFeature` can be listed if:
* `Subject` is an `AdformAdmin`.
* `Subject` is a `LocalAdmin` of a `Tenant` that owns the `Feature` that is contained in a `LicensedFeature`.

#### Nested typed

* `Features`: Display the `Features` that are contained in a `LicensedFeature`.

### `licensedFeatures(pagination:Pagination)`

Get a list of `Features`, it allows pagination, sorting and filtering.

#### Validation

A `LicensedFeature` can be listed if:
* `Subject` is an `AdformAdmin`.
* `Subject` is a `LocalAdmin` of a `Tenant` that owns the `Feature` that is contained in a `LicensedFeature`.

#### Nested typed

* `Features`: Display the `Features` that are contained in a `LicensedFeature`.

---

## Mutation

> Note: `LicensedFeature` is to be created by a seed script. Therefore no creation or deletion mutation exist.

### `assignLicensedFeaturesToBusinessAccount`

Assigns `Features` to a `Tenant` via `LicensedFeature`.

#### Validation

Only `AdformAdmin` can assign `Features` to a `Tenant`.

</details>

---

<details>
<summary>Tenant (BusinessAccount)</summary>

## Query

### `businessAccount(id:ID)`

Get a `BusinessAccount` by id.

#### Validation

A `BusinessAccount` can be listed if:
* `Subject` is an `AdformAdmin`.
* `Subject` is a `LocalAdmin` that contains a `Tenant` either directly or by inheritance (parent can see children).

#### Nested typed

* `Roles`: Display the `Roles` that are owned by a `BusinessAccount`.
* `Users`: Display the `Users` that are assigned to a `BusinessAccount`.
* `LicensedFeature`: Display the `LicensedFeature` that are assigned to a `BusinessAccount` via `Features`.

### `businessAccounts(pagination:Pagination)`

Get a list of `BusinessAccounts`, it allows pagination, sorting and filtering.

#### Validation

A `BusinessAccount` can be listed if:
* `Subject` is an `AdformAdmin`.
* `Subject` is a `LocalAdmin` that contains a `Tenant` either directly or by inheritance (parent can see children).

#### Nested typed

* `Roles`: Display the `Roles` that are owned by a `BusinessAccount`.
* `Users`: Display the `Users` that are assigned to a `BusinessAccount`.
* `LicensedFeature`: Display the `LicensedFeature` that are assigned to a `BusinessAccount` via `Features`.

---

## Mutation

> Note: Creation and deletion of `BusinessAccount` is handled by DDP.

### `assignBusinessAccountToFeature` [Deprecation] check `assignLicensedFeaturesToBusinessAccount`

Assigns a `BusinessAccount` to a `Feature`.

#### Validation

Only `AdformAdmin` can assign `Features` to a `Tenant`.

</details>

---

<details>
<summary>Group</summary>

> Note: The `Group` is an entity that cannot be retrieved and is used to create a link between the `Policy` tree and the `Tenancy` tree.
> Note: The `Group` is created in a lazy way during the assignment.

</details>

---

<details>
<summary>Subject (User)</summary>

## Query

> Note: A regular user does not have any query. Except for the `Runtime` evaluation. We could add an additional endpoint `me` that will only allow query of `Subject`.

### `user(id:ID)`

Get a `User` by id.

#### Validation

A `User` can be listed if:
* `Subject` is an `AdformAdmin`.
* `Subject` is a `LocalAdmin` that contains a `Tenant` that intersects with the `User`.

#### Nested typed

* `Roles`: Display the `Roles` that are assigned to a `User`.
* `BusinessAccounts`: Display the `BusinessAccounts` that are assigned to a `User`.

### `users(pagination:Pagination)`

Get a list of `users`, it allows pagination, sorting and filtering.

#### Validation

A `User` can be listed if:
* `Subject` is an `AdformAdmin`.
* `Subject` is a `LocalAdmin` that contains a `Tenant` that intersects with the `User`.

#### Nested typed

* `Roles`: Display the `Roles` that are assigned to a `User`.
* `BusinessAccounts`: Display the `BusinessAccounts` that are assigned to a `User`.

---

## Mutation

> Note: `Subject` is to be created by the User creation flow v2.

### `updateUserAssignments`

Assigns `Roles` to a `User`.

#### Validation

A `User` can be assigned `Roles` if:
* `Actor` is an `AdformAdmin`.
* `Actor` is a `LocalAdmin` and has visibility over the `Role` and `Tenant` and the `Subject` exist on the graph (user creation).

#### Events

All the events are emitted only if the validaton and the mutation itself succeeds.

* `SubjectDisabledEvent` is emitted if the original `AuthorizationResult` is not empty and the new `AuthorizationResult` is empty, meaning that the subject has no links to any `Tenant`. This event is used by `Auth` legacy to disable the `LocalLogin`.
* `SubjectAssignedEvent` is emitted if the new `AuthorizationResult` contains new `Tenant`. This event is used by `Auth` legacy to create profiles.
NOTE.- This event has a boolean flag `InitialConnection`, that indicates whether is a new `Tenant` to `Subject` relationship being created. In case of DMP the flag is not dependant on individual `Tenant`, but is evaluated by existance of a `Tenant` of type `DMP`, this way the event will be triggered only once for `DMP` not per `Tenant` like in `Agencies` or `Publishers`. 
* `SubjectUnassignedEvent` is emitted if the new `AuthorizationResult` contains new `Tenant`. This event is used by `Auth` legacy to disabled profiles.
* `SubjectAuthorizationResultChangedEvent` is emitted on base conditions. This notification is used by AAP to update the `Authorization` cache.
* `SubjectAssignmentsNotification` is emitted on base conditions. This notification is used by AAP Notification Service.

</details>

