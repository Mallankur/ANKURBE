MERGE (mk:Subject{Id:'33c19339-b3b6-4cd7-ad74-44d1bc33cb39',Name:'michal-komorowski',IsEnabled:true})
MERGE (crespo:Subject{Id:'3f8feda5-1094-4f08-9b3d-c545dcefdc0b',Name:'Mcrespo',IsEnabled:true})
MERGE (artur:Subject{Id:'13b4d573-2a26-471b-8287-41da8cd2f3a5',Name:'artur-master-test',IsEnabled:true})
MERGE (iza:Subject{Id:'ead02d69-7b50-43b6-971a-e489c3dcfb81',Name:'iblaszczyk',IsEnabled:true})
MERGE (aap:Subject{Id:'b132abd3-60fd-434d-8c34-479de526894b',Name:'aap.prod',IsEnabled:true})

MERGE (adformAdminGroup:Group{Id:'98719e64-e769-4c38-9b4e-a352544666df', Name:'Adform-Adform Admin'})

MERGE (adformAdminGroup)<-[:MEMBER_OF]-(mk)
MERGE (adformAdminGroup)<-[:MEMBER_OF]-(crespo)
MERGE (adformAdminGroup)<-[:MEMBER_OF]-(artur)
MERGE (adformAdminGroup)<-[:MEMBER_OF]-(iza)

MERGE (localAdmin3Group:Group{Id:'b5ef5374-0c00-4dd2-bbf3-c1254bed35ad', Name:'Nike-Local Admin'})
MERGE (localAdmin3Group)<-[:MEMBER_OF]-(aap)

MERGE (localAdmin5Group:Group{Id:'9d9247ce-6422-48e2-bf47-676710b010f0', Name:'Microsoft-Local Admin'})
MERGE (localAdmin5Group)<-[:MEMBER_OF]-(aap)
