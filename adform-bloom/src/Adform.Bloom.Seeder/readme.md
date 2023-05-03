# Adform Bloom

Windows:
```shell
.\bin\neo4j-admin.bat import 
--nodes=".\import\node-headers\node-header-advertisers.csv,.\import\nodes\node-advertisers.csv" 
--nodes=".\import\node-headers\node-header-agencies.csv,.\import\nodes\node-agencies.csv" 
--nodes=".\import\node-headers\node-header-customRoles.csv,.\import\nodes\node-customRoles.csv" 
--nodes=".\import\node-headers\node-header-customGroups.csv,.\import\nodes\node-customGroups.csv" 
--nodes=".\import\node-headers\node-header-templateGroups.csv,.\import\nodes\node-templateGroups.csv" 
--nodes=".\import\node-headers\node-header-permissions.csv,.\import\nodes\node-permissions.csv" 
--nodes=".\import\node-headers\node-header-policies.csv,.\import\nodes\node-policies.csv" 
--nodes=".\import\node-headers\node-header-root.csv,.\import\nodes\node-root.csv" 
--nodes=".\import\node-headers\node-header-rootPolicy.csv,.\import\nodes\node-rootPolicy.csv" 
--nodes=".\import\node-headers\node-header-templateRoles.csv,.\import\nodes\node-templateRoles.csv" 
--nodes=".\import\node-headers\node-header-users.csv,.\import\nodes\node-users.csv" 
--nodes=".\import\node-headers\node-header-features.csv,.\import\nodes\node-features.csv" 
--relationships=".\import\link-headers\relationship-header-advertisers.csv,.\import\links\relationship-advertisers.csv" 
--relationships=".\import\link-headers\relationship-header-agencies.csv,.\import\links\relationship-agencies.csv" 
--relationships=".\import\link-headers\relationship-header-customRoles.csv,.\import\links\relationship-customRoles.csv" 
--relationships=".\import\link-headers\relationship-header-customGroups.csv,.\import\links\relationship-customGroups.csv" 
--relationships=".\import\link-headers\relationship-header-templateGroups.csv,.\import\links\relationship-templateGroups.csv" 
--relationships=".\import\link-headers\relationship-header-permissions.csv,.\import\links\relationship-permissions.csv" 
--relationships=".\import\link-headers\relationship-header-policies.csv,.\import\links\relationship-policies.csv" 
--relationships=".\import\link-headers\relationship-header-roles.csv,.\import\links\relationship-roles.csv" 
--relationships=".\import\link-headers\relationship-header-templateRoles.csv,.\import\links\relationship-templateRoles.csv" 
--relationships=".\import\link-headers\relationship-header-users.csv,.\import\links\relationship-users.csv" 
--relationships=".\import\link-headers\relationship-header-features.csv,.\import\links\relationship-features.csv" 
--relationships=".\import\link-headers\relationship-header-tenantFeatures.csv,.\import\links\relationship-tenantFeatures.csv" 
--delimiter="|" 
--ignore-duplicate-nodes=true 
--ignore-missing-nodes=true 
```

Linux:
```shell
./neo4j-admin import --nodes="/import/node-headers/node-header-tenants.csv,/import/nodes/node-tenants.csv"  --nodes="/import/node-headers/node-header-customRoles.csv,/import/nodes/node-customRoles.csv" --nodes="/import/node-headers/node-header-permissions.csv,/import/nodes/node-permissions.csv" --nodes="/import/node-headers/node-header-policies.csv,/import/nodes/node-policies.csv" --nodes="/import/node-headers/node-header-root.csv,/import/nodes/node-root.csv" --nodes="/import/node-headers/node-header-rootPolicy.csv,/import/nodes/node-rootPolicy.csv" --nodes="/import/node-headers/node-header-templateRoles.csv,/import/nodes/node-templateRoles.csv" --nodes="/import/node-headers/node-header-users.csv,/import/nodes/node-users.csv" --nodes="/import/node-headers/node-header-features.csv,/import/nodes/node-features.csv" --nodes="/import/node-headers/node-header-licensedFeatures.csv,/import/nodes/node-licensedFeatures.csv" --nodes="/import/node-headers/node-header-templateGroups.csv,/import/nodes/node-templateGroups.csv" --nodes="/import/node-headers/node-header-customGroups.csv,/import/nodes/node-customGroups.csv" --relationships="/import/link-headers/relationship-header-tenants.csv,/import/links/relationship-tenants.csv" --relationships="/import/link-headers/relationship-header-customRoles.csv,/import/links/relationship-customRoles.csv" --relationships="/import/link-headers/relationship-header-permissions.csv,/import/links/relationship-permissions.csv" --relationships="/import/link-headers/relationship-header-policies.csv,/import/links/relationship-policies.csv"  --relationships="/import/link-headers/relationship-header-roles.csv,/import/links/relationship-roles.csv" --relationships="/import/link-headers/relationship-header-users.csv,/import/links/relationship-users.csv" --relationships="/import/link-headers/relationship-header-templateRoles.csv,/import/links/relationship-templateRoles.csv" --relationships="/import/link-headers/relationship-header-features.csv,/import/links/relationship-features.csv" --relationships="/import/link-headers/relationship-header-featureDependencies.csv,/import/links/relationship-featureDependencies.csv" --relationships="/import/link-headers/relationship-header-licensedFeatures.csv,/import/links/relationship-licensedFeatures.csv" --relationships="/import/link-headers/relationship-header-tenantFeatures.csv,/import/links/relationship-tenantFeatures.csv" --relationships="/import/link-headers/relationship-header-customGroups.csv,/import/links/relationship-customGroups.csv" --relationships="/import/link-headers/relationship-header-templateGroups.csv,/import/links/relationship-templateGroups.csv" --delimiter="|" --ignore-duplicate-nodes=true --ignore-missing-nodes=true
```
