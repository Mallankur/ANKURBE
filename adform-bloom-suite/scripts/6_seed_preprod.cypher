MERGE (mk:Subject{Id:'cf5394c2-60c5-4ba0-8ad6-ba196aff6ce3',Name:'michal-komorowski',IsEnabled:true})
MERGE (artur:Subject{Id:'361b0f24-a846-412a-83cb-c468e2e328db',Name:'a.kordzik@adform.com',IsEnabled:true})
MERGE (aap:Subject{Id:'64129b96-5098-46b8-b83a-b01d231e0bec',Name:'aap_user',IsEnabled:true})

MERGE (adformAdminGroup:Group{Id:'98719e64-e769-4c38-9b4e-a352544666df', Name:'Adform-Adform Admin'})

MERGE (adformAdminGroup)<-[:MEMBER_OF]-(mk)
MERGE (adformAdminGroup)<-[:MEMBER_OF]-(artur)

MERGE (localAdmin3Group:Group{Id:'b5ef5374-0c00-4dd2-bbf3-c1254bed35ad', Name:'Nike-Local Admin'})
MERGE (localAdmin3Group)<-[:MEMBER_OF]-(aap)

MERGE (localAdmin5Group:Group{Id:'9d9247ce-6422-48e2-bf47-676710b010f0', Name:'Microsoft-Local Admin'})
MERGE (localAdmin5Group)<-[:MEMBER_OF]-(aap)
