using Adform.Ciam.OngDb.Core.Interfaces;
using Adform.Ciam.OngDb.Core.Model;

namespace Adform.Bloom.Runtime.Infrastructure
{
    public static class Constants
    {
        public const string CHILD_OF = "CHILD_OF";
        public const string CONTAINS = "CONTAINS";
        public const string OWNS = "OWNS";
        public const string MEMBER_OF = "MEMBER_OF";
        public const string BELONGS = "BELONGS";
        public const string ASSIGNED = "ASSIGNED";

        public const string Tenant = "Tenant";
        public const string Group = "Group";
        public const string Role = "Role";
        public const string Subject = "Subject";

        public static readonly Link ChildOfDepthLink = new Link(CHILD_OF, null, RelationshipDirection.Outgoing, 5);
        public static readonly Link ChildOfDepthIncomingLink = new Link(CHILD_OF, null, RelationshipDirection.Incoming, 5);
        public static readonly Link ContainsLink = new Link(CONTAINS);
        public static readonly Link AssignedLink = new Link(ASSIGNED);
        public static readonly Link MemberOfLink = new Link(MEMBER_OF);
        public static readonly Link MemberOfIncomingLink = new Link(MEMBER_OF, null, RelationshipDirection.Incoming);
        public static readonly Link BelongsLink = new Link(BELONGS);
        public static readonly Link BelongsIncomingLink = new Link(BELONGS, null, RelationshipDirection.Incoming);

        public const string AdformLabel = "Adform";

        public static class Parameters
        {
            public const string SubjectId = "subjectId";
            public const string TenantIds = "tenantIds";
            public const string TenantLegacyIds = "tenantLegacyIds";
            public const string TenantType = "tenantType";
            public const string PolicyNames = "policyNames";
            public const string InheritanceEnabled = "inheritanceEnabled";
            public const string Query = "query";
            public const string Nodes = "nodes";
            public const string RoleName = "roleName";
            public const string TenantId = "tenantId";
        }
    }
}