# 18. profile-deactivation-new-flow

Date: 2021-01-20

## Status

Accepted

## Context

When the user gets his last role unassigned his profile needs to be deactivated

## Decision

During the role unassigment in bloom we need to evaluate whether the subject has any roles left in any tenats on the graph. In case of the last role is uanassigned we publish an event which is being handled by account-management. In the handler we deactivate the profile. If the last profile is deactivated, master account needs to be deactivated as well and it's local-login needs to be disabled.

## Consequences

Event handlers needs to be implemented for DMP/BuySide/SellSide

It might introduce data conflict between dmp mongo and auth mongo. Setting profile `active` flag on auth but not updating it on dmp

We still need to decide on the event name and adjust the existing stories as there is a story for `RoleUpdatedEvent` handler
