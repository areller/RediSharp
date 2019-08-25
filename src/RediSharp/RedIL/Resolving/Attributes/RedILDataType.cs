using System;
using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Resolving.Attributes
 {
     [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
     class RedILDataType : Attribute
     {
         public DataValueType Type { get; }

         public RedILDataType(DataValueType type)
         {
             Type = type;
         }
     }
 }