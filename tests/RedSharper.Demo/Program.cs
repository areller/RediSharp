using RedSharper.Contracts;
using RedSharper.Contracts.Extensions;

namespace RedSharper.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client(null);
            client.Execute<(int a, int b), RedResult>((cursor, keys, argv) =>
            {
                string resStr = cursor.Get("myKey");
                var res = cursor.Get("myNum").AsInt();
                res += 1;
                
                if (res > 10)
                {
                    return cursor.Set("myNum", 0);
                }
                else
                {
                    return cursor.Set("myNum", res);
                }
            }, (1, 2)).Wait();
        }
    }
}