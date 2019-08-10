using RedSharper.Contracts;
using RedSharper.Contracts.Extensions;
using RedSharper.Extensions;

namespace RedSharper.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client(null);
            /*
            client.Execute<(int a, int b), RedResult>((cursor, keys, argv) =>
            {
                string nameA = "Arik", nameB = "Lekar";
                (string name, int age) = ("Arik", 23);

                string resStr = cursor.Get("myKey");
                var res = cursor.Get("myNum").AsInt();

                while (res < 10)
                {
                    res += 1;
                    res += int.Parse("2");
                }

                var lng = (long)res;

                var y = 6;
                y = 7;

                y = res > 3 ? res++ : 5;
                
                if (res > 10)
                {
                    return cursor.Set("myNum", y);
                }
                else
                {
                    return cursor.Set("myNum", res);
                }
            }, (1, 2)).Wait();*/

            client.Execute<(int a, int b), RedResult>((cursor, keys, argv) =>
            {
                var indices = new int[5];
                for (int i = 0; i < 5; i++)
                {
                    indices[i] = i + 1;
                }
                
                foreach (var index in indices)
                {
                    cursor.Set("key_" + index, index + 1);
                }

                return RedResult.Ok;
            }, (1, 2)).Wait();
        }
    }
}