// ************ Finding features ************

MERGE(f0: Feature { Id: 'ae06853f-9d0f-40bf-92ae-4d59d6202c9d'})
MERGE(f1: Feature { Id: '1c379b1d-3f40-42d2-bc2b-503e6c73b6af'})
MERGE(f2: Feature { Id: '62bb4d2a-c6fe-4a9c-97c3-fe7d5f1b4e51'})
MERGE(f3: Feature { Id: '06b333c6-02f1-46fa-ac91-e0014453ea08'})
MERGE(f4: Feature { Id: 'ee06c53d-d063-4861-b9d5-ce513c5497b5'})
MERGE(f5: Feature { Id: '923e0b42-2ab8-41d4-8fdc-a285a860e1e5'})
MERGE(f6: Feature { Id: 'ac983a27-afa6-4156-bce6-f50c80a07dfd'})
MERGE(f7: Feature { Id: '6dba6883-b6d4-4ee0-9104-ce5d7dd37da8'})
MERGE(f8: Feature { Id: 'a80fc120-3ddf-4fa8-87fd-efbe3cef9cbe'})
MERGE(f9: Feature { Id: '89811fae-349d-44b6-bade-c322e6842b5e'})
MERGE(f10: Feature { Id: '4dd1d841-ad0b-4824-b237-6c6e92f2146d'})
MERGE(f11: Feature { Id: '22ba227e-abe2-4a20-843b-f808087d4d35'})
MERGE(f12: Feature { Id: '24965066-aaa2-4883-9de5-750be3687b2b'})
MERGE(f13: Feature { Id: '5b2ba69e-2af9-47c8-9cb7-d156f99ee586'})
MERGE(f14: Feature { Id: '48045138-19ac-429d-8a79-3cab3a5ce46a'})
MERGE(f15: Feature { Id: 'dfbfaa4d-68a0-4206-9a5e-a78729452035'})
MERGE(f16: Feature { Id: '131b93f3-f058-4069-877d-fa659009f349'})
MERGE(f17: Feature { Id: '561bd06e-86d8-445d-9c1f-6a8f683e1ced'})
MERGE(f18: Feature { Id: '5690e77d-bf62-4fec-8a4a-cbf6ada92658'})
MERGE(f19: Feature { Id: '96219d73-5b23-43a6-8dad-2899a6872d3c'})
MERGE(f20: Feature { Id: '2132108d-8ef4-445a-87c1-f9ce46833e65'})
MERGE(f21: Feature { Id: 'be531325-66e7-4e1e-a1de-67bb95a4168a'})
MERGE(f22: Feature { Id: '596d0bcd-6c75-465e-981f-e4e02968b04d'})
MERGE(f23: Feature { Id: '5c31ba04-9c63-44df-aa23-fc669afd3ba4'})
MERGE(f24: Feature { Id: 'a324eb13-e100-4b5e-8a31-0e73f3988df2'})
MERGE(f25: Feature { Id: 'd1a0d158-d0b7-44d6-ab39-a82eea73d9fd'})
MERGE(f26: Feature { Id: 'a8f298f2-d8df-46e1-9fd8-348b6c00c518'})
MERGE(f27: Feature { Id: '9b3876dd-b2c3-47c0-88d4-57c1d49da98f'})
MERGE(f28: Feature { Id: '0642bac1-f5e7-4aef-aade-31097888be35'})
MERGE(f29: Feature { Id: 'd0168429-3a60-406a-8fe5-ff8457a460e4'})
MERGE(f30: Feature { Id: '8b439e77-55fd-4f24-8da9-1b3f3b5b1b3d'})

// ************ Features for Adform tenant ************

MERGE (adform: Tenant{ Name: 'Adform'})
CREATE (adform)-[:ASSIGNED]->(f0)
CREATE (adform)-[:ASSIGNED]->(f1)
CREATE (adform)-[:ASSIGNED]->(f2)
CREATE (adform)-[:ASSIGNED]->(f3)
CREATE (adform)-[:ASSIGNED]->(f4)
CREATE (adform)-[:ASSIGNED]->(f5)
CREATE (adform)-[:ASSIGNED]->(f6)
CREATE (adform)-[:ASSIGNED]->(f7)
CREATE (adform)-[:ASSIGNED]->(f8)
CREATE (adform)-[:ASSIGNED]->(f9)
CREATE (adform)-[:ASSIGNED]->(f10)
CREATE (adform)-[:ASSIGNED]->(f11)
CREATE (adform)-[:ASSIGNED]->(f12)
CREATE (adform)-[:ASSIGNED]->(f13)
CREATE (adform)-[:ASSIGNED]->(f14)
CREATE (adform)-[:ASSIGNED]->(f15)
CREATE (adform)-[:ASSIGNED]->(f16)
CREATE (adform)-[:ASSIGNED]->(f17)
CREATE (adform)-[:ASSIGNED]->(f18)
CREATE (adform)-[:ASSIGNED]->(f19)
CREATE (adform)-[:ASSIGNED]->(f20)
CREATE (adform)-[:ASSIGNED]->(f21)
CREATE (adform)-[:ASSIGNED]->(f22)
CREATE (adform)-[:ASSIGNED]->(f23)
CREATE (adform)-[:ASSIGNED]->(f24)
CREATE (adform)-[:ASSIGNED]->(f25)
CREATE (adform)-[:ASSIGNED]->(f26)
CREATE (adform)-[:ASSIGNED]->(f27)
CREATE (adform)-[:ASSIGNED]->(f28)
CREATE (adform)-[:ASSIGNED]->(f29)
CREATE (adform)-[:ASSIGNED]->(f30)

// ************ Features for default tenants ************

MERGE (iKEA: Tenant{ Name: 'IKEA'})
CREATE (iKEA)-[:ASSIGNED]->(f0)
CREATE (iKEA)-[:ASSIGNED]->(f3)
CREATE (iKEA)-[:ASSIGNED]->(f5)
CREATE (iKEA)-[:ASSIGNED]->(f6)
CREATE (iKEA)-[:ASSIGNED]->(f7)
CREATE (iKEA)-[:ASSIGNED]->(f9)
CREATE (iKEA)-[:ASSIGNED]->(f10)
CREATE (iKEA)-[:ASSIGNED]->(f11)
CREATE (iKEA)-[:ASSIGNED]->(f12)
CREATE (iKEA)-[:ASSIGNED]->(f13)
CREATE (iKEA)-[:ASSIGNED]->(f15)
CREATE (iKEA)-[:ASSIGNED]->(f19)
CREATE (iKEA)-[:ASSIGNED]->(f20)
CREATE (iKEA)-[:ASSIGNED]->(f21)
CREATE (iKEA)-[:ASSIGNED]->(f24)
CREATE (iKEA)-[:ASSIGNED]->(f26)
CREATE (iKEA)-[:ASSIGNED]->(f27)
CREATE (iKEA)-[:ASSIGNED]->(f28)
CREATE (iKEA)-[:ASSIGNED]->(f29)
CREATE (iKEA)-[:ASSIGNED]->(f30)

MERGE (nike: Tenant{ Name: 'Nike'})
CREATE (nike)-[:ASSIGNED]->(f0)
CREATE (nike)-[:ASSIGNED]->(f2)
CREATE (nike)-[:ASSIGNED]->(f13)
CREATE (nike)-[:ASSIGNED]->(f15)
CREATE (nike)-[:ASSIGNED]->(f17)
CREATE (nike)-[:ASSIGNED]->(f18)
CREATE (nike)-[:ASSIGNED]->(f22)
CREATE (nike)-[:ASSIGNED]->(f23)

MERGE (apple: Tenant{ Name: 'Apple'})
CREATE (apple)-[:ASSIGNED]->(f0)
CREATE (apple)-[:ASSIGNED]->(f3)
CREATE (apple)-[:ASSIGNED]->(f5)
CREATE (apple)-[:ASSIGNED]->(f7)
CREATE (apple)-[:ASSIGNED]->(f10)
CREATE (apple)-[:ASSIGNED]->(f13)
CREATE (apple)-[:ASSIGNED]->(f15)
CREATE (apple)-[:ASSIGNED]->(f18)
CREATE (apple)-[:ASSIGNED]->(f20)
CREATE (apple)-[:ASSIGNED]->(f22)
CREATE (apple)-[:ASSIGNED]->(f23)
CREATE (apple)-[:ASSIGNED]->(f24)
CREATE (apple)-[:ASSIGNED]->(f25)
CREATE (apple)-[:ASSIGNED]->(f26)
CREATE (apple)-[:ASSIGNED]->(f30)

MERGE (microsoft: Tenant{ Name: 'Microsoft'})
CREATE (microsoft)-[:ASSIGNED]->(f1)
CREATE (microsoft)-[:ASSIGNED]->(f3)
CREATE (microsoft)-[:ASSIGNED]->(f4)
CREATE (microsoft)-[:ASSIGNED]->(f6)
CREATE (microsoft)-[:ASSIGNED]->(f7)
CREATE (microsoft)-[:ASSIGNED]->(f8)
CREATE (microsoft)-[:ASSIGNED]->(f9)
CREATE (microsoft)-[:ASSIGNED]->(f13)
CREATE (microsoft)-[:ASSIGNED]->(f14)
CREATE (microsoft)-[:ASSIGNED]->(f15)
CREATE (microsoft)-[:ASSIGNED]->(f17)
CREATE (microsoft)-[:ASSIGNED]->(f19)
CREATE (microsoft)-[:ASSIGNED]->(f21)
CREATE (microsoft)-[:ASSIGNED]->(f22)
CREATE (microsoft)-[:ASSIGNED]->(f23)
CREATE (microsoft)-[:ASSIGNED]->(f25)
CREATE (microsoft)-[:ASSIGNED]->(f26)
CREATE (microsoft)-[:ASSIGNED]->(f28)
CREATE (microsoft)-[:ASSIGNED]->(f29)

