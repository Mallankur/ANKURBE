using System.Net;
using Adform.Bloom.Common.Test;
using Xunit;

namespace Adform.Bloom.Acceptance.Test
{
    public static class Scenarios
    {
        public static TheoryData<string, string, HttpStatusCode> Delete_Role_Results_For_Subject1()
        {
            return new TheoryData<string, string, HttpStatusCode>
            {
                {Graph.Subject1, Graph.CustomRole0, HttpStatusCode.Forbidden},
                {Graph.Subject1, Graph.CustomRole10, HttpStatusCode.Forbidden},
                {Graph.Subject1, Graph.Role3, HttpStatusCode.Forbidden},
                {Graph.Subject1, Graph.CustomRole9, HttpStatusCode.Forbidden},
                {Graph.Subject1, Graph.LocalAdmin, HttpStatusCode.Forbidden}
            };
        }

        public static TheoryData<string, string, HttpStatusCode>  Delete_Role_Results_For_Subject3()
        {
            return new TheoryData<string, string, HttpStatusCode>
            {
                {Graph.Subject3, Graph.TransitionalRole22, HttpStatusCode.Forbidden},
                {Graph.Subject3, Graph.CustomRole19, HttpStatusCode.Forbidden},
                {Graph.Subject3, Graph.CustomRole6, HttpStatusCode.NoContent},
                {Graph.Subject3, Graph.LocalAdmin, HttpStatusCode.Forbidden},
                {Graph.Subject3, Graph.CustomRole9, HttpStatusCode.Forbidden}
            };
        }

        public static TheoryData<string, string, HttpStatusCode> Delete_Role_Results_For_AdformAdmin()
        {
            return new TheoryData<string, string, HttpStatusCode>
            {
                {Graph.Subject0, Graph.TransitionalRole22, HttpStatusCode.NoContent},
                {Graph.Subject0, Graph.TransitionalRole21, HttpStatusCode.NoContent},
                {Graph.Subject0, Graph.LocalAdmin, HttpStatusCode.NoContent},
                {Graph.Subject0, Graph.CustomRole9, HttpStatusCode.NoContent},
                {Graph.Subject0, Graph.Role3, HttpStatusCode.NoContent}
            };
        }

        public static TheoryData<string, string, string, bool> Assign_Permission_To_Role_Results_For_Subject3()
        {
            return new TheoryData<string, string, string, bool>
            {
                {Graph.Subject3,Graph.CustomRole0, Graph.Permission7, false},
                {Graph.Subject3,Graph.Role1, Graph.Permission7, false},
                {Graph.Subject3,Graph.Role2, Graph.Permission7, false},
                {Graph.Subject3,Graph.CustomRole6, Graph.Permission2, false},
                {Graph.Subject3,Graph.CustomRole7, Graph.Permission2, false},
                {Graph.Subject3,Graph.CustomRole8, Graph.Permission2, false},
                {Graph.Subject3,Graph.CustomRole9, Graph.Permission2, false},
                {Graph.Subject3,Graph.CustomRole10, Graph.Permission2, false},
                {Graph.Subject3,Graph.CustomRole11, Graph.Permission2, false}
            };
        }

        public static TheoryData<string, string, string, bool> Assign_Permission_To_Role_Results_For_AdformAdmin()
        {
            return new TheoryData<string, string, string, bool>
            {
                {Graph.Subject0,Graph.CustomRole0, Graph.Permission7, true},
                {Graph.Subject0,Graph.Role1, Graph.Permission1, true},
                {Graph.Subject0,Graph.Role2, Graph.Permission0, true},
                {Graph.Subject0,Graph.CustomRole4, Graph.Permission10, true},
                {Graph.Subject0,Graph.CustomRole5, Graph.Permission8, true},
                {Graph.Subject0,Graph.CustomRole6, Graph.Permission8, true},
                {Graph.Subject0,Graph.CustomRole7, Graph.Permission6, true},
                {Graph.Subject0,Graph.CustomRole8, Graph.Permission5, true},
                {Graph.Subject0,Graph.CustomRole9, Graph.Permission4, true}
            };
        }

        public static TheoryData<string, string, string, bool>
            Assign_User_To_Role_Results_For_Subject1() =>
            new TheoryData<string, string, string, bool>()
            {
                {Graph.Tenant1, Graph.Subject1, Graph.Role1, true}
            };
        
        public static TheoryData<bool, string[]> Get_Roles_With_Priority_Test()
        {
            return new TheoryData<bool, string[]>
            {
                {true,new[] {
                    Graph.Role1Name,Graph.Role3Name,Graph.Role2Name, 
                    Graph.LocalAdminRoleName, Graph.CustomRole8Name, 
                    Graph.CustomRole16Name, Graph.CustomRole17Name, Graph.CustomRole0Name, 
                    Graph.CustomRole9Name, Graph.CustomRole5Name
                }},
                {false,new[] {
                    Graph.CustomRole8Name, Graph.CustomRole16Name, 
                    Graph.CustomRole17Name, Graph.Role1Name, 
                    Graph.Role3Name, Graph.CustomRole0Name, Graph.CustomRole9Name, 
                    Graph.Role2Name, Graph.CustomRole5Name, Graph.CustomRole10Name
                }},
            };
        }
        public static TheoryData<string, string, string[]> Get_Roles_With_Sort_Test()
        {
            return new TheoryData<string, string, string[]>
            {
                {"Name","desc",new[] {
                    Graph.CustomRole9Name,Graph.CustomRole8Name,Graph.CustomRole7Name, 
                    Graph.CustomRole6Name, Graph.CustomRole5Name, 
                    Graph.CustomRole4Name, Graph.Role3Name, Graph.CustomRole20Name, 
                    Graph.Role2Name, Graph.CustomRole19Name
                }},
                {"Name","asc",new[] {
                    Graph.LocalAdminRoleName,Graph.CustomRole0Name,Graph.Role1Name, 
                    Graph.CustomRole10Name, Graph.CustomRole11Name, 
                    Graph.CustomRole12Name, Graph.CustomRole13Name, Graph.CustomRole14Name, 
                    Graph.CustomRole15Name, Graph.CustomRole16Name
                }},
                {"BusinessAccountName","asc",new[] {
                    Graph.CustomRole4Name,Graph.LocalAdminRoleName,Graph.CustomRole5Name, 
                    Graph.Role2Name, Graph.CustomRole0Name, 
                    Graph.Role3Name, Graph.Role1Name, Graph.CustomRole7Name, 
                    Graph.CustomRole14Name, Graph.CustomRole12Name
                }},
               
            };
        }


    }
}