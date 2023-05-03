using Adform.Bloom.DataAccess.Providers.Extensions;
using Adform.Bloom.Infrastructure.Models;
using Adform.Ciam.OngDb.Repository;
using Moq;
using Neo4jClient;
using Neo4jClient.Cypher;
using Xunit;

namespace Adform.Bloom.Unit.Test.DataAccess;

public class RoleProviderExtensions
{
    [Theory]
    [MemberData(nameof(SortingParamsGenerator))]
    public void RoleOrderByTest(SortingParams sortingParams, string expectedOrder)
    {   
        // Arrange
        var client = new Mock<IRawGraphClient>();
       
        // Act
        var query = new CypherFluentQuery(client.Object)
            .Match("n")
            .Return<object>("n")
            .RoleOrderBy("n", sortingParams)
            .Query;

        // Assert
        Assert.Equal($"MATCH n\nRETURN n\nORDER BY {expectedOrder}", query.QueryText.ReplaceLineEndings("\n"));
    }
    
    public static TheoryData<SortingParams, string> SortingParamsGenerator()
    {
        return new  TheoryData<SortingParams, string>
        {
            {new SortingParams(), "n.Id asc"},
            {new SortingParams
            {
                SortingOrder = SortingOrder.Descending
            }, "n.Id desc"},
            {new SortingParams
            {
                OrderBy = "Name",
                SortingOrder = SortingOrder.Ascending
            }, "TOLOWER(TOSTRING(n.Name)) asc, n.Id asc"},
            {new QueryParamsRoles
            {
                PrioritizeTemplateRoles = true,
                OrderBy = "Name",
                SortingOrder = SortingOrder.Ascending
            }, "n.Type desc, TOLOWER(TOSTRING(n.Name)) asc, n.Id asc"}
        };
    }
}