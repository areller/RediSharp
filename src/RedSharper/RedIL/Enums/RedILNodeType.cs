namespace RedSharper.RedIL.Enums
{
    public enum RedILNodeType
    {
        Empty,
        VariableDeclaration,
        Assign,
        BinaryExpression,
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
        While
    }
}