# Conclusion (2019-11-12)

In the short time of development good results were achieved thanks to the lessons learned from `ACL` and `Policy Server` PoC. Results seem promissing so far, and there is still a lot of optimization that can take place.

## Advantages

- Performance on `Bloom` is **multiple times** faster than the one observed on `Policy Server`.
- The code base is on our end allowing more flexibility.
- In terms of licensing, if well the project started with the concept of reusing the Open Source version of `Policy Server`, `Bloom` was build from scratch only with the notion of Policy Model in mind.
- IdSvr3 and IdSvr4 compatible as source of authority.
- GraphQL support (We are thinking to lean this way more as it allows subscriptions).
- Subscriptions can be made by WebSockets respecting the Apollo Guidelines.
- Model Documented.
- Api Documented.
- Tenant tree will be already build up which allows the next steps of the project (ACL).
- LaaS and MaaS compliant.
- Auditing can be performed and fitted for Adform.

## Disadvantage

- Is a new solution.
- OngDB Scaling (Raft).
- Database management
- Higher learning curve than NoSQL and SQL.
- Requires a bit of time to tweak and tune the performance even more.

## Recommendations

- `Bloom` is a good alternative for Policy Server (PS), it takes the core concepts of the `Policy Server` and also add more features like batch evaluation of tenant and policies which is a desired feature. 


