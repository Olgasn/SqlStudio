using SlqStudio.Application.SQL.Utils.Enum;

namespace SlqStudio.Application.SQL.Utils;

public static class SqlCommandTypeDetector
{
    public static SqlCommandType Detect(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return SqlCommandType.Unknown;

        var trimmed = sql.TrimStart().ToUpperInvariant();
        var firstLine = trimmed.Split('\n', '\r').FirstOrDefault()?.Trim() ?? "";
        var firstWords = firstLine.Split(new[] { ' ', '\t', '(' }, StringSplitOptions.RemoveEmptyEntries);

        if (firstWords.Length == 0)
            return SqlCommandType.Unknown;

        if (trimmed.StartsWith("SELECT")) return SqlCommandType.Select;
        if (trimmed.StartsWith("INSERT")) return SqlCommandType.Insert;
        if (trimmed.StartsWith("UPDATE")) return SqlCommandType.Update;
        if (trimmed.StartsWith("DELETE")) return SqlCommandType.Delete;

        if (trimmed.StartsWith("EXEC") || trimmed.StartsWith("EXECUTE"))
            return SqlCommandType.Execute;

        if (trimmed.StartsWith("CREATE PROCEDURE"))
            return SqlCommandType.CreateProcedure;
        if (trimmed.StartsWith("ALTER PROCEDURE"))
            return SqlCommandType.AlterProcedure;

        if (trimmed.StartsWith("CREATE FUNCTION"))
            return SqlCommandType.CreateFunction;
        if (trimmed.StartsWith("ALTER FUNCTION"))
            return SqlCommandType.AlterFunction;

        if (trimmed.StartsWith("CREATE TABLE"))
            return SqlCommandType.CreateTable;
        if (trimmed.StartsWith("ALTER TABLE"))
            return SqlCommandType.AlterTable;

        if (trimmed.StartsWith("DROP")) return SqlCommandType.Drop;
        
        if (trimmed.StartsWith("CREATE TRIGGER") &&
            (trimmed.Contains("ON DATABASE") ||
             trimmed.Contains("ON ALL SERVER") ||
             trimmed.Contains("FOR CREATE") ||
             trimmed.Contains("FOR ALTER") ||
             trimmed.Contains("FOR DROP")))
        {
            return SqlCommandType.DdlTrigger;
        }

        if (trimmed.StartsWith("CREATE TRIGGER"))
        {
            if (trimmed.Contains("INSTEAD OF"))
                return SqlCommandType.DmlTriggerInsteadOf;

            if (trimmed.Contains("AFTER"))
                return SqlCommandType.DmlTriggerAfter;

            return SqlCommandType.DmlTrigger;
        }

        return SqlCommandType.Unknown;
    }
}
