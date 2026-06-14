namespace SlqStudio.Application.SQL.Utils.Enum;

public enum SqlCommandType
{
    Unknown,
    Select,
    Insert,
    Update,
    Delete,
    Execute,
    
    CreateProcedure,
    AlterProcedure,
    CreateFunction,
    AlterFunction,
    
    CreateTable,
    AlterTable,
    Drop,

    DmlTrigger,
    DmlTriggerAfter,
    DmlTriggerInsteadOf,
    DdlTrigger
}
