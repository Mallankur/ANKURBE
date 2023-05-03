# Business Rules

<div class="table-wrapper">
    <table>
    <tr>
        <th>Policies</th>
        <th>Tenants</th>
        <th>Licensed Features</th>
        <th>Features</th>
        <th>Permissions</th>
        <th>Role</th>
        <th>Template Role</th>
        <th>User (Subjects)</th>
    </tr>
    <tr>
        <td>**Tenant specific**</td>
        <td>NO</td>
        <td>NO</td>
        <td>NO</td>
        <td>NO</td>
        <td>NO</td>
        <td>YES</td>
        <td>YES</td>
        <td>YES</td>
    </tr>
    <tr>
        <td>**Policy specific**</td>
        <td>NO</td>
        <td>NO</td>
        <td>NO</td>
        <td>NO</td>
        <td>NO</td>
        <td>YES</td>
        <td>YES</td>
        <td>NO</td>
    </tr>
    <tr>
        <td>**How visibility is controlled?**</td>
        <td>Globally visible.</td>
        <td>Controlled via assignments (of subjects to tenants via role assignments).</td>
        <td>Controlled via assignments (Feature to Tenant). This is mainly used to group Features in the UI.</td>
        <td>Controlled via features mappings.</td>
        <td>Controlled via features mappings.</td>
        <td>Controlled via assignments (of subjects to tenants via role assignments).</td>
        <td>Globally visible.</td>
        <td>Controlled via assignments (of subjects to tenants via role assignments).</td>
    </tr>
    <tr>
        <td>**When one can see given entity**</td>
        <td>One can see all policies.</td>
        <td>One can see tenants he is assigned to (via role assignment). <b>Please note that roles can be inherited. Let's assume that there is a parent tenant T1 with a role R1 and a subject S1 is assigned to this role in context of T1. Also let's assume that there is child tenant T2 that inhertis R1. In this case S1 will have access to T2.</b></td>
        <td>One can see the licensed features assigned to a tenant.</td>
        <td>One can see a feature if it is assigned to a tenant that: <br/><br/><ul><li>has given feature assigned</li><li>or inherited given feature.</li></ul>.</td>
        <td>One can see a permission if he has access to a proper feature i.e. a feature that contains given permission.</td>
        <td>One can see a role if he is assigned to a tenant that: <br/><br/><ul><li>is the owner of a given role</li><li>or inherited given role.</li></ul>.</td>
        <td>One can see all template roles.</td>
        <td>One can see a subject if given subject is assigned:<br/><br/><ul><li>to the same tenant</li><li>or successors of a given tenant.</li></ul>Admin of GHG Account can see all subjects of children accounts, e.g. Ikea Global will see all subjects of IKEA_PL.</td>
    </tr>
    <tr>
        <td>**Inheritance**</td>
        <td>N/A</td>
        <td>N/A</td>
        <td>N/A</td>
        <td>Inherited from predecessor tenants. There is also the concept of Co-Dependency at Feature level which allow that Permissions requirements validations.</td>
        <td>Inherited from predecessor tenants (via features inheritance).</td>
        <td>Inherited from predecessor tenants.</td>
        <td>N/A</td>
        <td>N/A</td>
    </tr>
    <tr>
        <td>**Who can perform CRUD operations**</td>
        <td>Only Adform Admin.</td>
        <td>Only Adform Admin.</td>
        <td>Only Adform Admin.</td>
        <td>Only Adform Admin.</td>
        <td>Only Adform Admin.</td>
        <td>One can manage a role if he can see a role and has correct rights.</td>
        <td>Only Adform Admin.</td>
        <td>Adform and Local Admin.</td>
    </tr>
    </table>
</div>
