using Adform.Ciam.OngDb.Core.Interfaces;
using Adform.Ciam.OngDb.Core.Model;

namespace Adform.Bloom.Infrastructure
{
    public static class Constants
    {
        public static readonly ILink AssignedIncomingLink =
            new Link(Relationship.ASSIGNED, null, RelationshipDirection.Incoming);

        public static readonly ILink AssignedWithVariableIncomingLink =
            new Link(Relationship.ASSIGNED, "assigned", RelationshipDirection.Incoming);

        public static readonly ILink AssignedLink = new Link(Relationship.ASSIGNED);
        public static readonly ILink AssignedWithVariableLink = new Link(Relationship.ASSIGNED, "assigned");
        public static readonly ILink BelongsLink = new Link(Relationship.BELONGS);

        public static readonly ILink BelongsIncomingLink =
            new Link(Relationship.BELONGS, null, RelationshipDirection.Incoming);

        public static readonly ILink MemberOfLink = new Link(Relationship.MEMBER_OF);
        public static readonly ILink MemberOfWithVariableLink = new Link(Relationship.MEMBER_OF, "r");

        public static readonly ILink MemberOfIncomingLink =
            new Link(Relationship.MEMBER_OF, "rel", RelationshipDirection.Incoming);

        public static readonly ILink ChildOfLink = new Link(Relationship.CHILD_OF);

        public static readonly ILink ChildOfDepthLink =
            new Link(Relationship.CHILD_OF, null, RelationshipDirection.Outgoing, 5);

        public static readonly ILink ChildOfIncomingDepthLink =
            new Link(Relationship.CHILD_OF, null, RelationshipDirection.Incoming, 5);

        public static readonly ILink ContainsLink = new Link(Relationship.CONTAINS);

        public static readonly ILink ContainsIncomingLink =
            new Link(Relationship.CONTAINS, null, RelationshipDirection.Incoming);

        public static readonly ILink ContainsVariableLink = new Link(Relationship.CONTAINS, "r");

        public static readonly ILink ContainsVariableIncomingLink =
            new Link(Relationship.CONTAINS, "r", RelationshipDirection.Incoming);

        public static readonly ILink OwnsLink = new Link(Relationship.OWNS);

        public static readonly ILink OwnsIncomingLink =
            new Link(Relationship.OWNS, null, RelationshipDirection.Incoming);

        public static readonly ILink DependsOnLink = new Link(Relationship.DEPENDS_ON);
        public static readonly ILink DependsOnRecursiveLink = new Link(Relationship.DEPENDS_ON, "dependsOn", depth: 5);

        public static class Parameters
        {
            public const string Id = "id";
            public const string Pagination = "pagination";
            public const string PageSize = "pageSize";
            public const string CurrentPage = "currentPage";
            public const string Subject = "subject";
            public const string SubjectId = "subjectId";
            public const string Assignment = "assignment";
            public const string Policy = "policy";
            public const string PolicyId = "policyId";
            public const string BusinessAccount = "businessAccount";
            public const string BusinessAccountType = "businessAccountType";
            public const string BusinessAccountId = "businessAccountId";
            public const string Role = "role";
            public const string RoleId = "roleId";
            public const string Permission = "permission";
            public const string PermissionNames = "permissionNames";
            public const string PermissionBusinessAccountEvaluationParameter = "permissionBusinessAccountEvaluationParameter";
            public const string ParentId = "parentId";
            public const string Feature = "feature";
            public const string ShowTemplateRoles = "showTemplateRoles";
            public const string PrioritizeTemplateRoles = "prioritizeTemplateRoles";
            public const string IsTemplateRole = "templateRole";
            public const string FeatureIds = "featureIds";
            public const string BusinessAccountIds = "businessAccountIds";
            public const string ProductNames = "productNames";
            public const string UserIds = "userIds";
            public const string Search = "search";
            public const string RoleTenantIds = "roleTenantIds";
            public const string AssignFeatureIds = "assignFeatureIds";
            public const string UnassignFeatureIds = "unassignFeatureIds";
            public const string AssignLicensedFeatureIds = "assignLicensedFeatureIds";
            public const string UnassignLicensedFeatureIds = "unassignLicensedFeatureIds";
            public const string FeatureId = "featureId";
            public const string DependentOnId = "dependentOnId";
            public const string Operation = "operation";
            public const string AssignRoleBusinessAccountIds = "assignRoleBusinessAccountIds";
            public const string UnassignRoleBusinessAccountIds = "unassignRoleBusinessAccountIds";
            public const string AssetsReassignments = "assetsReassignments";
            public const string UserId = "userId";
            public const string SortBy = "sortBy";
            public const string QueryParams = "queryParams";
        }

        public static class Relationship
        {
            public const string CHILD_OF = "CHILD_OF";
            public const string OWNS = "OWNS";
            public const string MEMBER_OF = "MEMBER_OF";
            public const string BELONGS = "BELONGS";
            public const string ASSIGNED = "ASSIGNED";
            public const string CONTAINS = "CONTAINS";
            public const string DEPENDS_ON = "DEPENDS_ON";
        }

        public static class Label
        {
            public const string CUSTOM_ROLE = "CustomRole";
            public const string TEMPLATE_ROLE = "TemplateRole";
            public const string TRANSITIONAL_ROLE = "TransitionalRole";
            public const string SYSTEM = "System";
            public const string ADFORM = "Adform";
            public const string IDENTITY = "Identity";
            public const string AGENCY = "Agency";
            public const string PUBLISHER = "Publisher";
            public const string DATA_PROVIDER = "DataProvider";
            public const string TRAFFICKER_ROLE = "TraffickerRole";
            public const string TRAFFICKER = "Trafficker";
        }

        public static class TenantNames
        {
            public const string Adform = "Adform";
        }

        public static class ParamNames
        {
            public const string RoleIdTenantId = "role_tenant_ids";
            public const string RoleId = "role_id";
            public const string TenantId = "tenant_id";
            public const string Email = "email";
        }
    }
}