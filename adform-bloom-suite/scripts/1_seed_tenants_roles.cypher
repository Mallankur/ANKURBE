MERGE (rootPolicy:Policy{Id:'eaa2879e-734c-4d25-b4bd-6086f69fc0c8',Name:'Root Policy',IsEnabled:true, CreatedAt: timestamp()})

// ************ Default Tenants tree ************

MERGE (adform:Tenant:Adform{Id:'51ac8f26-fe26-4052-b6d4-edda1324c313', LegacyId: 0, Name:'Adform',IsEnabled:true, CreatedAt: timestamp()})
MERGE (ikea:Tenant:Agency{Id:'9943b62a-10dc-4b59-aff0-a31d1be5e259', LegacyId: 1, Name:'IKEA',IsEnabled:true, CreatedAt: timestamp()})
MERGE (nike:Tenant:Agency{Id:'d5ad15ed-b00b-497e-86ab-e3bc5acde17e', LegacyId: 2,Type: 'Agency', Name:'Nike',IsEnabled:true, CreatedAt: timestamp()})
MERGE (apple:Tenant:Agency{Id:'d69bee1f-5441-4a32-b95c-221c8d8f380e', LegacyId: 3, Name:'Apple',IsEnabled:true, CreatedAt: timestamp()})
MERGE (microsoft:Tenant:Agency{Id:'43e35830-623c-4ad3-b6fc-8a660c8224a9', LegacyId: 4, Name:'Microsoft',IsEnabled:true, CreatedAt: timestamp()})

MERGE (ikea)-[:CHILD_OF]->(adform)
MERGE (nike)-[:CHILD_OF]->(adform)
MERGE (apple)-[:CHILD_OF]->(adform)
MERGE (microsoft)-[:CHILD_OF]->(adform)

// ************ Roles, groups, assignments of roles to policies and tenants ************

MERGE (adformAdmin:Role:CustomRole{Id:'29366571-5a53-448d-85b4-b98e7ba9dee6',Name:'Adform Admin',IsEnabled:true, CreatedAt: timestamp()})
MERGE (adformAdmin)<-[:OWNS]-(adform)
MERGE (adformAdminGroup:Group{Id:'98719e64-e769-4c38-9b4e-a352544666df',Name:'51ac8f26-fe26-4052-b6d4-edda1324c313_29366571-5a53-448d-85b4-b98e7ba9dee6',IsEnabled:true})
MERGE (adformAdminGroup)-[:ASSIGNED]->(adformAdmin)
MERGE (adformAdminGroup)-[:BELONGS]->(adform)
MERGE (rootPolicy)-[:CONTAINS]->(adformAdmin)

MERGE (localAdmin1:Role:CustomRole{Id:'2f2fc644-5500-4554-a62d-db8c7610684a',Name:'Local Admin',IsEnabled:true, CreatedAt: timestamp()})
MERGE (localAdmin1)<-[:OWNS]-(adform)
MERGE (localAdmin1Group:Group{Id:'57ae488f-537c-4f44-94b3-5ca624fc6357',Name:'51ac8f26-fe26-4052-b6d4-edda1324c313_2f2fc644-5500-4554-a62d-db8c7610684a',IsEnabled:true})
MERGE (localAdmin1Group)-[:ASSIGNED]->(localAdmin1)
MERGE (localAdmin1Group)-[:BELONGS]->(adform)
MERGE (rootPolicy)-[:CONTAINS]->(localAdmin1)

MERGE (localAdmin2:Role:CustomRole{Id:'622e0f44-8153-40e8-8f8a-2c63fa306d96',Name:'Local Admin',IsEnabled:true, CreatedAt: timestamp()})
MERGE (localAdmin2)<-[:OWNS]-(ikea)
MERGE (localAdmin2Group:Group{Id:'0a33359b-df43-497b-9983-7d99207b1c53',Name:'9943b62a-10dc-4b59-aff0-a31d1be5e259_622e0f44-8153-40e8-8f8a-2c63fa306d96',IsEnabled:true})
MERGE (localAdmin2Group)-[:ASSIGNED]->(localAdmin2)
MERGE (localAdmin2Group)-[:BELONGS]->(ikea)
MERGE (rootPolicy)-[:CONTAINS]->(localAdmin2)

MERGE (localAdmin3:Role:CustomRole{Id:'303dfcfb-2cfb-4089-a210-0aebf60d2150',Name:'Local Admin',IsEnabled:true, CreatedAt: timestamp()})
MERGE (localAdmin3)<-[:OWNS]-(nike)
MERGE (localAdmin3Group:Group{Id:'b5ef5374-0c00-4dd2-bbf3-c1254bed35ad',Name:'d5ad15ed-b00b-497e-86ab-e3bc5acde17e_303dfcfb-2cfb-4089-a210-0aebf60d2150',IsEnabled:true})
MERGE (localAdmin3Group)-[:ASSIGNED]->(localAdmin3)
MERGE (localAdmin3Group)-[:BELONGS]->(nike)
MERGE (rootPolicy)-[:CONTAINS]->(localAdmin3)

MERGE (localAdmin4:Role:CustomRole{Id:'421e2982-64b4-43cd-8363-9d9bd1ac124a',Name:'Local Admin',IsEnabled:true, CreatedAt: timestamp()})
MERGE (localAdmin4)<-[:OWNS]-(apple)
MERGE (localAdmin4Group:Group{Id:'8907f4ac-6683-436e-8e7c-d97fc5738e43',Name:'d69bee1f-5441-4a32-b95c-221c8d8f380e_421e2982-64b4-43cd-8363-9d9bd1ac124a',IsEnabled:true})
MERGE (localAdmin4Group)-[:ASSIGNED]->(localAdmin4)
MERGE (localAdmin4Group)-[:BELONGS]->(apple)
MERGE (rootPolicy)-[:CONTAINS]->(localAdmin4)

MERGE (localAdmin5:Role:CustomRole{Id:'19479ece-bc6d-4978-b5d5-f771913cde27',Name:'Local Admin',IsEnabled:true, CreatedAt: timestamp()})
MERGE (localAdmin5)<-[:OWNS]-(microsoft)
MERGE (localAdmin5Group:Group{Id:'9d9247ce-6422-48e2-bf47-676710b010f0',Name:'43e35830-623c-4ad3-b6fc-8a660c8224a9_19479ece-bc6d-4978-b5d5-f771913cde27',IsEnabled:true})
MERGE (localAdmin5Group)-[:ASSIGNED]->(localAdmin5)
MERGE (localAdmin5Group)-[:BELONGS]->(microsoft)
MERGE (rootPolicy)-[:CONTAINS]->(localAdmin5)

