using Adform.Bloom.Infrastructure;
using Adform.Bloom.Seeder.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Adform.Bloom.Seeder
{
    public class Graph
    {
        private const string CsvPath = "neo4j";

        private static readonly ParallelOptions Options = new ParallelOptions
            {MaxDegreeOfParallelism = Environment.ProcessorCount};

        public static async Task CreateGraph()
        {
            var noOfUsers = 8;
            var noOfPolicies = 1;
            var noOfTemplateRoles = 3;
            var noOfMaxCustomRoles = 5;
            var noOfPermissions = 1600;
            var maxNoOfPermissionsPerRole = (int) (0.23 * noOfPermissions);
            var noOfFeatures = 40;
            var noOfLicensedFeatures = 10;
            var noOfMaxFeaturesPerLicensedFeature = noOfFeatures / noOfLicensedFeatures;
            var noOfMaxPermissionsPerFeature = noOfPermissions / noOfFeatures;
            var noOfMaxTenantsToFeaturesAssignments = 10;
            var maxNoOfRolesPerUser = (int) (1.0 * noOfMaxCustomRoles);
            var noOfTenantHierarchies = 3;
            var noOfTenantsPerHierarchy = 6;
            var numberOfMaxSubjectAssignmentPerRoleWithinSameTenant = 3;

            var sw = Stopwatch.StartNew();

            var root = MeasureIt(() => CreateRootNode("Adform"), "node-root", x => new[] {x.ToCsv()});

            Console.WriteLine("Creating tenants...");
            var tenants = MeasureIt(() => CreateTenants(root.Item1, noOfTenantsPerHierarchy, noOfTenantHierarchies),
                "node-tenants",
                x => x.Select(y => y.ToCsv()));
            var tenantsRelationships = MeasureIt(
                () => CreateRelationship(tenants.Item1, Constants.Relationship.CHILD_OF),
                "relationship-tenants");

            Console.WriteLine("Creating subjects...");
            var subjects = MeasureIt(() => CreateUsersLayer(tenants.Item1, noOfUsers), "node-users",
                x => x.Select(y => y.ToCsv()));

            Console.WriteLine("Creating root policy...");
            var rootPolicy = MeasureIt(() => CreateRootPolicy("RootPolicy"), "node-rootPolicy", x => new[] {x.ToCsv()});

            Console.WriteLine("Creating child policies...");
            var childPolicies = MeasureIt(() => CreateChildPolicies(rootPolicy.Item1, noOfPolicies),
                "node-policies", x => x.Select(y => y.ToCsv()));
            var policiesRelationships = MeasureIt(
                () => CreateRelationship(childPolicies.Item1, Constants.Relationship.CHILD_OF),
                "relationship-policies");

            Console.WriteLine("Creating template roles...");
            var templateRoles = MeasureIt(() => CreateTemplateRoles(childPolicies.Item1, noOfTemplateRoles),
                "node-templateRoles", x => x.Select(y => y.ToCsv()));
            var templateRolesRelationships = MeasureIt(
                () => CreateRelationship(templateRoles.Item1, Constants.Relationship.CONTAINS),
                "relationship-templateRoles");

            Console.WriteLine("Creating custom roles...");
            var customRoles = MeasureIt(
                () => CreateCustomRoles(childPolicies.Item1.ToArray(), tenants.Item1,
                    noOfMaxCustomRoles),
                "node-customRoles", x => x.SelectMany(y => y.Value.Select(z => z.ToCsv())));
            var customRolesRelationships = MeasureIt(
                () => CreateRelationship(customRoles.Item1.SelectMany(x => x.Value), Constants.Relationship.CONTAINS),
                "relationship-customRoles");


            //Group to tenant, then group to role and finally user to group
            Console.WriteLine("Creating template groups...");
            var templateGroups = MeasureIt(() => CreateTemplateGroups(templateRoles.Item1, subjects.Item1),
                "node-templateGroups", x => x.SelectMany(y => y.Value.Select(z => z.ToCsv())));
            var templateGroupsRelationships = MeasureIt(
                () => CreateRelationship(templateGroups.Item1.SelectMany(x => x.Value), Constants.Relationship.BELONGS,
                    Constants.Relationship.ASSIGNED),
                "relationship-templateGroups");

            Console.WriteLine("Creating custom groups...");
            var customGroups = MeasureIt(() => CreateCustomGroups(customRoles.Item1),
                "node-customGroups", x => x.SelectMany(y => y.Value.Select(z => z.ToCsv())));
            var templateCustomRelationships = MeasureIt(
                () => CreateRelationship(customGroups.Item1.SelectMany(x => x.Value), Constants.Relationship.BELONGS,
                    Constants.Relationship.ASSIGNED),
                "relationship-customGroups");

            Console.WriteLine("Creating subject assignments...");
            var subjectAssignments = MeasureIt(
                () => CreateSubjectAssignments(subjects.Item1, tenants.Item1, templateGroups.Item1, customGroups.Item1,
                    maxNoOfRolesPerUser, numberOfMaxSubjectAssignmentPerRoleWithinSameTenant),
                "relationship-users");

            Console.WriteLine("Creating permissions...");
            var permissions = MeasureIt(
                () => CreatePermissions(noOfPermissions),
                "node-permissions", x => x.Select(y => y.ToCsv()));
            var permissionsRelationships = MeasureIt(() =>
                CreatePermissionsToRolesAssignments(
                    permissions.Item1.ToArray(),
                    templateRoles.Item1, customRoles.Item1,
                    maxNoOfPermissionsPerRole), "relationship-permissions");

            Console.WriteLine("Creating features...");
            var ff = MeasureIt(
                () => CreateFeatures(noOfFeatures),
                "node-features", x => x.Select(y => y.ToCsv()));
            var ffRelationships = MeasureIt(() =>
                CreateFeaturesToPermissionsAssignments(ff.Item1, permissions.Item1.ToArray(),
                    noOfMaxPermissionsPerFeature), "relationship-features");

            Console.WriteLine("Creating Licensed features...");
            var lf = MeasureIt(
                () => CreateLicensedFeatures(noOfLicensedFeatures),
                "node-licensedFeatures", x => x.Select(y => y.ToCsv()));
            var lfRelationships = MeasureIt(() =>
                CreateLicensedFeaturesToFeaturesAssignments(lf.Item1, ff.Item1.ToArray(),
                    noOfMaxFeaturesPerLicensedFeature), "relationship-licensedFeatures");


            Console.WriteLine("Creating custom roles assignments to tenants...");
            var customRolesAssignments = MeasureIt(
                () => CreateCustomRolesAssignments(customRoles.Item1), "relationship-roles");

            Console.WriteLine("Creating tenants to features assignments...");
            var tenantsToFFAssignments = MeasureIt(
                () => CreateTenantToFeaturesAssignments(ff.Item1.ToArray(), tenants.Item1,
                    noOfMaxTenantsToFeaturesAssignments),
                "relationship-tenantFeatures");

            var featureDependencies = MeasureIt(() => CreateFeatureDependencies(ff.Item1.ToList()),
                "relationship-featureDependencies");

            await Task.WhenAll(tenantsRelationships.Item2,
                policiesRelationships.Item2, templateRolesRelationships.Item2, customRolesRelationships.Item2,
                templateGroupsRelationships.Item2,
                templateCustomRelationships.Item2,
                permissionsRelationships.Item2,
                ffRelationships.Item2,
                lfRelationships.Item2,
                tenants.Item2,
                root.Item2.ContinueWith(t => t.Dispose()),
                subjects.Item2.ContinueWith(t => t.Dispose()),
                rootPolicy.Item2.ContinueWith(t => t.Dispose()),
                childPolicies.Item2.ContinueWith(t => childPolicies.Item1.Clear()).ContinueWith(t => t.Dispose()),
                templateRoles.Item2.ContinueWith(t => t.Dispose()),
                customRoles.Item2.ContinueWith(t => t.Dispose()),
                customGroups.Item2.ContinueWith(t => t.Dispose()),
                templateGroups.Item2.ContinueWith(t => t.Dispose()),
                permissions.Item2.ContinueWith(t => permissions.Item1.Clear()).ContinueWith(t => t.Dispose()),
                customRolesAssignments.Item2.ContinueWith(t => t.Dispose()),
                subjectAssignments.Item2.ContinueWith(t => t.Dispose()),
                ff.Item2.ContinueWith(t => t.Dispose()),
                lf.Item2.ContinueWith(t => t.Dispose()),
                tenantsToFFAssignments.Item2.ContinueWith(t => t.Dispose()),
                featureDependencies.Item2);

            //GC.Collect(2, GCCollectionMode.Default);

            sw.Stop();

            Console.WriteLine($"Total: {sw.Elapsed}");
        }

        private static ConcurrentDictionary<Guid, ConcurrentBag<Group>> CreateCustomGroups(
            ConcurrentDictionary<Guid, ConcurrentBag<Role>> customRolesItem)
        {
            var res = new ConcurrentDictionary<Guid, ConcurrentBag<Group>>();
            Parallel.ForEach(customRolesItem, tenant =>
            {
                var bag = new ConcurrentBag<Group>();
                Parallel.ForEach(tenant.Value, role =>
                {
                    var child = new Group($"Group_{tenant.Key}-{role.Id}", "Group", tenant.Key, role.Id);
                    bag.Add(child);
                });
                res.TryAdd(tenant.Key, bag);
            });

            return res;
        }

        private static ConcurrentDictionary<Guid, ConcurrentBag<Group>> CreateTemplateGroups(
            ConcurrentBag<Role> templateRolesItem, ConcurrentBag<Subject> subjectsItem)
        {
            var res = new ConcurrentDictionary<Guid, ConcurrentBag<Group>>();
            var tenants = subjectsItem.Select(o => o.TargetId).ToList();
            Parallel.ForEach(tenants, tenant =>
            {
                var bag = new ConcurrentBag<Group>();
                Parallel.ForEach(templateRolesItem, role =>
                {
                    var child = new Group($"Group_{tenants}-{role.Id}", "Group", tenant, role.Id);
                    bag.Add(child);
                });
                res.TryAdd(tenant.Value, bag);
            });

            return res;
        }

        private static (T, Task) MeasureIt<T>(Func<T> run, string key, Func<T, IEnumerable<string>> selector = null)
        {
            var importPath = $"{CsvPath}/import";
            var dirs = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), importPath),
                Path.Combine(Directory.GetCurrentDirectory(), importPath, "nodes"),
                Path.Combine(Directory.GetCurrentDirectory(), importPath, "node-headers"),
                Path.Combine(Directory.GetCurrentDirectory(), importPath, "links"),
                Path.Combine(Directory.GetCurrentDirectory(), importPath, "link-headers")
            };

            foreach (var dir in dirs)
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

            var sw = new Stopwatch();
            sw.Start();

            var x = run();

            sw.Stop();

            var elapsed = sw.Elapsed;
            var task = Task.Run(async () =>
            {
                StreamWriter writer = null;
                try
                {
                    if (selector != null)
                    {
                        await using var headersWriter = new StreamWriter(new FileStream(
                            $"./{importPath}/node-headers/{key.Replace("-", "-header-")}.csv", // relationship-header-type
                            FileMode.Create, FileAccess.Write));
                        await headersWriter.WriteLineAsync(NamedNode.AddHeaders());
                        await headersWriter.FlushAsync();
                        writer = new StreamWriter(new FileStream($"./{importPath}/nodes/{key}.csv", FileMode.Create,
                            FileAccess.Write));
                    }
                    else
                    {
                        await using var headersWriter = new StreamWriter(new FileStream(
                            $"./{importPath}/link-headers/{key.Replace("-", "-header-")}.csv", // relationship-header-type
                            FileMode.Create, FileAccess.Write));
                        await headersWriter.WriteLineAsync(":END_ID|:TYPE|:START_ID");
                        await headersWriter.FlushAsync();
                        writer = new StreamWriter(new FileStream($"./{importPath}/links/{key}.csv", FileMode.Create,
                            FileAccess.Write));
                    }

                    foreach (var c in selector != null ? selector(x) : (IEnumerable<string>) x)
                        await writer.WriteLineAsync(c);

                    var count = (selector != null ? selector(x) : (IEnumerable<string>) x).Count();

                    Console.WriteLine($"{key,50} {count,20} {elapsed,20}");
                    await writer.FlushAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    if (writer != null) await writer.DisposeAsync();
                }
            });

            return (x, task);
        }

        private static ConcurrentBag<string> CreateRelationship<T>(IEnumerable<T> nodes, string name, string second)
            where T : NamedNode
        {
            var bag = new ConcurrentBag<string>();

            Parallel.ForEach(nodes, Options, node =>
            {
                if (node.TargetId != null)
                    bag.Add(new Relationship<Guid>(node.Id, node.TargetId.Value, name).ToString());

                if (node.SecondTargetId != null)
                    bag.Add(new Relationship<Guid>(node.Id, node.SecondTargetId.Value, second).ToString());
            });

            return bag;
        }

        private static ConcurrentBag<string> CreateRelationship<T>(IEnumerable<T> nodes, string name) where T : NamedNode
        {
            var bag = new ConcurrentBag<string>();

            Parallel.ForEach(nodes, Options, node =>
            {
                if (node.TargetId != null)
                    bag.Add(new Relationship<Guid>(node.Id, node.TargetId.Value, name).ToString());

                if (node.ChildId != null) bag.Add(new Relationship<Guid>(node.ChildId.Value, node.Id, name).ToString());
            });

            return bag;
        }

        private static Tenant CreateRootNode(string name)
        {
            var root = new Tenant(name, "Adform;Tenant", null);

            return root;
        }

        private static Policy CreateRootPolicy(string name)
        {
            var root = new Policy(name, "Root;Policy", null);

            return root;
        }

        private static Tenant[] CreateTenants(Tenant parentTenant, int count, int level)
        {
            var res = new ConcurrentBag<Tenant>();
            FillBagWithTenants(parentTenant, count, level, res);
            return res.ToArray();

            static void FillBagWithTenants(Tenant pt, int c, int l, ConcurrentBag<Tenant> bag)
            {
                if (l == 1)
                {
                    Parallel.For(0, c, i => { bag.Add(new Tenant("BusinessAccount", "Tenant", pt.Id)); });
                    return;
                }

                Parallel.For(0, c, i =>
                {
                    var parent = new Tenant("BusinessAccount", "Tenant", pt.Id);
                    bag.Add(parent);
                    FillBagWithTenants(parent, c, l - 1, bag);
                });
            }
        }

        private static ConcurrentBag<Subject> CreateUsersLayer(
            IEnumerable<Tenant> tenants,
            int count)
        {
            var res = new ConcurrentBag<Subject>();
            var nums = Enumerable.Range(0, count).ToArray();

            Parallel.ForEach(tenants, Options, tenant =>
            {
                Parallel.ForEach(nums, Options, num =>
                {
                    var child = new Subject($"Subject_{num}", "Subject", tenant.Id);
                    res.Add(child);
                });
            });

            return res;
        }

        private static IEnumerable<Subject> CreateSubjects(int tenantsCount, int maxCount)
        {
            var res = new ConcurrentBag<Subject>();

            Parallel.For(0, tenantsCount * maxCount / 2, Options,
                i => { res.Add(new Subject($"Subject_{i}", "Subject", null)); });

            return res;
        }

        private static ConcurrentBag<Policy> CreateChildPolicies(Policy root, int count)
        {
            var nums = Enumerable.Range(0, count).ToArray();
            var res = new ConcurrentBag<Policy>();

            Parallel.ForEach(nums, Options, num =>
            {
                var child = new Policy("Policy", "Policy", root.Id);
                res.Add(child);
            });
            return res;
        }

        private static ConcurrentBag<Role> CreateTemplateRoles(IEnumerable<Policy> policies, int count)
        {
            var res = new ConcurrentBag<Role>();

            Parallel.ForEach(policies, Options, policy =>
            {
                var nums = Enumerable.Range(0, count).ToArray();

                Parallel.ForEach(nums, Options, num =>
                {
                    var child = new Role("Role", "Role", null, policy.Id);
                    res.Add(child);
                });
            });

            return res;
        }

        private static ConcurrentDictionary<Guid, ConcurrentBag<Role>> CreateCustomRoles(Policy[] policies,
            IEnumerable<Tenant> tenants, int maxCount)
        {
            var res = new ConcurrentDictionary<Guid, ConcurrentBag<Role>>();

            Parallel.ForEach(tenants, Options, agency =>
            {
                var bag = new ConcurrentBag<Role>();
                var nums = Enumerable.Range(0, ThreadSafeRandom.CurrentThreadInstance.Next(3, maxCount)).ToArray();

                Parallel.ForEach(nums, Options, num =>
                {
                    var policyId = policies[ThreadSafeRandom.CurrentThreadInstance.Next(0, policies.Length - 1)].Id;
                    var child = new Role("Role", "Role;CustomRole", null, policyId);
                    bag.Add(child);
                });

                res.TryAdd(agency.Id, bag);
            });

            return res;
        }

        private static ConcurrentBag<Permission> CreatePermissions(int maxCount)
        {
            var res = new ConcurrentBag<Permission>();
            var nums = Enumerable.Range(0, maxCount);

            Parallel.ForEach(nums, Options,
                n => { res.Add(new Permission($"Permission_{n}", "Permission", null, null)); });

            return res;
        }

        private static ConcurrentBag<Permission> CreatePermissions(IEnumerable<Role> templateRoles,
            ConcurrentDictionary<Guid, ConcurrentBag<Role>> customRoles, int maxCount)
        {
            var res = new ConcurrentBag<Permission>();

            Parallel.ForEach(templateRoles, Options, role =>
            {
                var random = ThreadSafeRandom.CurrentThreadInstance.Next(3, maxCount);
                var nums = Enumerable.Range(0, random).ToArray();

                Parallel.ForEach(nums, Options, num =>
                {
                    var child = new Permission("Permission", "Permission", null, role.Id);
                    res.Add(child);
                });
            });

            Parallel.ForEach(customRoles.SelectMany(x => x.Value), Options, role =>
            {
                var nums = Enumerable.Range(0, ThreadSafeRandom.CurrentThreadInstance.Next(3, maxCount)).ToArray();

                Parallel.ForEach(nums, Options, num =>
                {
                    var child = new Permission("Permission", "Permission", null, role.Id);
                    res.Add(child);
                });
            });

            return res;
        }

        private static ConcurrentBag<string> CreatePermissionsToRolesAssignments(
            IReadOnlyList<Permission> permissions,
            IEnumerable<Role> templateRoles,
            ConcurrentDictionary<Guid, ConcurrentBag<Role>> customRoles,
            int maxCount)
        {
            var res = new ConcurrentBag<string>();


            Parallel.ForEach(templateRoles, Options, role =>
            {
                var random = ThreadSafeRandom.CurrentThreadInstance.Next(3, maxCount);
                FillBag(random, role.Id, permissions, Constants.Relationship.CONTAINS, res);
            });

            Parallel.ForEach(customRoles.SelectMany(x => x.Value), Options, role =>
            {
                var random = ThreadSafeRandom.CurrentThreadInstance.Next(3, maxCount);

                FillBag(random, role.Id, permissions, Constants.Relationship.CONTAINS, res);
            });

            return res;
        }

        private static ConcurrentBag<Feature> CreateFeatures(int noOfFeatures)
        {
            var res = new ConcurrentBag<Feature>();
            var nums = Enumerable.Range(0, noOfFeatures);

            Parallel.ForEach(nums, Options,
                n => { res.Add(new Feature($"Feature_{n}", "Feature", null, null)); });

            return res;
        }

        private static ConcurrentBag<LicensedFeature> CreateLicensedFeatures(int noOfLicensedFeatures)
        {
            var res = new ConcurrentBag<LicensedFeature>();
            var nums = Enumerable.Range(0, noOfLicensedFeatures);

            Parallel.ForEach(nums, Options,
                n => { res.Add(new LicensedFeature($"LicensedFeature_{n}", "LicensedFeature", null, null)); });

            return res;
        }

        private static ConcurrentBag<string> CreateSubjectAssignments(
            IEnumerable<Subject> subjects,
            Tenant[] tenants,
            ConcurrentDictionary<Guid, ConcurrentBag<Group>> templateGroups,
            ConcurrentDictionary<Guid, ConcurrentBag<Group>> customGroups,
            int maxNoOfRolesPerUser,
            int numberOfMaxSubjectAssignmentPerRoleWithinSameTenant)
        {
            var res = new ConcurrentBag<string>();

            Parallel.ForEach(subjects, Options,
                sub =>
                {
                    Parallel.ForEach(templateGroups[sub.TargetId.Value], Options,
                        group =>
                        {
                            res.Add(
                                new Relationship<Guid>(sub.Id, group.Id, Constants.Relationship.MEMBER_OF).ToString());
                        });
                });

            Parallel.ForEach(subjects, Options, sub => { AssignSubjectToCustomGroups(sub.Id, res); });

            return res;

            void AssignSubjectToCustomGroups(Guid subId, ConcurrentBag<string> bag)
            {
                if (maxNoOfRolesPerUser > tenants.Length)
                {
                    throw new InvalidOperationException("Number of roles cannot exceed number of tenants");
                }

                var state = new List<int>();

                for (int i = 0; i < maxNoOfRolesPerUser; i++)
                {
                    int tenantIndex;
                    do
                    {
                        tenantIndex = ThreadSafeRandom.CurrentThreadInstance.Next(0, tenants.Length - 1);
                    } while (state.Contains(tenantIndex));

                    state.Add(tenantIndex);

                    var tenant = tenants[tenantIndex];
                    var groups = customGroups[tenant.Id].ToArray();

                    var groupAssignmentsState = Enumerable
                        .Range(0, numberOfMaxSubjectAssignmentPerRoleWithinSameTenant)
                        .Select(n => -1).ToArray();
                    var noOfAssignments = ThreadSafeRandom.CurrentThreadInstance.Next(1,
                        numberOfMaxSubjectAssignmentPerRoleWithinSameTenant);

                    for (int j = 0; j < noOfAssignments; j++)
                    {
                        int index;
                        do
                        {
                            index = ThreadSafeRandom.CurrentThreadInstance.Next(0, groups.Length - 1);
                        } while (groupAssignmentsState.Contains(index));

                        groupAssignmentsState[j] = index;
                        var group = groups[index];
                        bag.Add(new Relationship<Guid>(subId, group.Id, Constants.Relationship.MEMBER_OF).ToString());
                        if (j > 0) i++;
                    }
                }
            }
        }

        private static ConcurrentBag<string> CreateCustomRolesAssignments(
            ConcurrentDictionary<Guid, ConcurrentBag<Role>> customRoles)
        {
            var res = new ConcurrentBag<string>();

            Parallel.ForEach(customRoles, Options, kv =>
            {
                Parallel.ForEach(kv.Value, Options,
                    role =>
                    {
                        res.Add(new Relationship<Guid>(kv.Key, role.Id, Constants.Relationship.OWNS).ToString());
                    });
            });

            return res;
        }

        private static ConcurrentBag<string> CreateFeaturesToPermissionsAssignments(
            IEnumerable<Feature> features,
            IReadOnlyList<Permission> permissions, int maxCount)
        {
            var res = new ConcurrentBag<string>();
            var sharedState = new List<int>();

            Parallel.ForEach(features, Options,
                ff => { FillBag(maxCount, ff.Id, permissions, Constants.Relationship.CONTAINS, res, sharedState); });

            return res;
        }


        private static ConcurrentBag<string> CreateLicensedFeaturesToFeaturesAssignments(
            IEnumerable<LicensedFeature> licensedFeatures,
            IReadOnlyList<Feature> features, int maxCount)
        {
            var res = new ConcurrentBag<string>();
            var sharedState = new List<int>();

            Parallel.ForEach(licensedFeatures, Options,
                lf => { FillBag(maxCount, lf.Id, features, Constants.Relationship.CONTAINS, res, sharedState); });

            return res;
        }

        private static ConcurrentBag<string> CreateTenantToFeaturesAssignments(
            IReadOnlyList<Feature> features,
            IEnumerable<Tenant> tenants, int maxCount)
        {
            var res = new ConcurrentBag<string>();

            Parallel.ForEach(tenants, Options, a =>
            {
                var random = ThreadSafeRandom.CurrentThreadInstance.Next(3, maxCount);
                FillBag(random, a.Id, features, Constants.Relationship.ASSIGNED, res);
            });

            return res;
        }

        private static IEnumerable<string> CreateFeatureDependencies(IReadOnlyList<Feature> features)
        {
            var res = new List<string>();

            for (var i = 0; i < features.Count; i += 2)
            {
                int index;

                do
                {
                    index = ThreadSafeRandom.CurrentThreadInstance.Next(0, features.Count - 1);
                } while (index == i);

                res.Add(
                    new Relationship<Guid>(features[i].Id, features[index].Id, Constants.Relationship.DEPENDS_ON)
                        .ToString());
            }

            return res;
        }

        private static void FillBag(int count, Guid fromEntityId,
            IReadOnlyList<NamedNode> targetNodes, string relationshipName, ConcurrentBag<string> bag,
            IList<int> sharedState = null)
        {
            var state = sharedState ?? new List<int>();

            for (var i = 0; i < count; i++)
            {
                int value;
                do
                {
                    value = ThreadSafeRandom.CurrentThreadInstance.Next(0, targetNodes.Count);
                } while (state.Contains(value));

                state.Add(value);
                var ff = targetNodes[value];
                bag.Add(new Relationship<Guid>(fromEntityId, ff.Id, relationshipName).ToString());
            }
        }

        private static class ThreadSafeRandom
        {
            private static readonly ThreadLocal<Random> Random =
                new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

            public static Random CurrentThreadInstance => Random.Value;
        }
    }
}