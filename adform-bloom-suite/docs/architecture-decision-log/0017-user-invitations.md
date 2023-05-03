# 17. user-invitations

Date: 2021-04-26

## Status

Accepted

## Context

At the moment a `user account` is created is desired to sent an invitation to activate the account, so the email can be verify. Besides that is desired to have status of this invitation/user available on the model that is consumed by the UI.

## Decision

The UI resposible to complete the invitations has to remain on our end, as Flow is not prepared for UI that don't have authentication and in this kind of views the context of authentication is limited by TOTP. Idsvr is the proper place for the invitation acceptance and the new user creation flow.
The legacy UI (MyAccount) should not be considered as candidate.

The status needed for the UI will be obtained from `LocalLogin` so this can be aggregated in `bloom-read`.

## Consequences

A view needs to be redefined on Idsvr (as is the most appropriate place to handle views without authentication context). 

A new sink for ddp needs to be set for `LocalLogin` contracts.



