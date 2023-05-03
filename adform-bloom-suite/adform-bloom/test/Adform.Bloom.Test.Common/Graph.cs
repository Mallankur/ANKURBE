using System;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Domain.Entities;
using Xunit;
using Feature = Adform.Bloom.Domain.Entities.Feature;

namespace Adform.Bloom.Common.Test
{
    public static class Graph
    {
        public const string Policy0 = "647e0b74-b9f3-4e1b-934e-ae9fac0c335d";
        public const string Policy0Name = "Policy0";
        public const string Policy1 = "90e21677-cd27-49b6-a76f-0ba1779845a5";
        public const string Policy1Name = "Policy1";
        public const string Policy2 = "acd5fd66-d8ee-4a79-967a-4c955160f751";  
        public const string Policy2Name = "Policy2";
        public const string Policy3 = "8ae1dfec-7c07-4e6e-8bce-fdc20519fc02";
        public const string Policy3Name = "Policy3";
        public const string Policy4 = "2fa850c8-b76d-48f7-8aac-b4871d01e4f8";
        public const string Policy4Name = "Policy4";

        public const string Tenant0 = "bd926f81-0316-4a1a-b6ba-b717ccc1ff1d";
        public const string Tenant0Name = "Adform";
        public const string Tenant1 = "ea0d54a3-1d54-4410-a2a0-55548baa4587";
        public const string Tenant1Name = "Tenant1";
        public const string Tenant2 = "49195836-874c-4db2-b954-a4ee70717eab";
        public const string Tenant2Name = "Tenant2";
        public const string Tenant3 = "108b9151-15d3-4cbb-b4ec-fef42c4c5567";
        public const string Tenant3Name = "Tenant3";
        public const string Tenant4 = "287f9d9d-5917-4721-8e24-fa4e352099f1";
        public const string Tenant4Name = "Tenant4";
        public const string Tenant5 = "b214781b-5235-42c1-a89f-93a30c364eef";
        public const string Tenant5Name = "Tenant5";
        public const string Tenant6 = "0748600c-e328-48df-8ec9-be52af4f0889";
        public const string Tenant6Name = "Tenant6";
        public const string Tenant7 = "858e4425-72ca-4113-8fe9-27fa5b034878";
        public const string Tenant7Name = "Tenant7";
        public const string Tenant8 = "d2b69ddb-fe00-4c5d-916f-b71877dac4c8";
        public const string Tenant8Name = "Tenant8";
        public const string Tenant9 = "ce006fd5-3339-40de-b553-d12c2a136b3c";
        public const string Tenant9Name = "Tenant9";
        public const string Tenant10 = "d5a39696-5c2e-4047-b5c4-e318b81bbc61";
        public const string Tenant10Name = "Tenant10";
        public const string Tenant11 = "7bbd00be-ab52-4e7f-b75e-dd91276dc360";
        public const string Tenant11Name = "Tenant11";
        public const string Tenant12 = "ab5ad0f7-7643-467e-9f26-a6ddede2e268";
        public const string Tenant12Name = "Tenant12";
        public const string Tenant13 = "8ffc490c-a173-4499-92b3-686e2fe32bf8";
        public const string Tenant13Name = "Tenant13";
        public const string Tenant14 = "c8231bb6-a453-4683-835b-4c972e62b5f9";
        public const string Tenant14Name = "Tenant14";
        public const string Tenant15 = "7ff1c7cc-ab29-4e24-aa8b-28c415e025e0";
        public const string Tenant15Name = "Tenant15";
        public const string Tenant16 = "a9f07c1d-f137-4aa3-a5f6-e223a81da4b2";
        public const string Tenant16Name = "Tenant16";

        public const string SubjectUsedByBloomApi = Subject0;
        public const string Subject0 = "bf15ab64-ff92-4143-99df-2f409652e2e3";
        public const string Subject0Name = "Subject0";
        public const string Subject1 = "050c54d3-a928-4430-baa2-910a9526c505";
        public const string Subject1Name = "Subject1";
        public const string Subject2 = "058c54d3-a928-4430-baa2-910a9526c505";
        public const string Subject2Name = "Subject2";
        public const string Subject3 = "058c54d3-a928-4430-baa2-950a9526c505";
        public const string Subject3Name = "Subject3";
        public const string Subject4 = "058c54d3-a928-4430-1111-910a9526c000";
        public const string Subject4Name = "Subject4";
        public const string Subject5 = "e32539ac-3c97-48c6-80a5-cdf78635148a";
        public const string Subject5Name = "Subject5";
        public const string Subject6 = "e32539ac-3c97-48c6-80a5-cdf78635148b";
        public const string Subject6Name = "Subject6";
        public const string Subject7 = "e65399fe-b720-436c-b21e-f97ed9c968cb";
        public const string Subject7Name = "Subject7";
        public const string Subject8 = "d6d5f3e8-5578-464e-a7a6-f5107102e527";
        public const string Subject8Name = "Subject8";
        public const string Subject9 = "3edebd10-2c37-42fe-8216-5053efb0f511";
        public const string Subject9Name = "Subject9";
        public const string Subject10 = "a7c1e30f-1469-4df1-8e4b-144716b5d656";
        public const string Subject10Name = "Subject10";
        public const string NonExistentSubject = "474107ef-67a1-4c97-bb82-457ac1d90463";
        public const string NonExistentSubjectName = "NonExistentSubject";

        public const string Trafficker0 = "82c142d0-0533-4302-b09a-a0fb5373942c";
        public const string Trafficker0Name = "Trafficker0";
        public const string Trafficker1 = "b2500bf1-45e9-4b8f-9be4-a8c8dd52a9f0";
        public const string Trafficker1Name = "Trafficker1";
        public const string Trafficker2 = "74936140-fbdb-468c-8cff-891bdde5dd17";
        public const string Trafficker2Name = "Trafficker2";
        public const string TraffickerRoleName = "TraffickerRole";
        public const string Trafficker0Role = "9d754809-b7e1-460c-bff5-4528a5987590";
        public const string Trafficker1Role = "9d754819-b7e1-461c-bff5-4528a5987591";
        public const string Trafficker2Role = "9d754829-b7e1-462c-bff5-4528a5987592";
        public const string Trafficker3Role = "9d754839-b7e1-463c-bff5-4528a5987593";
        public const string Trafficker4Role = "9d754849-b7e1-464c-bff5-4528a5987594";
        public const string Trafficker5Role = "9d754859-b7e5-465c-bff5-4528a5987595";
        public const string Trafficker6Role = "9d754869-b7e6-466c-bff5-4528a5987596";
        public const string Trafficker7Role = "9d754879-b7e7-467c-bff5-4528a5987597";
        public const string Trafficker8Role = "9d754889-b7e8-468c-bff5-4528a5987598";
        public const string Trafficker9Role = "9d754899-b7e9-469c-bff5-4528a5987599";
        public const string Trafficker10Role = "9d754819-b7e1-461c-bff5-4528a5987510";
        public const string Trafficker11Role = "9d754819-b7e1-461c-bff5-4528a5987511";
        public const string Trafficker12Role = "9d754819-b7e1-461c-bff5-4528a5987512";
        public const string Trafficker13Role = "9d754819-b7e1-461c-bff5-4528a5987513";
        public const string Trafficker14Role = "9d754819-b7e1-461c-bff5-4528a5987514";
        public const string Trafficker15Role = "9d754819-b7e1-461c-bff5-4528a5987515";
        public const string Trafficker16Role = "9d754819-b7e1-461c-bff5-4528a5987516";

        public const string Feature0 = "98b3bd33-5f69-4dc9-aa89-8c2b0af900df";
        public const string Feature0Name = "Feature0";
        public const string Feature1 = "f86af37f-c992-4ed7-b469-5ae4c15f95c6";
        public const string Feature1Name = "Feature1";
        public const string Feature2 = "885b0e53-e44a-4181-bb79-e8844b655515";
        public const string Feature2Name = "Feature2";
        public const string Feature3 = "1cf6d35c-8336-4cec-af2c-a85b3dc745cb";
        public const string Feature3Name = "Feature3";
        public const string Feature4 = "196a53a8-4390-473f-af31-bed82910558b";
        public const string Feature4Name = "Feature4";
        public const string Feature5 = "8e4bb6b6-7418-4db6-bec8-e27b4f227faa";
        public const string Feature5Name = "Feature5";
        public const string Feature6 = "b65dc109-8dae-4e88-9372-6de3a9516229";
        public const string Feature6Name = "Feature6";
        public const string Feature7 = "4f838b42-dc83-4bb0-8420-c8f713da6d98";
        public const string Feature7Name = "Feature7";
        public const string Feature8 = "9781ba27-c5f3-4f00-9033-bc10ce91c9f6";
        public const string Feature8Name = "Feature8";

        public const string LicensedFeature0 = "d2b69ddb-fe00-4c5d-916f-b90877dac4c8";
        public const string LicensedFeature0Name = "LicensedFeature0";
        public const string LicensedFeature1 = "c2b69ddb-fa00-5c5d-916f-b90877dac4c8";
        public const string LicensedFeature1Name = "LicensedFeature1";
        public const string LicensedFeature2 = "c1b69ddb-fa00-5c5d-916f-b90877dac4c8";
        public const string LicensedFeature2Name = "LicensedFeature2";
        public const string LicensedFeature3 = "875964d3-f5d1-458b-b1c9-9c3a88d6b657";
        public const string LicensedFeature3Name = "LicensedFeature3";
        public const string LicensedFeature4 = "7a0d75ad-9702-4c02-a358-bf367e840754";
        public const string LicensedFeature4Name = "LicensedFeature4";

        public const string AdformAdmin = "caa9d05f-20c4-42b3-9c06-37c53bb0ae84";
        public const string AdformAdminRoleName = ClaimPrincipalExtensions.AdformAdmin;
        public const string IamAdmin = "caa9d05f-24c4-42b3-9c06-37c53bb0ae84";
        public const string IamAdminRoleName = "IAM Admin";
        public const string SellSideAdmin = "caa9d05f-22c4-42b3-9c06-37c53bb0ae84";
        public const string SellSideAdminRoleName = "Supply Side Admin";
        public const string BuySideAdmin = "caa9d05f-21c4-42b3-9c06-37c53bb0ae84";
        public const string BuySideAdminRoleName = "Demand Side Admin";
        public const string DmpAdmin = "caa9d05f-23c4-42b3-9c06-37c53bb0ae84";
        public const string DmpAdminRoleName = "Data Management Admin";
        public const string OtherRole = "admin";

        public const string LocalAdmin = "56bd1ad3-fd40-4200-84ee-62e1af1bd694";
        public const string LocalAdminRoleName = ClaimPrincipalExtensions.LocalAdmin;

        public const string CustomRole0 = "a04f8e43-bbfe-4cf5-8b1b-36faa08e946f";
        public const string CustomRole0Name = "Role0";
        public const string Role1 = "c24e8b1c-1815-499e-9331-628558090776";
        public const string Role1Name = "Role1";
        public const string Role2 = "80d5dbcc-8560-4538-a0ea-73604b2b4c63";
        public const string Role2Name = "Role2";
        public const string Role3 = "a706d1bc-9f78-4744-b8db-5772c4328416";
        public const string Role3Name = "Role3";
        public const string CustomRole4 = "386e1fbd-bd88-46a2-9a83-e1fd34e47189";
        public const string CustomRole4Name = "Role4";
        public const string CustomRole5 = "69afa7ed-7dda-497f-a0cc-7b144366b6ed";
        public const string CustomRole5Name = "Role5";
        public const string CustomRole6 = "58298ba8-b278-482d-b520-7dbff17875e6";
        public const string CustomRole6Name = "Role6";
        public const string CustomRole7 = "0a3a073f-dec3-4434-a357-86532b4f6195";
        public const string CustomRole7Name = "Role7";
        public const string CustomRole8 = "db95cb4d-25e4-4167-9516-4558cc880805";
        public const string CustomRole8Name = "Role8";
        public const string CustomRole9 = "893b3f49-9f5b-4cfb-afd9-2149b9344064";
        public const string CustomRole9Name = "Role9";
        public const string CustomRole10 = "69afa7ed-7dda-497f-a0cc-7b144366b6e0";
        public const string CustomRole10Name = "Role10";
        public const string CustomRole11 = "1be08cb0-ffec-4a45-8f09-6f756eaf10df";
        public const string CustomRole11Name = "Role11";
        public const string CustomRole12 = "2ac73707-d2cd-4773-be06-46620e47dbc2";
        public const string CustomRole12Name = "Role12";
        public const string CustomRole13 = "0ee13ad6-e26a-43fe-b9f1-1e4bf25b1c05";
        public const string CustomRole13Name = "Role13";
        public const string CustomRole14 = "01276c88-3a1e-4d69-8d79-fb2d860f199f";
        public const string CustomRole14Name = "Role14";
        public const string CustomRole15 = "198b07a2-6a5d-4b00-9c46-ae3056f7c2f2";
        public const string CustomRole15Name = "Role15";
        public const string CustomRole16 = "cf038c38-8475-4045-ac2a-fc26e930782c";
        public const string CustomRole16Name = "Role16";
        public const string CustomRole17 = "ca0ca9cb-12b8-409a-a32f-1770f19a072b";
        public const string CustomRole17Name = "Role17";
        public const string CustomRole18 = "1948cc7b-f806-4338-a9cd-5fe04f2a1a4b";
        public const string CustomRole18Name = "Role18";
        public const string CustomRole19 = "16ba0405-077f-4425-9afa-d2c97a69ac0c";
        public const string CustomRole19Name = "Role19";
        public const string CustomRole20 = "3bb49e8b-9415-48db-909d-05a123fb119d";
        public const string CustomRole20Name = "Role20";
        public const string TransitionalRole21 = "636ac495-b50e-470b-8a04-71328284aed2";
        public const string TransitionalRole21Name = "Role21";
        public const string TransitionalRole22 = "121109a1-5257-492a-a08a-271a1cefe178";
        public const string TransitionalRole22Name = "Role22";
        public const string TransitionalRole23 = "202f3869-de16-4e3b-a971-16c51bcede07";
        public const string TransitionalRole23Name = "Role23";
        public const string TransitionalRole24 = "1f8f8f8f-8f8f-8f8f-8f8f-8f8f8f8f8f8f";
        public const string TransitionalRole24Name = "Role24";
        public const string NonExistentRole = "1f110ce3-35ba-45d3-ba0e-433570880cf2";
        public const string NonExistentRoleName = "NonExistentRole";

        public const string Permission0 = "28480b45-c0a5-424b-99d0-e614aa8f117e";
        public const string Permission0Name = "Permission0";
        public const string Permission1 = "8c86fb94-7e39-4df9-8a61-6f3bbf58e2bf";
        public const string Permission1Name = "Permission1";
        public const string Permission2 = "d2479031-337d-4f37-b540-7199d61677f9";
        public const string Permission2Name = "Permission2";
        public const string Permission3 = "7f639f5f-b6e1-45f7-9534-d6d83562af25";
        public const string Permission3Name = "Permission3";
        public const string Permission4 = "e28be61d-9c61-449a-99c2-ca0da1261af3";
        public const string Permission4Name = "Permission4";
        public const string Permission5 = "65085381-8aea-4ab5-a035-21e1740b3d3f";
        public const string Permission5Name = "Permission5";
        public const string Permission6 = "0c110b5a-a57d-4f16-804f-46dea0091470";
        public const string Permission6Name = "Permission6";
        public const string Permission7 = "0c110b5a-a57d-4f16-804f-46dea0091471";
        public const string Permission7Name = "Permission7";
        public const string Permission8 = "0ddcd3c9-e699-48c5-bda3-7906cd299184";
        public const string Permission8Name = "Permission8";
        public const string Permission9 = "6a2a83f3-6d86-4e91-868c-011e424357e7";
        public const string Permission9Name = "Permission9";
        public const string Permission10 = "40f7f101-0f8e-429a-ad42-ae144bae9ebf";
        public const string Permission10Name = "Permission10";
        public const string Permission11 = "2271319e-b0bd-45bb-8506-ce88e85f31e8";
        public const string Permission11Name = "Permission11";
        public const string Permission12 = "0473543b-111c-4390-86ad-bb29dbb75984";
        public const string Permission12Name = "Permission12";
        public const string Permission13 = "ad32bd0e-eef5-40fd-b8a7-0a258d258f8c";
        public const string Permission13Name = "Permission13";
        public const string Permission14 = "0071956a-e264-4cf2-b9ac-2383bf703c2e";
        public const string Permission14Name = "Permission14";
        public const string Permission15 = "04b55220-ff19-4cbf-9e98-74e32e0333e9";
        public const string Permission15Name = "Permission15";
        public const string Permission16 = "78480b45-d0a5-524b-99d0-f614aa8f117e";
        public const string Permission16Name = "Permission16";

        public static string[] AllPermissions =>
            new[]
            {
                Permission0,
                Permission1,
                Permission2,
                Permission3,
                Permission4,
                Permission5,
                Permission6,
                Permission7,
                Permission8,
                Permission9,
                Permission10,
                Permission11,
                Permission12,
                Permission13,
                Permission14,
                Permission15,
                Permission15
            };


        public static Feature[] GetFeatures()
        {
            return new[]
            {
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature0),
                    Name = Graph.Feature0Name
                },
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature1),
                    Name = Graph.Feature1Name
                },
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature2),
                    Name = Graph.Feature2Name
                },
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature3Name
                },
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature4),
                    Name = Graph.Feature4Name
                },
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature5),
                    Name = Graph.Feature5Name
                },
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature6),
                    Name = Graph.Feature6Name
                },
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature7),
                    Name = Graph.Feature7Name
                },
                new Feature()
                {
                    Id = Guid.Parse(Graph.Feature8),
                    Name = Graph.Feature8Name,
                    IsEnabled = false
                }
            };
        }


        public static Subject[] GetSubjects()
        {
            var subjects = new[]
            {
                new Subject
                {
                    Id = Guid.Parse(Graph.Subject0),
                },
                new Subject
                {
                    Id = Guid.Parse(Graph.Subject1)
                },
                new Subject
                {
                    Id = Guid.Parse(Graph.Subject2)
                },
                new Subject
                {
                    Id = Guid.Parse(Graph.Subject3)
                },
                new Subject
                {
                    Id = Guid.Parse(Graph.Subject4)
                },
                new Subject
                {
                    Id = Guid.Parse(Graph.Subject5)
                },
                new Subject
                {
                    Id = Guid.Parse(Graph.Subject6)
                },
                new Subject
                {
                    Id = Guid.Parse(Graph.Subject7)
                },
                new Subject
                {
                    Id = Guid.Parse(Graph.Subject8)
                },
                new Subject
                {
                    Id = Guid.Parse(Graph.Subject9)
                },
                new Subject
                {
                    Id = Guid.Parse(Graph.Subject10)
                },
                new Subject
                {
                    Id = Guid.Parse(Graph.Trafficker0)
                },
                new Subject
                {
                    Id = Guid.Parse(Graph.Trafficker1)
                },
                new Subject
                {
                    Id = Guid.Parse(Graph.Trafficker2)
                }
            };
            return subjects;
        }

        public static TheoryData<string> AllPermissionsData()
        {
            var data = new TheoryData<string>();
            foreach (var p in AllPermissions)
                data.Add(p);
            return data;
        }

        public class RoleWithType : Role
        {
            public int Type { get; set; } // 0 template, 1 custom, 2 transitional
        }

        public static Role[] GetRoles()
        {
            var roles = new[]
            {
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.CustomRole0),
                    Name = Graph.CustomRole0Name
                },
                new RoleWithType
                {
                    Type = 0,
                    Id = Guid.Parse(Graph.Role1),
                    Name = Graph.Role1Name
                },
                new RoleWithType
                {
                    Type = 0,
                    Id = Guid.Parse(Graph.Role2),
                    Name = Graph.Role2Name
                },
                new RoleWithType
                {
                    Type = 0,
                    Id = Guid.Parse(Graph.Role3),
                    Name = Graph.Role3Name
                },
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.CustomRole4),
                    Name = Graph.CustomRole4Name
                },
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.CustomRole5),
                    Name = Graph.CustomRole5Name
                },
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.CustomRole6),
                    Name = Graph.CustomRole6Name
                },
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.CustomRole7),
                    Name = Graph.CustomRole7Name
                },
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.CustomRole8),
                    Name = Graph.CustomRole8Name
                },
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.CustomRole9),
                    Name = Graph.CustomRole9Name
                },
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.CustomRole10),
                    Name = Graph.CustomRole10Name
                },
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.CustomRole11),
                    Name = Graph.CustomRole11Name
                },
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.CustomRole12),
                    Name = Graph.CustomRole12Name
                },
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.CustomRole13),
                    Name = Graph.CustomRole13Name
                },
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.CustomRole14),
                    Name = Graph.CustomRole14Name
                },
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.CustomRole15),
                    Name = Graph.CustomRole15Name
                },
                new RoleWithType
                {
                    Type = 0,
                    Id = Guid.Parse(Graph.CustomRole16),
                    Name = Graph.CustomRole16Name
                },
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.CustomRole17),
                    Name = Graph.CustomRole17Name
                },
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.CustomRole18),
                    Name = Graph.CustomRole18Name
                },
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.CustomRole19),
                    Name = Graph.CustomRole19Name
                },
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.CustomRole20),
                    Name = Graph.CustomRole20Name
                },
                new RoleWithType
                {
                    Type = 0,
                    Id = Guid.Parse(Graph.TransitionalRole21),
                    Name = Graph.TransitionalRole21Name
                },
                new RoleWithType
                {
                    Type = 0,
                    Id = Guid.Parse(Graph.TransitionalRole22),
                    Name = Graph.TransitionalRole22Name
                },
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.TransitionalRole23),
                    Name = Graph.TransitionalRole23Name
                },
                new RoleWithType
                {
                    Type = 2,
                    Id = Guid.Parse(Graph.TransitionalRole24),
                    Name = Graph.TransitionalRole24Name
                },
                new RoleWithType
                {
                    Type = 1,
                    Id = Guid.Parse(Graph.AdformAdmin),
                    Name = Graph.AdformAdminRoleName
                },
                new RoleWithType
                {
                    Type = 0,
                    Id = Guid.Parse(Graph.LocalAdmin),
                    Name = Graph.LocalAdminRoleName
                }
            };
            return roles;
        }
        
        public static LicensedFeature[] GetLicensedFeatures()
        {
            return new[]
            {
                new LicensedFeature
                {
                    Id = Guid.Parse(Graph.LicensedFeature0),
                    Name = Graph.LicensedFeature0Name
                },
                new LicensedFeature
                {
                    Id = Guid.Parse(Graph.LicensedFeature1),
                    Name = Graph.LicensedFeature1Name
                },
                new LicensedFeature
                {
                    Id = Guid.Parse(Graph.LicensedFeature2),
                    Name = Graph.LicensedFeature2Name
                },
                new LicensedFeature
                {
                    Id = Guid.Parse(Graph.LicensedFeature3),
                    Name = Graph.LicensedFeature3Name,
                    IsEnabled = false
                },
                new LicensedFeature
                {
                    Id = Guid.Parse(Graph.LicensedFeature4),
                    Name = Graph.LicensedFeature4Name,
                    IsEnabled = false
                }
            };
        }

    }
}