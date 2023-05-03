To load data into database:
1. [Optionally] Clear database with the following script. However, if the database is very large it may take a lot of time or out of memory exception can occur:
```
MATCH (n) DETACH DELETE n
```
1. Run *0_seed_constraints.cypher*.
1. Run *1_seed_tenants_roles.cypher*.
1. [Optional] Run *2_seed_additional_tenants.cypher*.
1. Run *3_seed_features.cypher*.
1. Run *4_seed_default_tenants_to_features.cypher*.
1. [Optional] Run *4_seed_additional_tenants_to_features.cypher*.
1. [Optional] Run *5_seed_local_admins_for_additional_tenants*.
1. Run *6_seed_ENV.cypher* where *ENV* depends on a target environment and can be equal to *d1* or *prod*.
1. Run *7_indexes.cypher* to add constrains to the database.