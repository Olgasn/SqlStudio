using SlqStudio.Application.SQL.Utils;

namespace SlqStudio.Tests;

public sealed class DataComparerTests
{
    private readonly DataComparer _comparer = new();

    private static List<Dictionary<string, object>> Rows(params (string Key, object Value)[][] rows)
    {
        return rows
            .Select(row => row.ToDictionary(cell => cell.Key, cell => cell.Value))
            .ToList();
    }

    [Fact]
    public void CompareResults_WhenIdentical_ReturnsTrue()
    {
        var a = Rows(
            new[] { ("Id", (object)1), ("Name", (object)"Alice") },
            new[] { ("Id", (object)2), ("Name", (object)"Bob") });
        var b = Rows(
            new[] { ("Id", (object)1), ("Name", (object)"Alice") },
            new[] { ("Id", (object)2), ("Name", (object)"Bob") });

        Assert.True(_comparer.CompareResults(a, b));
    }

    [Fact]
    public void CompareResults_WhenRowCountDiffers_ReturnsFalse()
    {
        var a = Rows(new[] { ("Id", (object)1) }, new[] { ("Id", (object)2) });
        var b = Rows(new[] { ("Id", (object)1) });

        Assert.False(_comparer.CompareResults(a, b));
    }

    [Fact]
    public void CompareResults_WhenColumnNamesDiffer_ReturnsFalse()
    {
        var a = Rows(new[] { ("Id", (object)1) });
        var b = Rows(new[] { ("Code", (object)1) });

        Assert.False(_comparer.CompareResults(a, b));
    }

    [Fact]
    public void CompareResults_WhenColumnOrderDiffers_ReturnsFalse()
    {
        // Имена столбцов сравниваются по индексу, поэтому порядок важен.
        var a = Rows(new[] { ("Id", (object)1), ("Name", (object)"Alice") });
        var b = Rows(new[] { ("Name", (object)"Alice"), ("Id", (object)1) });

        Assert.False(_comparer.CompareResults(a, b));
    }

    [Fact]
    public void CompareResults_ColumnNameMatchIsCaseInsensitive()
    {
        // И список столбцов, и поиск данных регистронезависимы, поэтому "Id" == "ID".
        var a = Rows(new[] { ("Id", (object)1) });
        var b = Rows(new[] { ("ID", (object)1) });

        Assert.True(_comparer.CompareResults(a, b));
    }

    [Fact]
    public void CompareResults_WhenBothEmpty_ReturnsTrue()
    {
        var a = new List<Dictionary<string, object>>();
        var b = new List<Dictionary<string, object>>();

        Assert.True(_comparer.CompareResults(a, b));
    }

    [Fact]
    public void CompareResults_WhenOnlyOneEmpty_ReturnsFalse()
    {
        var a = new List<Dictionary<string, object>>();
        var b = Rows(new[] { ("Id", (object)1) });

        Assert.False(_comparer.CompareResults(a, b));
    }

    [Fact]
    public void CompareResults_WhenTypesDifferButValuesConvertEqual_ReturnsTrue()
    {
        // "5" (string) и 5 (int) приводятся друг к другу и считаются равными.
        var a = Rows(new[] { ("Code", (object)"5") });
        var b = Rows(new[] { ("Code", (object)5) });

        Assert.True(_comparer.CompareResults(a, b));
    }

    [Fact]
    public void CompareResults_WhenTypesDifferAndValuesNotEqual_ReturnsFalse()
    {
        // Конвертация удаётся, но "5" != 6 — наборы должны различаться.
        var a = Rows(new[] { ("Code", (object)"5") });
        var b = Rows(new[] { ("Code", (object)6) });

        Assert.False(_comparer.CompareResults(a, b));
    }

    [Fact]
    public void CompareResults_WhenTypesDifferAndNotConvertible_ReturnsFalse()
    {
        var a = Rows(new[] { ("Code", (object)"abc") });
        var b = Rows(new[] { ("Code", (object)5) });

        Assert.False(_comparer.CompareResults(a, b));
    }

    [Fact]
    public void CompareResults_WhenNumericValuesEqualAcrossTypes_ReturnsTrue()
    {
        // int, long и decimal с одинаковым значением считаются равными.
        var a = Rows(new[] { ("Amount", (object)5) });
        var b = Rows(new[] { ("Amount", (object)5L) });
        var c = Rows(new[] { ("Amount", (object)5m) });

        Assert.True(_comparer.CompareResults(a, b));
        Assert.True(_comparer.CompareResults(a, c));
        Assert.True(_comparer.CompareResults(b, c));
    }

    [Fact]
    public void CompareResults_WhenNumericValuesDiffer_ReturnsFalse()
    {
        var a = Rows(new[] { ("Amount", (object)5) });
        var b = Rows(new[] { ("Amount", (object)6) });

        Assert.False(_comparer.CompareResults(a, b));
    }

    [Fact]
    public void CompareResults_WhenBothValuesNull_ReturnsTrue()
    {
        var a = Rows(new[] { ("Name", (object)null!) });
        var b = Rows(new[] { ("Name", (object)null!) });

        Assert.True(_comparer.CompareResults(a, b));
    }

    [Fact]
    public void CompareResults_WhenOnlyOneValueNull_ReturnsFalse()
    {
        var a = Rows(new[] { ("Name", (object)null!) });
        var b = Rows(new[] { ("Name", (object)"Alice") });

        Assert.False(_comparer.CompareResults(a, b));
    }

    [Fact]
    public void CompareResults_WhenStringValuesDiffer_ReturnsFalse()
    {
        var a = Rows(new[] { ("Name", (object)"Alice") });
        var b = Rows(new[] { ("Name", (object)"Bob") });

        Assert.False(_comparer.CompareResults(a, b));
    }

    [Fact]
    public void CompareResults_WhenSameStringValues_ReturnsTrue()
    {
        var a = Rows(new[] { ("Name", (object)"Alice") });
        var b = Rows(new[] { ("Name", (object)"Alice") });

        Assert.True(_comparer.CompareResults(a, b));
    }
}
