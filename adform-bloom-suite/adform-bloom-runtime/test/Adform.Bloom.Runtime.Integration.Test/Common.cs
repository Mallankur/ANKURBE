using System;
using System.Collections.Generic;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Runtime.Integration.Test.Seed;
using Adform.Bloom.Runtime.Read.Entities;
using FluentResults;
using MediatR;
using Xunit;

namespace Adform.Bloom.Runtime.Integration.Test
{
    public static class Common
    {
        #region Graph 2

        public static TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> QueryInput2()
        {
            var data = new TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>>();

            EvaluateAAP_ForUnknownPolicy_NoResultsShouldBeReturned(data);

            EvaluateAAP_ForUnknownTenant_NoResultsShouldBeReturned(data);

            EvaluateAAP_InContextOfRootTenantForWhichItHasNoAssignment_NoResultShouldBeReturned(data);

            EvaluateAap_AcrossAllTenants_CorrectResultsShouldBe(data);

            EvaluateAap_ForSpecificTenants_CorrectResultsShouldBe(data);


            EvaluateMK_ForUnknownPolicy_NoResultsShouldBeReturned(data);

            EvaluateMK_ForUnknownTenant_NoResultsShouldBeReturned(data);

            EvaluateMK_VariousScenarios_CorrectResultsShouldBeReturned(data);

            EvaluateAC_InContextOfTenantsForWhichItHasNoAssignment_InheritanceEnabled_AssignmentsShouldBeInheritedFromRootTenant(
                data);

            EvaluateMK_EvaluateTenantsForWhichMkHasNoAssignmentNoInheritanceEnabledNoResultShouldBeReturned(data);


            EvaluateTest_VariousScenarios_CorrectResultsShouldBeReturned(data);

            EvaluateTest2_VariousScenarios_CorrectResultsShouldBeReturned(data);

            Children_Tenants_Inherit_Assignments_But_Parent_Ones_Do_Not_See_Assignments_From_Children(data);

            Children_Polices_Inherit_Assignments_But_Parent_Ones_Do_Not_See_Assignments_From_Children(data);

            return data;
        }


        #region Evaluation

        private static void EvaluateAAP_ForUnknownPolicy_NoResultsShouldBeReturned(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.aapSubject),
                    InheritanceEnabled = true,
                    PolicyNames = new List<string> {string.Empty}
                },
                new List<RuntimeResult>()
            );

            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.aapSubject),
                    InheritanceEnabled = true,
                    TenantIds = new List<Guid> {Guid.NewGuid()},
                    PolicyNames = new List<string> {Graph2.RootPolicyName}
                },
                new List<RuntimeResult>()
            );
        }

        private static void EvaluateAAP_ForUnknownTenant_NoResultsShouldBeReturned(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.aapSubject),
                    InheritanceEnabled = true,
                    TenantIds = new List<Guid> {Guid.NewGuid()}
                },
                new List<RuntimeResult>()
            );

            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.aapSubject),
                    InheritanceEnabled = true,
                    TenantIds = new List<Guid>
                    {
                        Guid.Parse(Graph2.Microsoft), Guid.Parse(Graph2.Nike), Guid.Parse(Graph2.Adform),
                        Guid.Parse(Graph2.Apple)
                    },
                    PolicyNames = new List<string> {string.Empty}
                },
                new List<RuntimeResult>()
            );
        }

        private static void EvaluateAAP_InContextOfRootTenantForWhichItHasNoAssignment_NoResultShouldBeReturned(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.aapSubject),
                    InheritanceEnabled = true,
                    TenantIds = new List<Guid> {Guid.Parse(Graph2.Adform)}
                },
                new List<RuntimeResult>()
            );
        }

        private static void EvaluateAap_AcrossAllTenants_CorrectResultsShouldBe(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.aapSubject),
                    InheritanceEnabled = true,
                    PolicyNames = new List<string> {Graph2.RootPolicyName}
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Microsoft),
                        TenantName = Graph2.MicrosoftName,
                        TenantType = Graph2.DataProviderLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph2.LocalAdminMicrosoftName},
                        Permissions = new List<string>()
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Nike),
                        TenantName = Graph2.NikeName,
                        TenantType = Graph2.PublisherLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph2.LocalAdminNikeName},
                        Permissions = new List<string>()
                    }
                }
            );

            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.aapSubject),
                    InheritanceEnabled = true,
                    TenantLegacyIds = new[] {4},
                    TenantType = Graph2.DataProviderLabel,
                    PolicyNames = new List<string> {Graph2.RootPolicyName}
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Microsoft),
                        TenantName = Graph2.MicrosoftName,
                        TenantType = Graph2.DataProviderLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph2.LocalAdminMicrosoftName},
                        Permissions = new List<string>()
                    }
                }
            );

            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.aapSubject),
                    InheritanceEnabled = false,
                    PolicyNames = new List<string> {Graph2.RootPolicyName}
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Microsoft),
                        TenantName = Graph2.MicrosoftName,
                        TenantType = Graph2.DataProviderLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph2.LocalAdminMicrosoftName},
                        Permissions = new List<string>()
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Nike),
                        TenantName = Graph2.NikeName,
                        TenantType = Graph2.PublisherLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph2.LocalAdminNikeName},
                        Permissions = new List<string>()
                    }
                }
            );
        }

        private static void EvaluateAap_ForSpecificTenants_CorrectResultsShouldBe(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.aapSubject),
                    InheritanceEnabled = true,
                    TenantIds = new List<Guid> {Guid.Parse(Graph2.Microsoft), Guid.Parse(Graph2.Nike)}
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Microsoft),
                        TenantName = Graph2.MicrosoftName,
                        TenantType = Graph2.DataProviderLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph2.LocalAdminMicrosoftName},
                        Permissions = new List<string>()
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Nike),
                        TenantName = Graph2.NikeName,
                        TenantType = Graph2.PublisherLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph2.LocalAdminNikeName},
                        Permissions = new List<string>()
                    }
                }
            );

            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.aapSubject),
                    InheritanceEnabled = true,
                    TenantIds = new List<Guid> {Guid.Parse(Graph2.Microsoft), Guid.Parse(Graph2.Nike)},
                    PolicyNames = new List<string> {Graph2.RootPolicyName}
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Microsoft),
                        TenantName = Graph2.MicrosoftName,
                        TenantType = Graph2.DataProviderLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph2.LocalAdminMicrosoftName},
                        Permissions = new List<string>()
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Nike),
                        TenantName = Graph2.NikeName,
                        TenantType = Graph2.PublisherLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph2.LocalAdminNikeName},
                        Permissions = new List<string>()
                    }
                }
            );

            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.aapSubject),
                    InheritanceEnabled = true,
                    TenantIds = new List<Guid> {Guid.Parse(Graph2.Microsoft), Guid.Parse(Graph2.Nike)},
                    PolicyNames = new List<string> {Graph2.RootPolicyName}
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Microsoft),
                        TenantName = Graph2.MicrosoftName,
                        TenantType = Graph2.DataProviderLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph2.LocalAdminMicrosoftName},
                        Permissions = new List<string>()
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Nike),
                        TenantName = Graph2.NikeName,
                        TenantType = Graph2.PublisherLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph2.LocalAdminNikeName},
                        Permissions = new List<string>()
                    }
                }
            );

            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.aapSubject),
                    InheritanceEnabled = true,
                    TenantIds = new List<Guid>
                    {
                        Guid.Parse(Graph2.Microsoft), Guid.Parse(Graph2.Nike), Guid.Parse(Graph2.Adform),
                        Guid.Parse(Graph2.Apple)
                    }
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Microsoft),
                        TenantName = Graph2.MicrosoftName,
                        TenantType = Graph2.DataProviderLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph2.LocalAdminMicrosoftName},
                        Permissions = new List<string>()
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Nike),
                        TenantName = Graph2.NikeName,
                        TenantType = Graph2.PublisherLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph2.LocalAdminNikeName},
                        Permissions = new List<string>()
                    }
                }
            );
        }

        private static void EvaluateMK_ForUnknownPolicy_NoResultsShouldBeReturned(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.mkSubject),
                    InheritanceEnabled = true,
                    PolicyNames = new List<string> {string.Empty}
                },
                new List<RuntimeResult>()
            );
        }

        private static void EvaluateMK_ForUnknownTenant_NoResultsShouldBeReturned(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.mkSubject),
                    InheritanceEnabled = true,
                    TenantIds = new List<Guid> {Guid.NewGuid()}
                },
                new List<RuntimeResult>()
            );
        }

        private static void EvaluateMK_VariousScenarios_CorrectResultsShouldBeReturned(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.mkSubject),
                    InheritanceEnabled = true,
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Adform),
                        TenantName = Graph2.AdformName,
                        TenantType = Graph2.RootLabel,
                        TenantLegacyId = 0,
                        Roles = new List<string> {Graph2.AdformAdminName},
                        Permissions = new List<string>()
                    }
                }
            );

            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.mkSubject),
                    InheritanceEnabled = false,
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Adform),
                        TenantName = Graph2.AdformName,
                        TenantType = Graph2.RootLabel,
                        TenantLegacyId = 0,
                        Roles = new List<string> {Graph2.AdformAdminName},
                        Permissions = new List<string>()
                    }
                }
            );
        }

        private static void
            EvaluateAC_InContextOfTenantsForWhichItHasNoAssignment_InheritanceEnabled_AssignmentsShouldBeInheritedFromRootTenant(
                TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.crespoSubject),
                    InheritanceEnabled = true,
                    TenantIds = new List<Guid> {Guid.Parse(Graph2.Microsoft)}
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Microsoft),
                        TenantName = Graph2.MicrosoftName,
                        TenantType = Graph2.DataProviderLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph2.LocalAdminMicrosoftName},
                        Permissions = new List<string>()
                    },
                }
            );

            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.crespoSubject),
                    InheritanceEnabled = true,
                    TenantIds = new List<Guid> {Guid.Parse(Graph2.IKEA)}
                },
                new List<RuntimeResult>
                {
                }
            );

            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.crespoSubject),
                    InheritanceEnabled = true,
                    TenantIds = new List<Guid> {Guid.Parse(Graph2.IKEA), Guid.Parse(Graph2.Microsoft)},
                    PolicyNames = new List<string> {Graph2.RootPolicyName}
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Microsoft),
                        TenantName = Graph2.MicrosoftName,
                        TenantType = Graph2.DataProviderLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph2.LocalAdminMicrosoftName},
                        Permissions = new List<string>()
                    }
                }
            );
        }

        private static void
            EvaluateMK_EvaluateTenantsForWhichMkHasNoAssignmentNoInheritanceEnabledNoResultShouldBeReturned(
                TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.mkSubject),
                    InheritanceEnabled = false,
                    TenantIds = new List<Guid> {Guid.Parse(Graph2.IKEA)}
                },
                new List<RuntimeResult>()
            );

            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.mkSubject),
                    InheritanceEnabled = false,
                    TenantIds = new List<Guid> {Guid.Parse(Graph2.IKEA), Guid.Parse(Graph2.Microsoft)}
                },
                new List<RuntimeResult>()
            );
        }

        private static void EvaluateTest_VariousScenarios_CorrectResultsShouldBeReturned(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.testSubject),
                    InheritanceEnabled = true,
                    PolicyNames = new List<string> {Graph2.RootPolicyName}
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Adform),
                        TenantName = Graph2.AdformName,
                        TenantType = Graph2.RootLabel,
                        TenantLegacyId = 0,
                        Roles = new List<string> {Graph2.LocalAdminAdformName},
                        Permissions = new List<string>()
                    }
                }
            );

            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.testSubject),
                    InheritanceEnabled = true,
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Adform),
                        TenantName = Graph2.AdformName,
                        TenantType = Graph2.RootLabel,
                        TenantLegacyId = 0,
                        Roles = new List<string> {Graph2.LocalAdminAdformName},
                        Permissions = new List<string>()
                    }
                }
            );

            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.testSubject),
                    InheritanceEnabled = true,
                    TenantType = Graph2.RootLabel
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Adform),
                        TenantName = Graph2.AdformName,
                        TenantType = Graph2.RootLabel,
                        TenantLegacyId = 0,
                        Roles = new List<string> {Graph2.LocalAdminAdformName},
                        Permissions = new List<string>()
                    }
                }
            );
        }

        private static void EvaluateTest2_VariousScenarios_CorrectResultsShouldBeReturned(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.test2subject),
                    InheritanceEnabled = true,
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.IKEA),
                        TenantName = Graph2.IKEAName,
                        TenantType = Graph2.AgencyLabel,
                        TenantLegacyId = 1,
                        Roles = new List<string> {Graph2.LocalAdminIkeaName},
                        Permissions = new List<string>()
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Nike),
                        TenantName = Graph2.NikeName,
                        TenantType = Graph2.PublisherLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph2.LocalAdminNikeName},
                        Permissions = new List<string>()
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Microsoft),
                        TenantName = Graph2.MicrosoftName,
                        TenantType = Graph2.DataProviderLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph2.LocalAdminTestName},
                    }
                }
            );


            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.test2subject),
                    InheritanceEnabled = true,
                    PolicyNames = new List<string> {Graph2.RootPolicyName}
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.IKEA),
                        TenantName = Graph2.IKEAName,
                        TenantType = Graph2.AgencyLabel,
                        TenantLegacyId = 1,
                        Roles = new List<string> {Graph2.LocalAdminIkeaName},
                        Permissions = new List<string>()
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Nike),
                        TenantName = Graph2.NikeName,
                        TenantType = Graph2.PublisherLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph2.LocalAdminNikeName},
                        Permissions = new List<string>()
                    }
                }
            );
        }

        private static void Children_Tenants_Inherit_Assignments_But_Parent_Ones_Do_Not_See_Assignments_From_Children(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.testSubject),
                    InheritanceEnabled = true,
                    TenantIds = new List<Guid> {Guid.Parse(Graph2.Microsoft)},
                    PolicyNames = new List<string> {Graph2.ChildPolicyName}
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Microsoft),
                        TenantName = Graph2.MicrosoftName,
                        TenantType = Graph2.DataProviderLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph2.LocalAdminTestName},
                        Permissions = new List<string>()
                    },
                }
            );

            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.testSubject),
                    InheritanceEnabled = false,
                    TenantIds = new List<Guid> {Guid.Parse(Graph2.Microsoft)},
                    PolicyNames = new List<string> {Graph2.RootPolicyName}
                },
                new List<RuntimeResult>()
            );


            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.test2subject),
                    InheritanceEnabled = true,
                    TenantIds = new List<Guid> {Guid.Parse(Graph2.Adform)}
                },
                new List<RuntimeResult>()
            );
        }

        private static void Children_Polices_Inherit_Assignments_But_Parent_Ones_Do_Not_See_Assignments_From_Children(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.testSubject),
                    InheritanceEnabled = true,
                    PolicyNames = new List<string> {Graph2.ChildPolicyName}
                },
                new List<RuntimeResult>()
            );

            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.test2subject),
                    InheritanceEnabled = true,
                    PolicyNames = new List<string> {Graph2.ChildPolicyName}
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.IKEA),
                        TenantName = Graph2.IKEAName,
                        TenantType = Graph2.AgencyLabel,
                        TenantLegacyId = 1,
                        Roles = new List<string> {Graph2.LocalAdminIkeaName},
                        Permissions = new List<string>()
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Nike),
                        TenantName = Graph2.NikeName,
                        TenantType = Graph2.PublisherLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph2.LocalAdminNikeName},
                        Permissions = new List<string>()
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Microsoft),
                        TenantName = Graph2.MicrosoftName,
                        TenantType = Graph2.DataProviderLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph2.LocalAdminTestName},
                        Permissions = new List<string>()
                    }
                }
            );

            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph2.test2subject),
                    InheritanceEnabled = false,
                    PolicyNames = new List<string> {Graph2.ChildPolicyName}
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph2.Microsoft),
                        TenantName = Graph2.MicrosoftName,
                        TenantType = Graph2.DataProviderLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph2.LocalAdminTestName},
                        Permissions = new List<string>()
                    }
                }
            );
        }

        #endregion

        #endregion

        #region Graph 1

        public static TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> QueryInput()
        {
            var data = new TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>>();

            UnknownSubject(data);
            Subject1_UnknownTenant(data);
            Subject1_UnknownPolicy(data);
            Subject1_UnknownPolicyAndTenant(data);
            
            Subject1_AllTenantsAndPolicies_InheritanceEnabled(data);
            Subject1_AllTenantsAndPolicies_NoInheritanceEnabled(data);
            
            Subject2_AllTenantsAndPolicies_InheritanceEnabled(data);
            Subject2_AllTenantsAndPolicies_NoInheritanceEnabled(data);
            Subject2_TenantLegacyId_InheritanceEnabled(data);
            Subject2_SpecificTenantForWhichSubjectHasNoAssignment_InheritanceEnabled(data);
            Subject2_SpecificTenantForWhichSubjectHasNoAssignment_NoInheritanceEnabled_NoResultShouldBeReturned(data);
            
            Subject3_AllTenantsAndPolicies_InheritanceEnabled(data);
            
            WhenEvaluatingAdformUsersInheritanceShouldBeDisabledAndLegacyIdsOverriden(data);

            return data;
        }

        public static TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> IntersectionQueryInput()
        {
            var data = new TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>>();

            UnknownSubject(data);
            Actor4Subject1_UnknownTenant(data);
            Actor4Subject1_UnknownPolicy(data);
            Actor4Subject1_UnknownPolicyAndTenant(data);

            Actor4Subject1_AllTenantsAndPolicies_InheritanceEnabled(data);
            Actor4Subject1_AllTenantsAndPolicies_NoInheritanceEnabled(data);
            Actor4Subject1_SpecificLegacyTenant_NoInheritanceEnabled(data);
            Actor4Subject1_SpecificLegacyTenant_InheritanceEnabled(data);

            Subject2_AllTenantsAndPolicies_InheritanceEnabled(data);
            Subject2_AllTenantsAndPolicies_InheritanceDisabled(data);
            Subject2_TenantLegacyId_InheritanceEnabled(data);
            Subject2_SpecificTenantForWhichSubjectHasNoDirectAssignment_InheritanceEnabled(data);
            Subject2_SpecificTenantForWhichSubjectHasNoDirectAssignment_NoInheritanceEnabled_NoResultShouldBeReturned(
                data);

            Actor4Subject5_AllTenantsAndPolicies_InheritanceEnabled_NoResultShouldBeReturned(data);
            Actor5Subject4_AllTenantsAndPolicies_InheritanceEnabled(data);
            Actor5Subject4_AllTenantsAndPolicies_InheritanceDisabled_NoResultShouldBeReturned(data);

            Actor6Subject3_SpecificTenantForWhichActorHasNoAssignment_InheritanceEnabled_NoResultShouldBeReturned(data);

            Actor4Subject2_AllTenantsAndPolicies_InheritanceEnabled(data);
            Actor4Subject2_AllTenantsAndPolicies_InheritanceDisabled_NoResultShouldBeReturned(data);
            Actor4Subject2_TenantLegacyId_InheritanceEnabled(data);
            Actor4Subject2_SpecificTenantForWhichSubjectHasNoDirectAssignment_InheritanceEnabled(data);
            Actor4Subject2_SpecificTenantForWhichSubjectHasNoDirectAssignment_InheritanceDisabled_NoResultShouldBeReturned(
                data);
            Actor4Subject2_SpecificTenantForWhichActorHasNoDirectAssignment_InheritanceEnabled_NoResultShouldBeReturned(
                data);
            Actor4Subject2_SpecificTenantForWhichActorHasNoDirectAssignment_InheritanceDisabled_NoResultShouldBeReturned(
                data);

            WhenEvaluatingAdformUsersInheritanceShouldBeDisabledAndLegacyIdsOverriden(data);

            return data;
        }

        public static TheoryData<IRequest<Result<bool>>, bool, string> RoleExistenceQueryInput()
        {
            var data = new TheoryData<IRequest<Result<bool>>, bool, string>
            {
                {
                    new RoleExistenceQuery {RoleName = "nonexistent", TenantId = Guid.Parse(Graph.Tenant0)}, false,
                    "role-exists"
                },
                {
                    new RoleExistenceQuery {RoleName = Graph.Role1Name, TenantId = Guid.NewGuid()}, false, "role-exists"
                },
                {
                    new RoleExistenceQuery {RoleName = Graph.Role1Name, TenantId = Guid.Parse(Graph.Tenant0)}, false,
                    "role-exists"
                },
                {
                    new RoleExistenceQuery {RoleName = Graph.CustomRole9Name, TenantId = Guid.Parse(Graph.Tenant0)},
                    true, "role-exists"
                }
            };

            return data;
        }

        public static TheoryData<IRequest<Result<bool>>, string> RoleExistenceQueryValidationErrorInput()
        {
            var data = new TheoryData<IRequest<Result<bool>>, string>
            {
                {
                    new RoleExistenceQuery {RoleName = "", TenantId = Guid.Empty}, "role-exists"
                },
                {
                    new RoleExistenceQuery {RoleName = "string", TenantId = Guid.Empty}, "role-exists"
                },
                {
                    new RoleExistenceQuery {RoleName = "", TenantId = Guid.NewGuid()}, "role-exists"
                }
            };

            return data;
        }

        public static TheoryData<IRequest<Result<bool>>, bool, string> LegacyTenantExistenceQueryInput()
        {
            var data = new TheoryData<IRequest<Result<bool>>, bool, string>
            {
                {
                    new LegacyTenantExistenceQuery {TenantLegacyIds = new List<int> {0}, TenantType = "nonexistent"},
                    false, "legacy-tenants-exist"
                },
                {
                    new LegacyTenantExistenceQuery {TenantLegacyIds = new List<int> {1}, TenantType = "Agency"}, true,
                    "legacy-tenants-exist"
                },
                {
                    new LegacyTenantExistenceQuery {TenantLegacyIds = new List<int> {4}, TenantType = "DataProvider"},
                    true, "legacy-tenants-exist"
                },
                {
                    new LegacyTenantExistenceQuery {TenantLegacyIds = new List<int> {1, 3}, TenantType = "Agency"},
                    true, "legacy-tenants-exist"
                },
                {
                    new LegacyTenantExistenceQuery {TenantLegacyIds = new List<int> {0, 1}, TenantType = "Agency"},
                    false, "legacy-tenants-exist"
                },
            };

            return data;
        }

        public static TheoryData<IRequest<Result<bool>>, string> LegacyTenantExistenceQueryValidationErrorInput()
        {
            var data = new TheoryData<IRequest<Result<bool>>, string>
            {
                {
                    new LegacyTenantExistenceQuery {TenantLegacyIds = new List<int> { }, TenantType = "nonexistent"},
                    "legacy-tenants-exist"
                },
                {
                    new LegacyTenantExistenceQuery {TenantLegacyIds = new List<int> {1}, TenantType = ""},
                    "legacy-tenants-exist"
                },
            };

            return data;
        }

        public static TheoryData<IRequest<Result<bool>>, bool, string> NodeExistenceQueryInput()
        {
            var data = new TheoryData<IRequest<Result<bool>>, bool, string>
            {
                {
                    new NodeExistenceQuery
                    {
                        NodeDescriptors = new List<NodeDescriptor>
                        {
                            new NodeDescriptor
                            {
                                Label = Graph.TenantLabel,
                                Id = Guid.Parse(Graph.Tenant0)
                            },
                            new NodeDescriptor
                            {
                                Label = Graph.TenantLabel,
                                Id = Guid.NewGuid()
                            }
                        }
                    },
                    false, "nodes-exist"
                },
                {
                    new NodeExistenceQuery
                    {
                        NodeDescriptors = new List<NodeDescriptor>
                        {
                            new NodeDescriptor
                            {
                                Label = "nonexistent",
                                Id = Guid.Parse(Graph.Tenant0)
                            },
                        }
                    },
                    false, "nodes-exist"
                },
                {
                    new NodeExistenceQuery
                    {
                        NodeDescriptors = new List<NodeDescriptor>
                        {
                            new NodeDescriptor
                            {
                                Label = Graph.TenantLabel,
                                Id = Guid.Parse(Graph.Tenant0)
                            },
                            new NodeDescriptor
                            {
                                Label = Graph.TenantLabel,
                                Id = Guid.Parse(Graph.Tenant1),
                                UniqueName = Graph.Tenant1Name
                            },
                            new NodeDescriptor
                            {
                                Label = Graph.PermissionLabel,
                                UniqueName = Graph.Permission0Name
                            }
                        }
                    },
                    true, "nodes-exist"
                },
            };

            return data;
        }

        public static TheoryData<IRequest<Result<bool>>, string> NodeExistenceQueryValidationErrorInput()
        {
            var data = new TheoryData<IRequest<Result<bool>>, string>
            {
                {
                    new NodeExistenceQuery {NodeDescriptors = new List<NodeDescriptor> { }}, "nodes-exist"
                },
                {
                    new NodeExistenceQuery
                    {
                        NodeDescriptors = new List<NodeDescriptor>
                        {
                            new NodeDescriptor
                            {
                                Id = Guid.Empty,
                                UniqueName = ""
                            },
                            new NodeDescriptor
                            {
                                Id = Guid.Empty,
                                UniqueName = "name"
                            },
                            new NodeDescriptor
                            {
                                Id = Guid.NewGuid(),
                                UniqueName = ""
                            },
                        }
                    },
                    "nodes-exist"
                },
            };

            return data;
        }

        #region Evaluation

        private static void WhenEvaluatingAdformUsersInheritanceShouldBeDisabledAndLegacyIdsOverriden(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph.SubjectTest),
                    InheritanceEnabled = true,
                    TenantLegacyIds = new List<int> {0, 4},
                    TenantType = "Agency"
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.TenantAdform),
                        TenantName = Graph.TenantAdformName,
                        TenantType = Graph.AdformLabel,
                        TenantLegacyId = 0,
                        Roles = new List<string> {Graph.CustomAdformLocalAdminRoleName},
                        Permissions = new List<string>()
                    }
                });

            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph.SubjectMasterCrespo),
                    InheritanceEnabled = true,
                    TenantLegacyIds = new List<int> {0, 4},
                    TenantType = "Agency"
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.TenantAdform),
                        TenantName = Graph.TenantAdformName,
                        TenantType = Graph.AdformLabel,
                        TenantLegacyId = 0,
                        Roles = new List<string> {Graph.CustomAdformAdminRoleName},
                        Permissions = new List<string>()
                    }
                });
        }

        private static void UnknownSubject(TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.NewGuid(),
                    InheritanceEnabled = true,
                },
                new List<RuntimeResult>()
            );
            
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.NewGuid(),
                    InheritanceEnabled = true,
                },
                new List<RuntimeResult>()
            );
            
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.NewGuid(),
                    InheritanceEnabled = false,
                },
                new List<RuntimeResult>()
            );
            
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.NewGuid(),
                    InheritanceEnabled = false,
                },
                new List<RuntimeResult>()
            );
        }

        private static void Subject1_UnknownTenant(TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph.Subject1),
                    InheritanceEnabled = true,
                    TenantIds = new[] {Guid.Empty}
                },
                new List<RuntimeResult>()
            );
        }

        private static void Subject1_UnknownPolicy(TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph.Subject1),
                    InheritanceEnabled = true,
                    PolicyNames = new[] {Graph.PolicyUnknown}
                },
                new List<RuntimeResult>()
            );
        }

        private static void Subject1_UnknownPolicyAndTenant(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph.Subject1),
                    InheritanceEnabled = true,
                    PolicyNames = new[] {string.Empty},
                    TenantIds = new[] {Guid.Empty}
                },
                new List<RuntimeResult>()
            );
        }


        private static void Subject1_AllTenantsAndPolicies_InheritanceEnabled(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph.Subject1),
                    InheritanceEnabled = true,
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 1,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    },

                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant2),
                        TenantName = Graph.Tenant2Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant3),
                        TenantName = Graph.Tenant3Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 3,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant4),
                        TenantName = Graph.Tenant4Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant5),
                        TenantName = Graph.Tenant5Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 5,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant6),
                        TenantName = Graph.Tenant6Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 6,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant7),
                        TenantName = Graph.Tenant7Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 7,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    }
                }
            );
        }

        private static void Subject1_AllTenantsAndPolicies_NoInheritanceEnabled(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph.Subject1),
                    InheritanceEnabled = false,
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 1,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    }
                }
            );
        }

        private static void Subject2_AllTenantsAndPolicies_InheritanceEnabled(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph.Subject2),
                    InheritanceEnabled = true,
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant2),
                        TenantName = Graph.Tenant2Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph.Role2Name},
                        Permissions = new List<string> {Graph.Permission3Name, Graph.Permission4Name}
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant4),
                        TenantName = Graph.Tenant4Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph.Role2Name},
                        Permissions = new List<string> {Graph.Permission3Name, Graph.Permission4Name}
                    },

                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant5),
                        TenantName = Graph.Tenant5Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 5,
                        Roles = new List<string> {Graph.Role2Name},
                        Permissions = new List<string> {Graph.Permission3Name, Graph.Permission4Name}
                    }
                }
            );
        }

        private static void Subject2_TenantLegacyId_InheritanceEnabled(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph.Subject2),
                    InheritanceEnabled = true,
                    TenantLegacyIds = new[] {2, 4},
                    TenantType = Graph.TenantLabel
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant2),
                        TenantName = Graph.Tenant2Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph.Role2Name},
                        Permissions = new List<string> {Graph.Permission3Name, Graph.Permission4Name}
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant4),
                        TenantName = Graph.Tenant4Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph.Role2Name},
                        Permissions = new List<string> {Graph.Permission3Name, Graph.Permission4Name}
                    }
                }
            );
        }

        private static void Subject2_AllTenantsAndPolicies_NoInheritanceEnabled(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph.Subject2),
                    InheritanceEnabled = false,
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant2),
                        TenantName = Graph.Tenant2Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph.Role2Name},
                        Permissions = new List<string> {Graph.Permission3Name, Graph.Permission4Name}
                    }
                }
            );
        }

        private static void Subject2_SpecificTenantForWhichSubjectHasNoAssignment_InheritanceEnabled(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph.Subject2),
                    InheritanceEnabled = true,
                    TenantIds = new List<Guid> {Guid.Parse(Graph.Tenant5)}
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant5),
                        TenantName = Graph.Tenant5Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 5,
                        Roles = new List<string> {Graph.Role2Name},
                        Permissions = new List<string> {Graph.Permission3Name, Graph.Permission4Name}
                    }
                }
            );
        }

        private static void
            Subject2_SpecificTenantForWhichSubjectHasNoAssignment_NoInheritanceEnabled_NoResultShouldBeReturned(
                TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph.Subject2),
                    InheritanceEnabled = false,
                    TenantIds = new List<Guid> {Guid.Parse(Graph.Tenant5)}
                },
                new List<RuntimeResult>()
            );
        }


        private static void Subject3_AllTenantsAndPolicies_InheritanceEnabled(
            TheoryData<SubjectRuntimeQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectRuntimeQuery
                {
                    SubjectId = Guid.Parse(Graph.Subject3),
                    InheritanceEnabled = true,
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant5),
                        TenantName = Graph.Tenant5Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 5,
                        Roles = new List<string> {Graph.Role3Name},
                        Permissions = new List<string>
                        {
                            Graph.Permission5Name, Graph.Permission6Name, Graph.Permission8Name, Graph.Permission13Name
                        }
                    }
                }
            );
        }

        #endregion

        #region Intersection

        private static void WhenEvaluatingAdformUsersInheritanceShouldBeDisabledAndLegacyIdsOverriden(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.SubjectTest),
                    SubjectId = Guid.Parse(Graph.Subject1),
                    InheritanceEnabled = true,
                    TenantLegacyIds = new List<int> {0, 4},
                    TenantType = "Agency"
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.TenantAdform),
                        TenantName = Graph.TenantAdformName,
                        TenantType = Graph.AdformLabel,
                        TenantLegacyId = 0,
                        Roles = new List<string> {Graph.CustomAdformLocalAdminRoleName},
                        Permissions = new List<string>()
                    }
                });

            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.SubjectTest),
                    SubjectId = Guid.Parse(Graph.SubjectMasterCrespo),
                    InheritanceEnabled = true,
                    TenantLegacyIds = new List<int> {0, 4},
                    TenantType = "Agency"
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.TenantAdform),
                        TenantName = Graph.TenantAdformName,
                        TenantType = Graph.AdformLabel,
                        TenantLegacyId = 0,
                        Roles = new List<string> {Graph.CustomAdformLocalAdminRoleName},
                        Permissions = new List<string>()
                    }
                });
        }

        private static void UnknownSubject(TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.NewGuid(),
                    SubjectId = Guid.NewGuid(),
                    InheritanceEnabled = true,
                },
                new List<RuntimeResult>()
            );

            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.NewGuid(),
                    SubjectId = Guid.NewGuid(),
                    InheritanceEnabled = true,
                },
                new List<RuntimeResult>()
            );

            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.NewGuid(),
                    SubjectId = Guid.NewGuid(),
                    InheritanceEnabled = false,
                },
                new List<RuntimeResult>()
            );

            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.NewGuid(),
                    SubjectId = Guid.NewGuid(),
                    InheritanceEnabled = false,
                },
                new List<RuntimeResult>()
            );
        }

        private static void Actor4Subject1_UnknownTenant(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject4),
                    SubjectId = Guid.Parse(Graph.Subject1),
                    InheritanceEnabled = true,
                    TenantIds = new[] {Guid.Empty}
                },
                new List<RuntimeResult>()
            );
        }

        private static void Actor4Subject1_UnknownPolicy(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject4),
                    SubjectId = Guid.Parse(Graph.Subject1),
                    InheritanceEnabled = true,
                    PolicyNames = new[] {string.Empty}
                },
                new List<RuntimeResult>()
            );
        }

        private static void Actor4Subject1_UnknownPolicyAndTenant(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject4),
                    SubjectId = Guid.Parse(Graph.Subject1),
                    InheritanceEnabled = true,
                    PolicyNames = new[] {string.Empty},
                    TenantIds = new[] {Guid.Empty}
                },
                new List<RuntimeResult>()
            );
        }

        private static void Actor4Subject1_AllTenantsAndPolicies_InheritanceEnabled(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject4),
                    SubjectId = Guid.Parse(Graph.Subject1),
                    InheritanceEnabled = true,
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 1,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    },

                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant2),
                        TenantName = Graph.Tenant2Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant3),
                        TenantName = Graph.Tenant3Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 3,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant4),
                        TenantName = Graph.Tenant4Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant5),
                        TenantName = Graph.Tenant5Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 5,
                        Roles = new List<string> {Graph.CustomRole0Name, Graph.Role3Name},
                        Permissions = new List<string>
                        {
                            Graph.Permission0Name, Graph.Permission5Name, Graph.Permission6Name, Graph.Permission8Name,
                            Graph.Permission13Name, Graph.Permission14Name
                        }
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant6),
                        TenantName = Graph.Tenant6Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 6,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant7),
                        TenantName = Graph.Tenant7Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 7,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    }
                }
            );
        }

        private static void Actor4Subject1_AllTenantsAndPolicies_NoInheritanceEnabled(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject4),
                    SubjectId = Guid.Parse(Graph.Subject1),
                    InheritanceEnabled = false,
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 1,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    }
                }
            );
        }

        private static void Actor4Subject1_SpecificLegacyTenant_NoInheritanceEnabled(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject4),
                    SubjectId = Guid.Parse(Graph.Subject1),
                    TenantLegacyIds = new[] {1},
                    TenantType = Graph.TenantLabel,
                    InheritanceEnabled = false,
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 1,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    }
                }
            );
        }


        private static void Actor4Subject1_SpecificLegacyTenant_InheritanceEnabled(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject4),
                    SubjectId = Guid.Parse(Graph.Subject1),
                    TenantLegacyIds = new[] {1},
                    TenantType = Graph.TenantLabel,
                    InheritanceEnabled = true,
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 1,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    }
                }
            );
        }

        private static void Subject2_AllTenantsAndPolicies_InheritanceEnabled(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject2),
                    SubjectId = Guid.Parse(Graph.Subject2),
                    InheritanceEnabled = true,
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant2),
                        TenantName = Graph.Tenant2Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph.Role2Name},
                        Permissions = new List<string> {Graph.Permission3Name, Graph.Permission4Name}
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant4),
                        TenantName = Graph.Tenant4Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph.Role2Name},
                        Permissions = new List<string> {Graph.Permission3Name, Graph.Permission4Name}
                    },

                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant5),
                        TenantName = Graph.Tenant5Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 5,
                        Roles = new List<string> {Graph.Role2Name},
                        Permissions = new List<string> {Graph.Permission3Name, Graph.Permission4Name}
                    }
                }
            );
        }

        private static void Subject2_AllTenantsAndPolicies_InheritanceDisabled(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject2),
                    SubjectId = Guid.Parse(Graph.Subject2),
                    InheritanceEnabled = false,
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant2),
                        TenantName = Graph.Tenant2Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph.Role2Name},
                        Permissions = new List<string> {Graph.Permission3Name, Graph.Permission4Name}
                    }
                }
            );
        }

        private static void Subject2_TenantLegacyId_InheritanceEnabled(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject2),
                    SubjectId = Guid.Parse(Graph.Subject2),
                    InheritanceEnabled = true,
                    TenantLegacyIds = new[] {2, 4},
                    TenantType = Graph.TenantLabel
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant2),
                        TenantName = Graph.Tenant2Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph.Role2Name},
                        Permissions = new List<string> {Graph.Permission3Name, Graph.Permission4Name}
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant4),
                        TenantName = Graph.Tenant4Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph.Role2Name},
                        Permissions = new List<string> {Graph.Permission3Name, Graph.Permission4Name}
                    }
                }
            );
        }

        private static void Subject2_SpecificTenantForWhichSubjectHasNoDirectAssignment_InheritanceEnabled(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject2),
                    SubjectId = Guid.Parse(Graph.Subject2),
                    InheritanceEnabled = true,
                    TenantIds = new List<Guid> {Guid.Parse(Graph.Tenant5)}
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant5),
                        TenantName = Graph.Tenant5Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 5,
                        Roles = new List<string> {Graph.Role2Name},
                        Permissions = new List<string> {Graph.Permission3Name, Graph.Permission4Name}
                    }
                }
            );
        }

        private static void
            Subject2_SpecificTenantForWhichSubjectHasNoDirectAssignment_NoInheritanceEnabled_NoResultShouldBeReturned(
                TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject2),
                    SubjectId = Guid.Parse(Graph.Subject2),
                    InheritanceEnabled = false,
                    TenantIds = new List<Guid> {Guid.Parse(Graph.Tenant5)}
                },
                new List<RuntimeResult>()
            );
        }

        private static void Actor4Subject5_AllTenantsAndPolicies_InheritanceEnabled_NoResultShouldBeReturned(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject4),
                    SubjectId = Guid.Parse(Graph.Subject5),
                    InheritanceEnabled = true,
                },
                new List<RuntimeResult>()
            );
        }

        private static void Actor5Subject4_AllTenantsAndPolicies_InheritanceEnabled(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject5),
                    SubjectId = Guid.Parse(Graph.Subject4),
                    InheritanceEnabled = true
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 1,
                        Roles = new List<string> {Graph.CustomRole9Name},
                        Permissions = new List<string>()
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant2),
                        TenantName = Graph.Tenant2Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph.CustomRole9Name},
                        Permissions = new List<string>()
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant3),
                        TenantName = Graph.Tenant3Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 3,
                        Roles = new List<string> {Graph.CustomRole9Name},
                        Permissions = new List<string>()
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant4),
                        TenantName = Graph.Tenant4Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph.CustomRole9Name},
                        Permissions = new List<string>()
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant5),
                        TenantName = Graph.Tenant5Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 5,
                        Roles = new List<string> {Graph.CustomRole9Name},
                        Permissions = new List<string>()
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant6),
                        TenantName = Graph.Tenant6Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 6,
                        Roles = new List<string> {Graph.CustomRole9Name},
                        Permissions = new List<string>()
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant7),
                        TenantName = Graph.Tenant7Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 7,
                        Roles = new List<string> {Graph.CustomRole9Name},
                        Permissions = new List<string>()
                    },
                });
        }

        private static void
            Actor6Subject3_SpecificTenantForWhichActorHasNoAssignment_InheritanceEnabled_NoResultShouldBeReturned(
                TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject6),
                    SubjectId = Guid.Parse(Graph.Subject3),
                    TenantIds = new List<Guid> {Guid.Parse(Graph.Tenant5)},
                    InheritanceEnabled = true
                },
                new List<RuntimeResult>());
        }

        private static void Actor5Subject4_AllTenantsAndPolicies_InheritanceDisabled_NoResultShouldBeReturned(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject5),
                    SubjectId = Guid.Parse(Graph.Subject4),
                    InheritanceEnabled = false
                },
                new List<RuntimeResult>
                {
                });
        }

        private static void Actor4Subject2_AllTenantsAndPolicies_InheritanceEnabled(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject4),
                    SubjectId = Guid.Parse(Graph.Subject2),
                    InheritanceEnabled = true
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant2),
                        TenantName = Graph.Tenant2Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant4),
                        TenantName = Graph.Tenant4Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant5),
                        TenantName = Graph.Tenant5Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 5,
                        Roles = new List<string> {Graph.CustomRole0Name, Graph.Role3Name},
                        Permissions = new List<string>
                        {
                            Graph.Permission0Name, Graph.Permission5Name, Graph.Permission6Name, Graph.Permission8Name,
                            Graph.Permission13Name, Graph.Permission14Name
                        }
                    },
                });
        }

        private static void Actor4Subject2_AllTenantsAndPolicies_InheritanceDisabled_NoResultShouldBeReturned(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject4),
                    SubjectId = Guid.Parse(Graph.Subject2),
                    InheritanceEnabled = false
                },
                new List<RuntimeResult>());
        }

        private static void Actor4Subject2_TenantLegacyId_InheritanceEnabled(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject4),
                    SubjectId = Guid.Parse(Graph.Subject2),
                    InheritanceEnabled = true,
                    TenantLegacyIds = new[] {2, 4},
                    TenantType = Graph.TenantLabel
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant2),
                        TenantName = Graph.Tenant2Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 2,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    },
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant4),
                        TenantName = Graph.Tenant4Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 4,
                        Roles = new List<string> {Graph.CustomRole0Name},
                        Permissions = new List<string> {Graph.Permission0Name, Graph.Permission14Name}
                    },
                });
        }

        private static void Actor4Subject2_SpecificTenantForWhichSubjectHasNoDirectAssignment_InheritanceEnabled(
            TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject4),
                    SubjectId = Guid.Parse(Graph.Subject2),
                    InheritanceEnabled = true,
                    TenantIds = new List<Guid> {Guid.Parse(Graph.Tenant5)}
                },
                new List<RuntimeResult>
                {
                    new RuntimeResult
                    {
                        TenantId = Guid.Parse(Graph.Tenant5),
                        TenantName = Graph.Tenant5Name,
                        TenantType = Graph.TenantLabel,
                        TenantLegacyId = 5,
                        Roles = new List<string> {Graph.CustomRole0Name, Graph.Role3Name},
                        Permissions = new List<string>
                        {
                            Graph.Permission0Name, Graph.Permission5Name, Graph.Permission6Name, Graph.Permission8Name,
                            Graph.Permission13Name, Graph.Permission14Name
                        }
                    },
                });
        }

        private static void
            Actor4Subject2_SpecificTenantForWhichSubjectHasNoDirectAssignment_InheritanceDisabled_NoResultShouldBeReturned(
                TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject4),
                    SubjectId = Guid.Parse(Graph.Subject2),
                    InheritanceEnabled = false,
                    TenantIds = new List<Guid> {Guid.Parse(Graph.Tenant5)}
                },
                new List<RuntimeResult>());
        }

        private static void
            Actor4Subject2_SpecificTenantForWhichActorHasNoDirectAssignment_InheritanceEnabled_NoResultShouldBeReturned(
                TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject4),
                    SubjectId = Guid.Parse(Graph.Subject2),
                    InheritanceEnabled = true,
                    TenantIds = new List<Guid> {Guid.Parse(Graph.Tenant3)}
                },
                new List<RuntimeResult>());
        }

        private static void
            Actor4Subject2_SpecificTenantForWhichActorHasNoDirectAssignment_InheritanceDisabled_NoResultShouldBeReturned(
                TheoryData<SubjectIntersectionQuery, IReadOnlyList<RuntimeResult>> data)
        {
            data.Add(
                new SubjectIntersectionQuery
                {
                    ActorId = Guid.Parse(Graph.Subject4),
                    SubjectId = Guid.Parse(Graph.Subject2),
                    InheritanceEnabled = false,
                    TenantIds = new List<Guid> {Guid.Parse(Graph.Tenant3)}
                },
                new List<RuntimeResult>());
        }

        #endregion

        #endregion
    }
}