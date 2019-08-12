using RedSharper.RedIL.Enums;
using System;
using ICSharpCode.Decompiler.TypeSystem;

namespace RedSharper.RedIL.Utilities
{
    static class TypeUtilities
    {
        public static bool IsIntegerType(object obj)
            => IsIntegerType(obj.GetType());

        public static bool IsIntegerType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsIntegerType(KnownTypeCode kTypeCode)
        {
            switch (kTypeCode)
            {
                case KnownTypeCode.Int16:
                case KnownTypeCode.Byte:
                case KnownTypeCode.SByte:
                case KnownTypeCode.UInt16:
                case KnownTypeCode.Int32:
                case KnownTypeCode.UInt32:
                case KnownTypeCode.Int64:
                case KnownTypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsFloatType(object obj)
            => IsFloatType(obj.GetType());

        public static bool IsFloatType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Single:
                case TypeCode.Decimal:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsFloatType(KnownTypeCode kTypeCode)
        {
            switch (kTypeCode)
            {
                case KnownTypeCode.Single:
                case KnownTypeCode.Decimal:
                case KnownTypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsBooleanType(object obj)
            => IsBooleanType(obj.GetType());

        public static bool IsBooleanType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsBooleanType(KnownTypeCode kTypeCode)
        {
            switch (kTypeCode)
            {
                case KnownTypeCode.Boolean:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsStringType(object obj)
            => IsStringType(obj.GetType());

        public static bool IsStringType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.String:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsStringType(KnownTypeCode kTypeCode)
        {
            switch (kTypeCode)
            {
                case KnownTypeCode.String:
                    return true;
                default:
                    return false;
            }
        }

        public static DataValueType GetValueType(object obj)
            => GetValueType(obj.GetType());

        public static DataValueType GetValueType(Type type)
        {
            type = GetNullableUnderlyingType(type);
            
            if (IsIntegerType(type))
                return DataValueType.Integer;
            else if (IsFloatType(type))
                return DataValueType.Float;
            else if (IsBooleanType(type))
                return DataValueType.Boolean;
            else if (IsStringType(type))
                return DataValueType.String;

            return DataValueType.Unknown;
        }

        public static DataValueType GetValueType(KnownTypeCode kTypeCode)
        {
            if (IsIntegerType(kTypeCode))
                return DataValueType.Integer;
            else if (IsFloatType(kTypeCode))
                return DataValueType.Float;
            else if (IsBooleanType(kTypeCode))
                return DataValueType.Boolean;
            else if (IsStringType(kTypeCode))
                return DataValueType.String;

            return DataValueType.Unknown;
        }

        private static Type GetNullableUnderlyingType(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying is null)
            {
                return type;
            }

            return underlying;
        }
    }
}