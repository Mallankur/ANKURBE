MERGE (mk:Subject{Id:'7c96d6e5-0a75-4a50-b55a-a0af3fc4a1ac',Name:'michal-komorowski', Username: 'michal-komorowski', LegacyId: 243050, Email:"michal.komorowski@adform.com",IsEnabled:true, CreatedAt: timestamp()})
MERGE (crespo:Subject{Id:'33f88767-6b55-488d-8a26-ac17cc07876c',Name:'MasterCrespo',Username:'MasterCrespo',Email:"a.crespo@adform.com", LegacyId: 265177,IsEnabled:true, CreatedAt: timestamp()})
MERGE (artur:Subject{Id:'aaee56dc-3abc-4f81-aad7-eb751c9456a9',Name:'artur-master-username',Username:'artur-master-username',Email:"artur.test.22@adform.com", LegacyId: 215419,IsEnabled:true, CreatedAt: timestamp()})
MERGE (iza:Subject{Id:'c1b0afc9-b3fc-4210-941e-b53b4781f6bd',Name:'iblaszczyk',Username:'iblaszczyk',Email:"izabela.blaszczyk@adform.com", LegacyId: 350114,IsEnabled:true, CreatedAt: timestamp()})
MERGE (aap:Subject{Id:'4bf932dc-1f93-455e-b8a7-4fa731268ca6',Name:'aap_user',Username:'aap_user',Email:"v.shaban@adform.com", LegacyId: 218301,IsEnabled:true, CreatedAt: timestamp()})

MERGE (adformAdminGroup:Group{Name:'51ac8f26-fe26-4052-b6d4-edda1324c313_29366571-5a53-448d-85b4-b98e7ba9dee6'})

MERGE (adformAdminGroup)<-[:MEMBER_OF]-(mk)
MERGE (adformAdminGroup)<-[:MEMBER_OF]-(crespo)
MERGE (adformAdminGroup)<-[:MEMBER_OF]-(artur)
MERGE (adformAdminGroup)<-[:MEMBER_OF]-(iza)

MERGE (localAdmin3Group:Group{Name:'d5ad15ed-b00b-497e-86ab-e3bc5acde17e_303dfcfb-2cfb-4089-a210-0aebf60d2150'})
MERGE (localAdmin3Group)<-[:MEMBER_OF]-(aap)

MERGE (localAdmin5Group:Group{Name:'43e35830-623c-4ad3-b6fc-8a660c8224a9_19479ece-bc6d-4978-b5d5-f771913cde27'})
MERGE (localAdmin5Group)<-[:MEMBER_OF]-(aap)

MERGE (adformLocalAdminGroup:Group{Name:'51ac8f26-fe26-4052-b6d4-edda1324c313_2f2fc644-5500-4554-a62d-db8c7610684a'})
MERGE (adformLocalAdminGroup)<-[:MEMBER_OF]-(aap)
