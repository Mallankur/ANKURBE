# 13. modules-features-permissions

Date: 2020-08-13

## Status

Accepted

## Context

Due to original assumption, that both, licensed features can overlap features and features can overlap permissions (many-to-many relationship), we are not able to determine which features are represented by specific permissions and which licensed features are represented by specific features. Such functionality though is crucial for the UI.

What also needs to be considered is the fact that some features will surely depend on other features (e.g. `Edit Campaign` will depend on `View Campaign`).

## Decision

There were two posible solutions for such:

* Decouple UI-specific model (licensed features, features and permissions) from authorization model, store it in a separate database and host additional service, which would decorate UI queries with licensed features & features.
* Make an assumption (and validate it an any point) that relationship between licensed features and features, features and permissions **MUST BE** one-to-many.

Applies to both solutions - introduce another type of relationship `DEPENDS_ON` reflecting features depenencies. This type of relationship would be used only during features-to-role assignment operation as an additional validation and will not be considered in permission evaluation as such.

Due the fact that:

1. Above assumption is acceptable from the business perspective
2. Hosting additional service is a big overhead

It was decided to go with the second solution and add new constraints and new type of relationship to the existing model.

## Consequences

* Additional validation needs to be implemented upon assignment creation operation.
* There could be a slight increase of the number of feature and licensed features but the overall number of those is still very low and will not affect performance.
* There could be a slight increase of the number relationships (edges) but due to the fact the overall expected number of features is low it will not affect performance.
