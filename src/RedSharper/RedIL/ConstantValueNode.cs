using System;
using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class ConstantValueNode : RedILNode
    {
        public DataValueType DataType { get; }

        public object Value { get; }

        public ConstantValueNode(DataValueType type, object value)
            : base(RedILNodeType.Constant)
        {
            DataType = type;
            Value = value;
        }
    }
}