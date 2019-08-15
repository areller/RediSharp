namespace RediSharp.RedIL.Enums
{
    public enum RedILNodeType
    {
        Empty,
        VariableDeclaration,
        Assign,
        BinaryExpression,
        UniformExpression,
        UnaryExpression,
        Block,
        Constant,
        CallRedisMethod,
        Parameter,
        TableKeyAccess,
        If,
        Return,
        ArgsTable,
        KeysTable,
        Break,
        Nil,
        Cast,
        Conditional,
        DoWhile,
        While,
        Status,
        CallLuaMethod,
        Continue,
        Cursor,
        ArrayTableDefinition,
        IteratorLoop
    }
}