using SlqStudio.Application.SQL.Utils;
using SlqStudio.Application.SQL.Utils.Enum;

namespace SlqStudio.Tests;

public sealed class SqlCommandTypeDetectorTests
{
    [Theory]
    [InlineData("SELECT * FROM Students", SqlCommandType.Select)]
    [InlineData("  select 1", SqlCommandType.Select)]
    [InlineData("INSERT INTO Students VALUES (1)", SqlCommandType.Insert)]
    [InlineData("UPDATE Students SET Name = 'A'", SqlCommandType.Update)]
    [InlineData("DELETE FROM Students", SqlCommandType.Delete)]
    [InlineData("EXEC GetStudents", SqlCommandType.Execute)]
    [InlineData("EXECUTE GetStudents", SqlCommandType.Execute)]
    [InlineData("CREATE PROCEDURE p AS SELECT 1", SqlCommandType.CreateProcedure)]
    [InlineData("ALTER PROCEDURE p AS SELECT 1", SqlCommandType.AlterProcedure)]
    [InlineData("CREATE FUNCTION f() RETURNS INT AS BEGIN RETURN 1 END", SqlCommandType.CreateFunction)]
    [InlineData("ALTER FUNCTION f() RETURNS INT AS BEGIN RETURN 1 END", SqlCommandType.AlterFunction)]
    [InlineData("CREATE TABLE T (Id INT)", SqlCommandType.CreateTable)]
    [InlineData("ALTER TABLE T ADD Name NVARCHAR(50)", SqlCommandType.AlterTable)]
    [InlineData("DROP TABLE T", SqlCommandType.Drop)]
    public void Detect_RecognizesBasicStatements(string sql, SqlCommandType expected)
    {
        Assert.Equal(expected, SqlCommandTypeDetector.Detect(sql));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Detect_WhenNullOrWhitespace_ReturnsUnknown(string? sql)
    {
        Assert.Equal(SqlCommandType.Unknown, SqlCommandTypeDetector.Detect(sql!));
    }

    [Theory]
    [InlineData("MERGE INTO T USING S ON ...")]
    [InlineData("WITH cte AS (SELECT 1) SELECT * FROM cte")]
    [InlineData("GRANT SELECT ON T TO user")]
    public void Detect_WhenUnsupported_ReturnsUnknown(string sql)
    {
        Assert.Equal(SqlCommandType.Unknown, SqlCommandTypeDetector.Detect(sql));
    }

    [Fact]
    public void Detect_DmlTriggerAfter()
    {
        const string sql = "CREATE TRIGGER trg ON Students AFTER INSERT AS BEGIN SELECT 1 END";

        Assert.Equal(SqlCommandType.DmlTriggerAfter, SqlCommandTypeDetector.Detect(sql));
    }

    [Fact]
    public void Detect_DmlTriggerInsteadOf()
    {
        const string sql = "CREATE TRIGGER trg ON Students INSTEAD OF DELETE AS BEGIN SELECT 1 END";

        Assert.Equal(SqlCommandType.DmlTriggerInsteadOf, SqlCommandTypeDetector.Detect(sql));
    }

    [Fact]
    public void Detect_DmlTriggerWithoutTiming()
    {
        const string sql = "CREATE TRIGGER trg ON Students FOR INSERT AS BEGIN SELECT 1 END";

        Assert.Equal(SqlCommandType.DmlTrigger, SqlCommandTypeDetector.Detect(sql));
    }

    [Theory]
    [InlineData("CREATE TRIGGER trg ON DATABASE FOR DROP_TABLE AS BEGIN SELECT 1 END")]
    [InlineData("CREATE TRIGGER trg ON ALL SERVER FOR CREATE_DATABASE AS BEGIN SELECT 1 END")]
    public void Detect_DdlTrigger(string sql)
    {
        Assert.Equal(SqlCommandType.DdlTrigger, SqlCommandTypeDetector.Detect(sql));
    }

    [Fact]
    public void Detect_IsCaseInsensitiveAndIgnoresLeadingWhitespace()
    {
        const string sql = "\n\t  create trigger trg on Students after update as begin select 1 end";

        Assert.Equal(SqlCommandType.DmlTriggerAfter, SqlCommandTypeDetector.Detect(sql));
    }
}
