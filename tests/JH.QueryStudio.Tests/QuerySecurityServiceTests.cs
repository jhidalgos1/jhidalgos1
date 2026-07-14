using JH.QueryStudio.Core.Security;
using Xunit;

namespace JH.QueryStudio.Tests;

public sealed class QuerySecurityServiceTests
{
    [Fact]
    public void Detects_Update_Without_Where()
    {
        var risks = QuerySafetyAnalyzer.Analyze("UPDATE dbo.Clientes SET Nombre='A'");
        Assert.Contains("UPDATE sin WHERE.", risks);
    }

    [Fact]
    public void Detects_Delete_Without_Where()
    {
        var risks = QuerySafetyAnalyzer.Analyze("DELETE FROM dbo.Clientes");
        Assert.Contains("DELETE sin WHERE.", risks);
    }

    [Fact]
    public void Allows_Filtered_Delete()
    {
        var risks = QuerySafetyAnalyzer.Analyze("DELETE FROM dbo.Clientes WHERE Id = 10");
        Assert.DoesNotContain("DELETE sin WHERE.", risks);
    }
}
