namespace SlqStudio.Tests;

public sealed class SqlManagerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenConnectionStringIsInvalid_ThrowsArgumentException(string? connectionString)
    {
        Assert.Throws<ArgumentException>(() => new SqlManager(connectionString!));
    }

    [Theory]
    [InlineData("INSERT INTO Students (Id, Name) VALUES (1, 'A')", "INSERT_Students")]
    [InlineData("UPDATE Students SET Name='A' WHERE Id=1", "UPDATE_Students")]
    [InlineData("DELETE FROM Students WHERE Id=1", "DELETE_Students")]
    [InlineData("SELECT * FROM Students", "SELECT_Students")]
    [InlineData(" select * from dbo.Students ", "SELECT_dbo.Students")]
    public void GetSqlOperationAndTable_ParsesSupportedStatements(string sql, string expected)
    {
        var manager = new SqlManager("Server=.;Database=Dummy;Trusted_Connection=True;");

        var result = manager.GetSqlOperationAndTable(sql);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("CREATE TABLE Students(Id INT)")]
    [InlineData("EXEC SomeProcedure")]
    public void GetSqlOperationAndTable_WhenUnsupported_ReturnsNull(string sql)
    {
        var manager = new SqlManager("Server=.;Database=Dummy;Trusted_Connection=True;");

        var result = manager.GetSqlOperationAndTable(sql);

        Assert.Null(result);
    }
}
