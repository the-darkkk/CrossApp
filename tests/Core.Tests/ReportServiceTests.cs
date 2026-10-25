using Core.Domain;
using Core.Reports;
using Xunit;

namespace Core.Tests;

public sealed class ReportServiceTests
{
    [Theory]
    [InlineData(10, 2)]
    [InlineData(1, 0)]
    public void LowStock_CountsOnlyBelowThreshold(int threshold, int expected)
    {
        var result = ReportService.LowStock(TestData.Items(), threshold);
        Assert.Equal(expected, result.Count);
    }

    [Fact]
    public void TopByQuantity_IsDeterministic()
    {
        var top = ReportService.TopByQuantity(TestData.Items(), 2);
        Assert.Equal(new[] { "SKU-BIG", "SKU-MID" }, top.Select(l => l.Sku));
    }

    [Fact]
    public void Totals_OnEmptyCollection_DoesNotThrow_AndReturnsZeros()
    {
        var totals = ReportService.Totals(Enumerable.Empty<Book>());
        Assert.Equal(0, totals.Items);
        Assert.Equal(0, totals.Total);
        Assert.Equal(0d, totals.Average);
        Assert.Equal(0, totals.Max);
    }

    [Fact]
    public void ByUnit_GroupsCorrectlyAndOrdersByTotalDescending()
    {
        var groups = ReportService.ByUnit(TestData.Items());
        Assert.Equal(2, groups.Count);
        Assert.Equal("шт", groups[0].Key);
        Assert.Equal(35, groups[0].TotalQuantity); // 20 + 15
        Assert.Equal("м", groups[1].Key);
        Assert.Equal(3, groups[1].TotalQuantity);  // 2 + 1
    }
}

