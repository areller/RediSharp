using RedSharper.RedIL.Attributes;

namespace RedSharper.Contracts.Extensions
{
    public static class RedSingleResultExtensions
    {
        [RedILResolve(typeof(SingleResultAsIntResolver))]
        public static int? AsInt(this RedSingleResult result) => result.ConvertToInt();

        [RedILResolve(typeof(SingleResultAsLongResolver))]
        public static long? AsLong(this RedSingleResult result) => result.ConvertToLong();

        [RedILResolve(typeof(SingleResultAsDoubleResolver))]
        public static double? AsDouble(this RedSingleResult result) => result.ConvertToDouble();
    }
}