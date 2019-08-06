namespace RedSharper.Contracts.Extensions
{
    public static class RedSingleResultExtensions
    {
        public static int AsInt(this RedSingleResult result) => result.ConvertToInt();

        public static long AsLong(this RedSingleResult result) => result.ConvertToLong();

        public static double AsDouble(this RedSingleResult result) => result.ConvertToDouble();
    }
}