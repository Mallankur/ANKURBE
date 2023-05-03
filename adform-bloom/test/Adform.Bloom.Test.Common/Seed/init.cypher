CREATE 
(p0:Policy:Adform{Id:'647e0b74-b9f3-4e1b-934e-ae9fac0c335d',Name:'Policy0',IsEnabled:true}),
(p1:Policy:Identity{Id:'90e21677-cd27-49b6-a76f-0ba1779845a5',Name:'Policy1',IsEnabled:true}),
(p2:Policy:Agency{Id:'acd5fd66-d8ee-4a79-967a-4c955160f751',Name:'Policy2',IsEnabled:true}),
(p3:Policy:Publisher{Id:'8ae1dfec-7c07-4e6e-8bce-fdc20519fc02',Name:'Policy3',IsEnabled:true}),
(p4:Policy:DataProvider{Id:'2fa850c8-b76d-48f7-8aac-b4871d01e4f8',Name:'Policy4',IsEnabled:true}),

(p0)<-[:CHILD_OF]-(p1),
(p0)<-[:CHILD_OF]-(p2),
(p0)<-[:CHILD_OF]-(p3),
(p0)<-[:CHILD_OF]-(p4),

(t0:Tenant:Adform{Id:'bd926f81-0316-4a1a-b6ba-b717ccc1ff1d',Name:'Adform',IsEnabled:true,LegacyId:0}),
(t1:Tenant:Agency{Id:'ea0d54a3-1d54-4410-a2a0-55548baa4587',Name:'Tenant1',IsEnabled:true,LegacyId:1}),
(t2:Tenant:Agency{Id:'49195836-874c-4db2-b954-a4ee70717eab',Name:'Tenant2',IsEnabled:true,LegacyId:2}),
(t3:Tenant:Publisher{Id:'108b9151-15d3-4cbb-b4ec-fef42c4c5567',Name:'Tenant3',IsEnabled:true,LegacyId:3}),
(t4:Tenant:Publisher{Id:'287f9d9d-5917-4721-8e24-fa4e352099f1',Name:'Tenant4',IsEnabled:true,LegacyId:4}),
(t5:Tenant:DataProvider{Id:'b214781b-5235-42c1-a89f-93a30c364eef',Name:'Tenant5',IsEnabled:true,LegacyId:5}),
(t6:Tenant:DataProvider{Id:'0748600c-e328-48df-8ec9-be52af4f0889',Name:'Tenant6',IsEnabled:true,LegacyId:6}),
(t7:Tenant:Agency{Id:'858e4425-72ca-4113-8fe9-27fa5b034878',Name:'Tenant7',IsEnabled:true,LegacyId:7}),
(t8:Tenant:Agency{Id:'d2b69ddb-fe00-4c5d-916f-b71877dac4c8',Name:'Tenant8',IsEnabled:true,LegacyId:8}),
(t9:Tenant:Agency{Id:'ce006fd5-3339-40de-b553-d12c2a136b3c',Name:'Tenant9',IsEnabled:true, LegacyId:9}),
(t10:Tenant:Publisher{Id:'d5a39696-5c2e-4047-b5c4-e318b81bbc61',Name:'Tenant10',IsEnabled:true,LegacyId:10}),
(t11:Tenant:Publisher{Id:'7bbd00be-ab52-4e7f-b75e-dd91276dc360',Name:'Tenant11',IsEnabled:true,LegacyId:11}),
(t12:Tenant:Publisher{Id:'ab5ad0f7-7643-467e-9f26-a6ddede2e268',Name:'Tenant12',IsEnabled:true,LegacyId:12}),
(t13:Tenant:DataProvider{Id:'8ffc490c-a173-4499-92b3-686e2fe32bf8',Name:'Tenant13',IsEnabled:true,LegacyId:13}),
(t14:Tenant:DataProvider{Id:'c8231bb6-a453-4683-835b-4c972e62b5f9',Name:'Tenant14',IsEnabled:true,LegacyId:14}),
(t15:Tenant:DataProvider{Id:'7ff1c7cc-ab29-4e24-aa8b-28c415e025e0',Name:'Tenant15',IsEnabled:true,LegacyId:15}),
(t16:Tenant:DataProvider{Id:'a9f07c1d-f137-4aa3-a5f6-e223a81da4b2',Name:'Tenant16',IsEnabled:true,LegacyId:16}),

(t0)<-[:CHILD_OF]-(t1),
(t0)<-[:CHILD_OF]-(t2),
(t0)<-[:CHILD_OF]-(t3),
(t0)<-[:CHILD_OF]-(t4),
(t0)<-[:CHILD_OF]-(t5),
(t0)<-[:CHILD_OF]-(t6),
(t1)<-[:CHILD_OF]-(t7),
(t1)<-[:CHILD_OF]-(t8),
(t2)<-[:CHILD_OF]-(t9),
(t3)<-[:CHILD_OF]-(t10),
(t3)<-[:CHILD_OF]-(t11),
(t4)<-[:CHILD_OF]-(t12),
(t5)<-[:CHILD_OF]-(t13),
(t5)<-[:CHILD_OF]-(t14),
(t6)<-[:CHILD_OF]-(t15),
(t6)<-[:CHILD_OF]-(t16),

(s0:Subject{Id:'bf15ab64-ff92-4143-99df-2f409652e2e3',Name:'Subject0',Email:"subject0@test",IsEnabled:true}),
(s1:Subject{Id:'050c54d3-a928-4430-baa2-910a9526c505',Name:'Subject1',Email:"subject1@test",IsEnabled:true}),
(s2:Subject{Id:'058c54d3-a928-4430-baa2-910a9526c505',Name:'Subject2',Email:"subject2@test",IsEnabled:true}),
(s3:Subject{Id:'058c54d3-a928-4430-baa2-950a9526c505',Name:'Subject3',Email:"subject3@test",IsEnabled:true}),
(s4:Subject{Id:'058c54d3-a928-4430-1111-910a9526c000',Name:'Subject4',Email:"subject4@test",IsEnabled:true}),
(s5:Subject{Id:'e32539ac-3c97-48c6-80a5-cdf78635148a',Name:'Subject5',Email:"subject5@test",IsEnabled:true}),
(s6:Subject{Id:'e32539ac-3c97-48c6-80a5-cdf78635148b',Name:'Subject6',Email:"subject6@test",IsEnabled:true}),
(s7:Subject{Id:'e65399fe-b720-436c-b21e-f97ed9c968cb',Name:'Subject7',Email:"subject7@test",IsEnabled:true}),
(s8:Subject{Id:'d6d5f3e8-5578-464e-a7a6-f5107102e527',Name:'Subject8',Email:"subject8@test",IsEnabled:true}),
(s9:Subject{Id:'3edebd10-2c37-42fe-8216-5053efb0f511',Name:'Subject9',Email:"subject9@test",IsEnabled:true}),
(s10:Subject{Id:'a7c1e30f-1469-4df1-8e4b-144716b5d656',Name:'Subject10',Email:"subject10@test",IsEnabled:true}),

(ts0:Subject:Trafficker{Id:'82c142d0-0533-4302-b09a-a0fb5373942c',Name:'Trafficker0',Email:"trafficker0@test",IsEnabled:true}),
(ts1:Subject:Trafficker{Id:'b2500bf1-45e9-4b8f-9be4-a8c8dd52a9f0',Name:'Trafficker1',Email:"trafficker1@test",IsEnabled:true}),
(ts2:Subject:Trafficker{Id:'74936140-fbdb-468c-8cff-891bdde5dd17',Name:'Trafficker2',Email:"trafficker2@test",IsEnabled:true}),

(per0:Permission{Id:'28480b45-c0a5-424b-99d0-e614aa8f117e',Name:'Permission0',IsEnabled:true}),
(per1:Permission{Id:'8c86fb94-7e39-4df9-8a61-6f3bbf58e2bf',Name:'Permission1',IsEnabled:true}),
(per2:Permission{Id:'d2479031-337d-4f37-b540-7199d61677f9',Name:'Permission2',IsEnabled:true}),
(per3:Permission{Id:'7f639f5f-b6e1-45f7-9534-d6d83562af25',Name:'Permission3',IsEnabled:true}),
(per4:Permission{Id:'e28be61d-9c61-449a-99c2-ca0da1261af3',Name:'Permission4',IsEnabled:true}),
(per5:Permission{Id:'65085381-8aea-4ab5-a035-21e1740b3d3f',Name:'Permission5',IsEnabled:true}),
(per6:Permission{Id:'0c110b5a-a57d-4f16-804f-46dea0091470',Name:'Permission6',IsEnabled:true}),
(per7:Permission{Id:'0c110b5a-a57d-4f16-804f-46dea0091471',Name:'Permission7',IsEnabled:true}),
(per8:Permission{Id:'0ddcd3c9-e699-48c5-bda3-7906cd299184',Name:'Permission8',IsEnabled:true}),
(per9:Permission{Id:'6a2a83f3-6d86-4e91-868c-011e424357e7',Name:'Permission9',IsEnabled:true}),
(per10:Permission{Id:'40f7f101-0f8e-429a-ad42-ae144bae9ebf',Name:'Permission10',IsEnabled:true}),
(per11:Permission{Id:'2271319e-b0bd-45bb-8506-ce88e85f31e8',Name:'Permission11',IsEnabled:true}),
(per12:Permission{Id:'0473543b-111c-4390-86ad-bb29dbb75984',Name:'Permission12',IsEnabled:true}),
(per13:Permission{Id:'ad32bd0e-eef5-40fd-b8a7-0a258d258f8c',Name:'Permission13',IsEnabled:true}),
(per14:Permission{Id:'0071956a-e264-4cf2-b9ac-2383bf703c2e',Name:'Permission14',IsEnabled:true}),
(per15:Permission{Id:'04b55220-ff19-4cbf-9e98-74e32e0333e9',Name:'Permission15',IsEnabled:true}),
(per16:Permission{Id:'78480b45-d0a5-524b-99d0-f614aa8f117e',Name:'Permission16',IsEnabled:true}),

(lf0:LicensedFeature{Id:'d2b69ddb-fe00-4c5d-916f-b90877dac4c8',Name:'LicensedFeature0',IsEnabled:true}),
(lf1:LicensedFeature{Id:'c2b69ddb-fa00-5c5d-916f-b90877dac4c8',Name:'LicensedFeature1',IsEnabled:true}),
(lf2:LicensedFeature{Id:'c1b69ddb-fa00-5c5d-916f-b90877dac4c8',Name:'LicensedFeature2',IsEnabled:true}),
(lf3:LicensedFeature{Id:'875964d3-f5d1-458b-b1c9-9c3a88d6b657',Name:'LicensedFeature3',IsEnabled:false}),
(lf4:LicensedFeature{Id:'7a0d75ad-9702-4c02-a358-bf367e840754',Name:'LicensedFeature4',IsEnabled:false}),

(f0:Feature{Id:'98b3bd33-5f69-4dc9-aa89-8c2b0af900df',Name:'Feature0',IsEnabled:true}),
(f1:Feature{Id:'f86af37f-c992-4ed7-b469-5ae4c15f95c6',Name:'Feature1',IsEnabled:true}),
(f2:Feature{Id:'885b0e53-e44a-4181-bb79-e8844b655515',Name:'Feature2',IsEnabled:true}),
(f3:Feature{Id:'1cf6d35c-8336-4cec-af2c-a85b3dc745cb',Name:'Feature3',IsEnabled:true}),
(f4:Feature{Id:'196a53a8-4390-473f-af31-bed82910558b',Name:'Feature4',IsEnabled:true}),
(f5:Feature{Id:'8e4bb6b6-7418-4db6-bec8-e27b4f227faa',Name:'Feature5',IsEnabled:true}),
(f6:Feature{Id:'b65dc109-8dae-4e88-9372-6de3a9516229',Name:'Feature6',IsEnabled:true}),
(f7:Feature{Id:'4f838b42-dc83-4bb0-8420-c8f713da6d98',Name:'Feature7',IsEnabled:true}),
(f8:Feature{Id:'9781ba27-c5f3-4f00-9033-bc10ce91c9f6',Name:'Feature8',IsEnabled:false}),
(f8)-[:DEPENDS_ON]->(f7),

(lf0)-[:CONTAINS]->(f0),
(lf1)-[:CONTAINS]->(f1),
(lf1)-[:CONTAINS]->(f2),
(lf2)-[:CONTAINS]->(f3),
(lf2)-[:CONTAINS]->(f4),
(lf3)-[:CONTAINS]->(f5),
(lf3)-[:CONTAINS]->(f6),
(lf4)-[:CONTAINS]->(f7),
(lf4)-[:CONTAINS]->(f8),

(t0)-[:ASSIGNED]->(lf0),
(t0)-[:ASSIGNED]->(lf1),
(t0)-[:ASSIGNED]->(lf2),
(t0)-[:ASSIGNED]->(lf3),
(t0)-[:ASSIGNED]->(lf4),

(t1)-[:ASSIGNED]->(lf2),

(t2)-[:ASSIGNED]->(lf2),

(t3)-[:ASSIGNED]->(lf3),

(t4)-[:ASSIGNED]->(lf3),

(t5)-[:ASSIGNED]->(lf4),

(t6)-[:ASSIGNED]->(lf4),

(t7)-[:ASSIGNED]->(lf2),

(t8)-[:ASSIGNED]->(lf2),

(t9)-[:ASSIGNED]->(lf2),

(t10)-[:ASSIGNED]->(lf3),

(t11)-[:ASSIGNED]->(lf3),

(t12)-[:ASSIGNED]->(lf3),

(t13)-[:ASSIGNED]->(lf4),

(t14)-[:ASSIGNED]->(lf4),

(t15)-[:ASSIGNED]->(lf4),

(t16)-[:ASSIGNED]->(lf4),

(f0)-[:CONTAINS]->(per0),
(f1)-[:CONTAINS]->(per1),
(f1)-[:CONTAINS]->(per2),
(f2)-[:CONTAINS]->(per3),
(f2)-[:CONTAINS]->(per4),

(f3)-[:CONTAINS]->(per5),
(f3)-[:CONTAINS]->(per6),
(f4)-[:CONTAINS]->(per7),
(f4)-[:CONTAINS]->(per8),

(f5)-[:CONTAINS]->(per9),
(f5)-[:CONTAINS]->(per10),
(f6)-[:CONTAINS]->(per11),
(f6)-[:CONTAINS]->(per12),

(f7)-[:CONTAINS]->(per13),
(f7)-[:CONTAINS]->(per14),
(f8)-[:CONTAINS]->(per15),
(f8)-[:CONTAINS]->(per16),


(adformAdmin:Role:System{Id:'caa9d05f-20c4-42b3-9c06-37c53bb0ae84',Name:'Adform Admin',IsEnabled:true}),
(bsAdmin:Role:System{Id:'caa9d05f-21c4-42b3-9c06-37c53bb0ae84',Name:'Demand Side Admin',IsEnabled:true}),
(ssAdmin:Role:System{Id:'caa9d05f-22c4-42b3-9c06-37c53bb0ae84',Name:'Supply Side Admin',IsEnabled:true}),
(dmpAdmin:Role:System{Id:'caa9d05f-23c4-42b3-9c06-37c53bb0ae84',Name:'Data Management Admin',IsEnabled:true}),
(iamAdmin:Role:System{Id:'caa9d05f-24c4-42b3-9c06-37c53bb0ae84',Name:'IAM Admin',IsEnabled:true}),

(localAdmin:Role:TemplateRole{Id:'56bd1ad3-fd40-4200-84ee-62e1af1bd694',Name:'Local Admin',IsEnabled:true}),
(r0:Role:CustomRole{Id:'a04f8e43-bbfe-4cf5-8b1b-36faa08e946f',Name:'Role0',IsEnabled:true}),
(r1:Role:TemplateRole{Id:'c24e8b1c-1815-499e-9331-628558090776',Name:'Role1',IsEnabled:true}),
(r2:Role:TemplateRole{Id:'80d5dbcc-8560-4538-a0ea-73604b2b4c63',Name:'Role2',IsEnabled:true}),
(r3:Role:TemplateRole{Id:'a706d1bc-9f78-4744-b8db-5772c4328416',Name:'Role3',IsEnabled:true}),
(r4:Role:CustomRole{Id:'386e1fbd-bd88-46a2-9a83-e1fd34e47189',Name:'Role4',IsEnabled:true}),
(r5:Role:CustomRole{Id:'69afa7ed-7dda-497f-a0cc-7b144366b6ed',Name:'Role5',IsEnabled:true}),
(r6:Role:CustomRole{Id:'58298ba8-b278-482d-b520-7dbff17875e6',Name:'Role6',IsEnabled:true}),
(r7:Role:CustomRole{Id:'0a3a073f-dec3-4434-a357-86532b4f6195',Name:'Role7',IsEnabled:true}),
(r8:Role:CustomRole{Id:'db95cb4d-25e4-4167-9516-4558cc880805',Name:'Role8',IsEnabled:true}),
(r9:Role:CustomRole{Id:'893b3f49-9f5b-4cfb-afd9-2149b9344064',Name:'Role9',IsEnabled:true}),
(r10:Role:CustomRole{Id:'69afa7ed-7dda-497f-a0cc-7b144366b6e0',Name:'Role10',IsEnabled:true}),
(r11:Role:CustomRole{Id:'1be08cb0-ffec-4a45-8f09-6f756eaf10df',Name:'Role11',IsEnabled:true}),
(r12:Role:CustomRole{Id:'2ac73707-d2cd-4773-be06-46620e47dbc2',Name:'Role12',IsEnabled:true}),
(r13:Role:CustomRole{Id:'0ee13ad6-e26a-43fe-b9f1-1e4bf25b1c05',Name:'Role13',IsEnabled:true}),
(r14:Role:CustomRole{Id:'01276c88-3a1e-4d69-8d79-fb2d860f199f',Name:'Role14',IsEnabled:true}),
(r15:Role:CustomRole{Id:'198b07a2-6a5d-4b00-9c46-ae3056f7c2f2',Name:'Role15',IsEnabled:true}),
(r16:Role:CustomRole{Id:'cf038c38-8475-4045-ac2a-fc26e930782c',Name:'Role16',IsEnabled:true}),
(r17:Role:CustomRole{Id:'ca0ca9cb-12b8-409a-a32f-1770f19a072b',Name:'Role17',IsEnabled:true}),
(r18:Role:CustomRole{Id:'1948cc7b-f806-4338-a9cd-5fe04f2a1a4b',Name:'Role18',IsEnabled:true}),
(r19:Role:CustomRole{Id:'16ba0405-077f-4425-9afa-d2c97a69ac0c',Name:'Role19',IsEnabled:true}),
(r20:Role:CustomRole{Id:'3bb49e8b-9415-48db-909d-05a123fb119d',Name:'Role20',IsEnabled:true}),
(r21:Role:TransitionalRole{Id:'636ac495-b50e-470b-8a04-71328284aed2',Name:'Role21',IsEnabled:true}),
(r22:Role:TransitionalRole{Id:'121109a1-5257-492a-a08a-271a1cefe178',Name:'Role22',IsEnabled:true}),
(r23:Role:TransitionalRole{Id:'202f3869-de16-4e3b-a971-16c51bcede07',Name:'Role23',IsEnabled:true}),
(r24:Role:TransitionalRole{Id:'1f8f8f8f-8f8f-8f8f-8f8f-8f8f8f8f8f8f',Name:'Role24',IsEnabled:true}),

(tr0:Role:TraffickerRole{Id: '9d754809-b7e1-460c-bff5-4528a5987590',Name:'TraffickerRole',IsEnabled:true}),
(tg0:Group{Id: '7ffa0eb7-a291-4ae7-be9e-17b0bf7d0cda',Name:'Adform-Trafficker',IsEnabled:true}),
(t0)-[:OWNS]->(tr0),
(t0)<-[:BELONGS]-(tg0)-[:ASSIGNED]->(tr0),
(tr1:Role:TraffickerRole{Id: '9d754819-b7e1-461c-bff5-4528a5987591',Name:'TraffickerRole',IsEnabled:true}),
(tg1:Group{Id: '7ffa1eb7-a291-4ae7-be9e-17b1bf7d1cda',Name:'Tenant1-Trafficker',IsEnabled:true}),
(t1)-[:OWNS]->(tr1),
(t1)<-[:BELONGS]-(tg1)-[:ASSIGNED]->(tr1),
(tr2:Role:TraffickerRole{Id: '9d754829-b7e1-462c-bff5-4528a5987592',Name:'TraffickerRole',IsEnabled:true}),
(tg2:Group{Id: '7ffa2eb7-a291-4ae7-be9e-17b2bf7d2cda',Name:'Tenant2-Trafficker',IsEnabled:true}),
(t2)-[:OWNS]->(tr2),
(t2)<-[:BELONGS]-(tg2)-[:ASSIGNED]->(tr2),
(tr3:Role:TraffickerRole{Id: '9d754839-b7e1-463c-bff5-4528a5987593',Name:'TraffickerRole',IsEnabled:true}),
(tg3:Group{Id: '7ffa3eb7-a291-4ae7-be9e-17b3bf7d3cda',Name:'Tenant3-Trafficker',IsEnabled:true}),
(t3)-[:OWNS]->(tr3),
(t3)<-[:BELONGS]-(tg3)-[:ASSIGNED]->(tr3),
(tr4:Role:TraffickerRole{Id: '9d754849-b7e1-464c-bff5-4528a5987594',Name:'TraffickerRole',IsEnabled:true}),
(tg4:Group{Id: '7ffa4eb7-a291-4ae7-be9e-17b4bf7d4cda',Name:'Tenant4-Trafficker',IsEnabled:true}),
(t4)-[:OWNS]->(tr4),
(t4)<-[:BELONGS]-(tg4)-[:ASSIGNED]->(tr4),
(tr5:Role:TraffickerRole{Id: '9d754859-b7e5-465c-bff5-4528a5987595',Name:'TraffickerRole',IsEnabled:true}),
(tg5:Group{Id: '7ffa5eb7-a295-4ae7-be9e-57b5bf7d5cda',Name:'Tenant5-Trafficker',IsEnabled:true}),
(t5)-[:OWNS]->(tr5),
(t5)<-[:BELONGS]-(tg5)-[:ASSIGNED]->(tr5),
(tr6:Role:TraffickerRole{Id: '9d754869-b7e6-466c-bff5-4528a5987596',Name:'TraffickerRole',IsEnabled:true}),
(tg6:Group{Id: '7ffa6eb7-a296-4ae7-be9e-67b6bf7d6cda',Name:'Tenant6-Trafficker',IsEnabled:true}),
(t6)-[:OWNS]->(tr6),
(t6)<-[:BELONGS]-(tg6)-[:ASSIGNED]->(tr6),
(tr7:Role:TraffickerRole{Id: '9d754879-b7e7-467c-bff5-4528a5987597',Name:'TraffickerRole',IsEnabled:true}),
(tg7:Group{Id: '7ffa7eb7-a297-4ae7-be9e-77b7bf7d7cda',Name:'Tenant7-Trafficker',IsEnabled:true}),
(t7)-[:OWNS]->(tr7),
(t7)<-[:BELONGS]-(tg7)-[:ASSIGNED]->(tr7),
(tr8:Role:TraffickerRole{Id: '9d754889-b7e8-468c-bff5-4528a5987598',Name:'TraffickerRole',IsEnabled:true}),
(tg8:Group{Id: '7ffa8eb7-a298-4ae7-be9e-87b8bf7d8cda',Name:'Tenant8-Trafficker',IsEnabled:true}),
(t8)-[:OWNS]->(tr8),
(t8)<-[:BELONGS]-(tg8)-[:ASSIGNED]->(tr8),
(tr9:Role:TraffickerRole{Id: '9d754899-b7e9-469c-bff5-4528a5987599',Name:'TraffickerRole',IsEnabled:true}),
(tg9:Group{Id: '7ffa9eb7-a299-4ae7-be9e-97b9bf7d9cda',Name:'Tenant9-Trafficker',IsEnabled:true}),
(t9)-[:OWNS]->(tr9),
(t9)<-[:BELONGS]-(tg9)-[:ASSIGNED]->(tr9),
(tr10:Role:TraffickerRole{Id: '9d754819-b7e1-461c-bff5-4528a5987510',Name:'TraffickerRole',IsEnabled:true}),
(tg10:Group{Id: '7ffa1eb7-a210-4ae7-be9e-17b1bf7d1cda',Name:'Tenant10-Trafficker',IsEnabled:true}),
(t10)-[:OWNS]->(tr1),
(t10)<-[:BELONGS]-(tg10)-[:ASSIGNED]->(tr10),
(tr11:Role:TraffickerRole{Id: '9d754819-b7e1-461c-bff5-4528a5987511',Name:'TraffickerRole',IsEnabled:true}),
(tg11:Group{Id: '7ffa1eb7-a211-4ae7-be9e-17b1bf7d1cda',Name:'Tenant11-Trafficker',IsEnabled:true}),
(t11)-[:OWNS]->(tr11),
(t11)<-[:BELONGS]-(tg11)-[:ASSIGNED]->(tr11),
(tr12:Role:TraffickerRole{Id: '9d754819-b7e1-461c-bff5-4528a5987512',Name:'TraffickerRole',IsEnabled:true}),
(tg12:Group{Id: '7ffa1eb7-a212-4ae7-be9e-17b1bf7d1cda',Name:'Tenant12-Trafficker',IsEnabled:true}),
(t12)-[:OWNS]->(tr12),
(t12)<-[:BELONGS]-(tg12)-[:ASSIGNED]->(tr12),
(tr13:Role:TraffickerRole{Id: '9d754819-b7e1-461c-bff5-4528a5987513',Name:'TraffickerRole',IsEnabled:true}),
(tg13:Group{Id: '7ffa1eb7-a213-4ae7-be9e-17b1bf7d1cda',Name:'Tensant13-Trafficker',IsEnabled:true}),
(t13)-[:OWNS]->(tr13),
(t13)<-[:BELONGS]-(tg13)-[:ASSIGNED]->(tr13),
(tr14:Role:TraffickerRole{Id: '9d754819-b7e1-461c-bff5-4528a5987514',Name:'TraffickerRole',IsEnabled:true}),
(tg14:Group{Id: '7ffa1eb7-a214-4ae7-be9e-17b1bf7d1cda',Name:'Tenant14-Trafficker',IsEnabled:true}),
(t14)-[:OWNS]->(tr14),
(t14)<-[:BELONGS]-(tg14)-[:ASSIGNED]->(tr14),
(tr15:Role:TraffickerRole{Id: '9d754819-b7e1-461c-bff5-4528a5987515',Name:'TraffickerRole',IsEnabled:true}),
(tg15:Group{Id: '7ffa1eb7-a215-4ae7-be9e-17b1bf7d1cda',Name:'Tenant15-Trafficker',IsEnabled:true}),
(t15)-[:OWNS]->(tr15),
(t15)<-[:BELONGS]-(tg15)-[:ASSIGNED]->(tr15),
(tr16:Role:TraffickerRole{Id: '9d754819-b7e1-461c-bff5-4528a5987516',Name:'TraffickerRole',IsEnabled:true}),
(tg16:Group{Id: '7ffa1eb7-a216-4ae7-be9e-17b1bf7d1cda',Name:'Tenant16-Trafficker',IsEnabled:true}),
(t16)-[:OWNS]->(tr16),
(t16)<-[:BELONGS]-(tg16)-[:ASSIGNED]->(tr16),

(p0)-[:CONTAINS]->(adformAdmin),
(p1)-[:CONTAINS]->(iamAdmin),
(p2)-[:CONTAINS]->(bsAdmin),
(p3)-[:CONTAINS]->(ssAdmin),
(p4)-[:CONTAINS]->(dmpAdmin),

(p1)-[:CONTAINS]->(localAdmin),
(p1)-[:CONTAINS]->(r0),
(p1)-[:CONTAINS]->(r1),
(p1)-[:CONTAINS]->(r2),
(p1)-[:CONTAINS]->(r3),
(p1)-[:CONTAINS]->(r4),

(p1)-[:CONTAINS]->(r5),
(p2)-[:CONTAINS]->(r6),
(p2)-[:CONTAINS]->(r7),
(p2)-[:CONTAINS]->(r8),
(p2)-[:CONTAINS]->(r9),
(p2)-[:CONTAINS]->(r10),
(p2)-[:CONTAINS]->(r11),

(p3)-[:CONTAINS]->(r12),
(p3)-[:CONTAINS]->(r13),
(p3)-[:CONTAINS]->(r14),
(p3)-[:CONTAINS]->(r15),
(p3)-[:CONTAINS]->(r16),
(p3)-[:CONTAINS]->(r17),

(p4)-[:CONTAINS]->(r18),
(p4)-[:CONTAINS]->(r19),
(p4)-[:CONTAINS]->(r20),
(p4)-[:CONTAINS]->(r21),
(p4)-[:CONTAINS]->(r22),
(p4)-[:CONTAINS]->(r23),
(p4)-[:CONTAINS]->(r24),

(tr0)<-[:CONTAINS]-(p0),
(tr1)<-[:CONTAINS]-(p2),
(tr2)<-[:CONTAINS]-(p2),
(tr3)<-[:CONTAINS]-(p3),
(tr4)<-[:CONTAINS]-(p3),
(tr5)<-[:CONTAINS]-(p4),
(tr6)<-[:CONTAINS]-(p4),
(tr7)<-[:CONTAINS]-(p2),
(tr8)<-[:CONTAINS]-(p2),
(tr9)<-[:CONTAINS]-(p2),
(tr10)<-[:CONTAINS]-(p2),
(tr11)<-[:CONTAINS]-(p3),
(tr12)<-[:CONTAINS]-(p3),
(tr13)<-[:CONTAINS]-(p4),
(tr14)<-[:CONTAINS]-(p4),
(tr15)<-[:CONTAINS]-(p4),
(tr16)<-[:CONTAINS]-(p4),

(adformAdmin)-[:CONTAINS]->(per0),
(iamAdmin)-[:CONTAINS]->(per1),
(iamAdmin)-[:CONTAINS]->(per2),
(iamAdmin)-[:CONTAINS]->(per3),
(iamAdmin)-[:CONTAINS]->(per4),
(bsAdmin)-[:CONTAINS]->(per5),
(bsAdmin)-[:CONTAINS]->(per6),
(bsAdmin)-[:CONTAINS]->(per7),
(bsAdmin)-[:CONTAINS]->(per8),
(ssAdmin)-[:CONTAINS]->(per9),
(ssAdmin)-[:CONTAINS]->(per10),
(ssAdmin)-[:CONTAINS]->(per11),
(ssAdmin)-[:CONTAINS]->(per12),
(dmpAdmin)-[:CONTAINS]->(per13),
(dmpAdmin)-[:CONTAINS]->(per14),
(dmpAdmin)-[:CONTAINS]->(per15),
(dmpAdmin)-[:CONTAINS]->(per16),

(localAdmin)-[:CONTAINS]->(per1),
(localAdmin)-[:CONTAINS]->(per2),
(localAdmin)-[:CONTAINS]->(per3),
(localAdmin)-[:CONTAINS]->(per4),
(r0)-[:CONTAINS]->(per1),
(r0)-[:CONTAINS]->(per2),
(r1)-[:CONTAINS]->(per1),
(r1)-[:CONTAINS]->(per2),
(r2)-[:CONTAINS]->(per3),
(r2)-[:CONTAINS]->(per4),
(r3)-[:CONTAINS]->(per3),
(r3)-[:CONTAINS]->(per4),
(r4)-[:CONTAINS]->(per3),
(r4)-[:CONTAINS]->(per4),
(r5)-[:CONTAINS]->(per3),
(r5)-[:CONTAINS]->(per4),

(r6)-[:CONTAINS]->(per5),
(r6)-[:CONTAINS]->(per6),
(r7)-[:CONTAINS]->(per5),
(r7)-[:CONTAINS]->(per6),
(r8)-[:CONTAINS]->(per7),
(r8)-[:CONTAINS]->(per8),
(r9)-[:CONTAINS]->(per7),
(r9)-[:CONTAINS]->(per8),
(r10)-[:CONTAINS]->(per7),
(r10)-[:CONTAINS]->(per8),
(r11)-[:CONTAINS]->(per5),
(r11)-[:CONTAINS]->(per6),
(r11)-[:CONTAINS]->(per7),
(r11)-[:CONTAINS]->(per8),

(r12)-[:CONTAINS]->(per9),
(r12)-[:CONTAINS]->(per10),
(r13)-[:CONTAINS]->(per9),
(r13)-[:CONTAINS]->(per10),
(r14)-[:CONTAINS]->(per9),
(r14)-[:CONTAINS]->(per10),
(r15)-[:CONTAINS]->(per11),
(r15)-[:CONTAINS]->(per12),
(r16)-[:CONTAINS]->(per11),
(r16)-[:CONTAINS]->(per12),
(r17)-[:CONTAINS]->(per9),
(r17)-[:CONTAINS]->(per10),
(r17)-[:CONTAINS]->(per11),
(r17)-[:CONTAINS]->(per12),

(r18)-[:CONTAINS]->(per13),
(r18)-[:CONTAINS]->(per14),
(r19)-[:CONTAINS]->(per13),
(r19)-[:CONTAINS]->(per14),
(r20)-[:CONTAINS]->(per13),
(r20)-[:CONTAINS]->(per14),
(r21)-[:CONTAINS]->(per15),
(r21)-[:CONTAINS]->(per16),
(r22)-[:CONTAINS]->(per15),
(r22)-[:CONTAINS]->(per16),
(r23)-[:CONTAINS]->(per15),
(r23)-[:CONTAINS]->(per16),
(r24)-[:CONTAINS]->(per13),
(r24)-[:CONTAINS]->(per14),
(r24)-[:CONTAINS]->(per15),
(r24)-[:CONTAINS]->(per16),

(tr0)-[:CONTAINS]->(per0),
(tr0)-[:CONTAINS]->(per1),
(tr0)-[:CONTAINS]->(per2),
(tr0)-[:CONTAINS]->(per3),
(tr0)-[:CONTAINS]->(per4),
(tr0)-[:CONTAINS]->(per5),
(tr0)-[:CONTAINS]->(per6),
(tr0)-[:CONTAINS]->(per7),
(tr0)-[:CONTAINS]->(per8),
(tr0)-[:CONTAINS]->(per9),
(tr0)-[:CONTAINS]->(per10),
(tr0)-[:CONTAINS]->(per11),
(tr0)-[:CONTAINS]->(per12),
(tr0)-[:CONTAINS]->(per13),
(tr0)-[:CONTAINS]->(per14),
(tr0)-[:CONTAINS]->(per15),
(tr0)-[:CONTAINS]->(per16),

(tr1)-[:CONTAINS]->(per5),
(tr1)-[:CONTAINS]->(per6),
(tr1)-[:CONTAINS]->(per7),
(tr1)-[:CONTAINS]->(per8),

(tr2)-[:CONTAINS]->(per5),
(tr2)-[:CONTAINS]->(per6),
(tr2)-[:CONTAINS]->(per7),
(tr2)-[:CONTAINS]->(per8),

(tr3)-[:CONTAINS]->(per9),
(tr3)-[:CONTAINS]->(per10),
(tr3)-[:CONTAINS]->(per11),
(tr3)-[:CONTAINS]->(per12),

(tr4)-[:CONTAINS]->(per9),
(tr4)-[:CONTAINS]->(per10),
(tr4)-[:CONTAINS]->(per11),
(tr4)-[:CONTAINS]->(per12),

(tr5)-[:CONTAINS]->(per13),
(tr5)-[:CONTAINS]->(per14),
(tr5)-[:CONTAINS]->(per15),
(tr5)-[:CONTAINS]->(per16),

(tr6)-[:CONTAINS]->(per13),
(tr6)-[:CONTAINS]->(per14),
(tr6)-[:CONTAINS]->(per15),
(tr6)-[:CONTAINS]->(per16),

(tr7)-[:CONTAINS]->(per5),
(tr7)-[:CONTAINS]->(per6),
(tr7)-[:CONTAINS]->(per7),
(tr7)-[:CONTAINS]->(per8),

(tr8)-[:CONTAINS]->(per5),
(tr8)-[:CONTAINS]->(per6),
(tr8)-[:CONTAINS]->(per7),
(tr8)-[:CONTAINS]->(per8),

(tr9)-[:CONTAINS]->(per5),
(tr9)-[:CONTAINS]->(per6),
(tr9)-[:CONTAINS]->(per7),
(tr9)-[:CONTAINS]->(per8),

(tr10)-[:CONTAINS]->(per9),
(tr10)-[:CONTAINS]->(per10),
(tr10)-[:CONTAINS]->(per11),
(tr10)-[:CONTAINS]->(per12),

(tr11)-[:CONTAINS]->(per9),
(tr11)-[:CONTAINS]->(per10),
(tr11)-[:CONTAINS]->(per11),
(tr11)-[:CONTAINS]->(per12),

(tr12)-[:CONTAINS]->(per9),
(tr12)-[:CONTAINS]->(per10),
(tr12)-[:CONTAINS]->(per11),
(tr12)-[:CONTAINS]->(per12),

(tr13)-[:CONTAINS]->(per13),
(tr13)-[:CONTAINS]->(per14),
(tr13)-[:CONTAINS]->(per15),
(tr13)-[:CONTAINS]->(per16),

(tr14)-[:CONTAINS]->(per13),
(tr14)-[:CONTAINS]->(per14),
(tr14)-[:CONTAINS]->(per15),
(tr14)-[:CONTAINS]->(per16),

(tr15)-[:CONTAINS]->(per13),
(tr15)-[:CONTAINS]->(per14),
(tr15)-[:CONTAINS]->(per15),
(tr15)-[:CONTAINS]->(per16),

(tr16)-[:CONTAINS]->(per13),
(tr16)-[:CONTAINS]->(per14),
(tr16)-[:CONTAINS]->(per15),
(tr16)-[:CONTAINS]->(per16),

(t0)-[:OWNS]->(r1),
(t0)-[:OWNS]->(r2),
(t0)-[:OWNS]->(r3),
(t0)-[:OWNS]->(r4),
(t0)-[:OWNS]->(r5),

(t15)-[:OWNS]->(r19),
(t16)-[:OWNS]->(r22),
(t16)-[:OWNS]->(r23),

(t4)-[:OWNS]->(r16),
(t4)-[:OWNS]->(r13),

(t1)-[:OWNS]->(r7),
(t8)-[:OWNS]->(r9),
(t8)-[:OWNS]->(r11),


(g0:Group{Id:'a1b22f28-1111-2222-3333-689f2d107d04',Name:'Adform-AdformAdmin',IsEnabled:true}),
(t0)-[:OWNS]->(adformAdmin),
(g0)-[:ASSIGNED]->(adformAdmin),
(g0)-[:BELONGS]->(t0),
(g0)<-[:MEMBER_OF]-(s0),

(t0)-[:OWNS]->(localAdmin),

(g1:Group{Id:'a1b22f28-4516-4005-854d-689f2d107d04',Name:'Tenant0-Role0',IsEnabled:true}),
(t0)-[:OWNS]->(r0),
(g1)-[:ASSIGNED]->(r0),
(g1)-[:BELONGS]->(t0),
(g1)<-[:MEMBER_OF]-(s1),

(g2:Group{Id:'ce4c5cc9-cbb4-4b48-86fb-beaf25b554ba',Name:'Tenant2-Role6',IsEnabled:true}),
(t2)-[:OWNS]->(r6),
(g2)-[:ASSIGNED]->(r6),
(g2)-[:BELONGS]->(t2),
(g2)<-[:MEMBER_OF]-(s2),
(g2)<-[:MEMBER_OF]-(s3),

(g3:Group{Id:'a1b23f28-4516-4005-854d-689f2d107d04',Name:'Tenant7-Role8',IsEnabled:true}),
(t7)-[:OWNS]->(r8),
(g3)-[:ASSIGNED]->(r8),
(g3)-[:BELONGS]->(t7),
(g3)<-[:MEMBER_OF]-(s4),

(g4:Group{Id:'736a8758-255c-4ead-948a-802c3b4e9e28',Name:'Tenant3-Role17',IsEnabled:true}),
(t3)-[:OWNS]->(r17),
(g4)-[:ASSIGNED]->(r17),
(g4)-[:BELONGS]->(t3),
(g4)<-[:MEMBER_OF]-(s5),

(g5:Group{Id:'736a8758-255c-4ead-948a-802c3b4e9e20',Name:'Tenant12-Role15',IsEnabled:true}),
(t12)-[:OWNS]->(r15),
(g5)-[:ASSIGNED]->(r15),
(g5)-[:BELONGS]->(t12),
(g5)<-[:MEMBER_OF]-(s6),

(g6:Group{Id:'a1b22f28-4516-4005-854d-689f2d107d00',Name:'Tenant11-Role12',IsEnabled:true}),
(t11)-[:OWNS]->(r12),
(g6)-[:ASSIGNED]->(r12),
(g6)-[:BELONGS]->(t11),
(g6)<-[:MEMBER_OF]-(s7),

(g7:Group{Id:'f58d131b-5bb4-404e-913a-197ab433f601',Name:'Tenant5-Role24',IsEnabled:true}),
(t5)-[:OWNS]->(r24),
(g7)-[:ASSIGNED]->(r24),
(g7)-[:BELONGS]->(t5),
(g7)<-[:MEMBER_OF]-(s8),

(g8:Group{Id:'91841a35-e733-4a58-ac9b-88e70b8544b7',Name:'Tenant14-Role18',IsEnabled:true}),
(t14)-[:OWNS]->(r18),
(g8)-[:ASSIGNED]->(r18),
(g8)-[:BELONGS]->(t14),
(g8)<-[:MEMBER_OF]-(s9),

(g9:Group{Id:'649130d8-24fb-4132-8c0a-9933905ce446',Name:'Tenant13-Role20',IsEnabled:true}),
(t13)-[:OWNS]->(r20),
(g9)-[:ASSIGNED]->(r20),
(g9)-[:BELONGS]->(t13),
(g9)<-[:MEMBER_OF]-(s10),

(g10:Group{Id:'0a9cde70-f3f7-499b-bb43-551ddcce2468',Name:'Tenant9-Role10',IsEnabled:true}),
(t9)-[:OWNS]->(r10),
(g10)-[:ASSIGNED]->(r10),
(g10)-[:BELONGS]->(t9),
(g10)<-[:MEMBER_OF]-(ts0),

(g11:Group{Id:'02dc5e28-87c5-4956-9930-f698153aa6d2',Name:'Tenant10-Role14',IsEnabled:true}),
(t10)-[:OWNS]->(r14),
(g11)-[:ASSIGNED]->(r14),
(g11)-[:BELONGS]->(t10),
(g11)<-[:MEMBER_OF]-(ts1),

(g12:Group{Id:'23dd7826-ad13-4a5d-90c8-dc193308fdd4', Name:'Tenant6-Role21', IsEnabled:true}),
(t6)-[:OWNS]->(r21),
(g12)-[:ASSIGNED]->(r21),
(g12)-[:BELONGS]->(t6),
(g12)<-[:MEMBER_OF]-(ts2),

(g13:Group{Id:'2177ad91-9789-4e9e-a253-d1ab5befd9ef',Name:'Tenant1-LocalAdmin',IsEnabled:true}),
(g14:Group{Id:'e11c35d1-8ca6-414f-b57c-b8ab49b0f93e',Name:'Tenant3-LocalAdmin',IsEnabled:true}),
(g15:Group{Id:'2024c86e-15ce-4f09-84d1-066795cff283',Name:'Tenant5-LocalAdmin',IsEnabled:true}),
(g13)-[:ASSIGNED]->(localAdmin),
(g14)-[:ASSIGNED]->(localAdmin),
(g15)-[:ASSIGNED]->(localAdmin),
(g13)-[:BELONGS]->(t1),
(g14)-[:BELONGS]->(t3),
(g15)-[:BELONGS]->(t5),

(g13)<-[:MEMBER_OF]-(ts0),
(g14)<-[:MEMBER_OF]-(ts1),
(g15)<-[:MEMBER_OF]-(ts2),

(tg1)<-[:MEMBER_OF]-(ts0),
(tg5)<-[:MEMBER_OF]-(ts1),
(tg8)<-[:MEMBER_OF]-(ts2),

(g16:Group{Id:'967a74e6-9d6d-4274-834d-b626c2421e3e',Name:'Tenant2-LocalAdmin',IsEnabled:true}),
(g17:Group{Id:'13d4fe0e-749a-4fd3-8a1e-ee1862407b15',Name:'Tenant4-LocalAdmin',IsEnabled:true}),
(g18:Group{Id:'dd84fba4-e576-4227-9b87-93103020b8a4',Name:'Tenant6-LocalAdmin',IsEnabled:true}),
(g16)-[:ASSIGNED]->(localAdmin),
(g17)-[:ASSIGNED]->(localAdmin),
(g18)-[:ASSIGNED]->(localAdmin),
(g16)-[:BELONGS]->(t2),
(g17)-[:BELONGS]->(t4),
(g18)-[:BELONGS]->(t6),
(g16)<-[:MEMBER_OF]-(s3),
(g17)<-[:MEMBER_OF]-(s5),
(g18)<-[:MEMBER_OF]-(s8)