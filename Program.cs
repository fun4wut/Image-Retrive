using System;

namespace img_search
{
    class Program
    {
        static void Swap(ref int a, ref int b)
        {
            int tmp = a;
            a = b;
            b = tmp;
        }
        static void Main(string[] args)
        {
            int a = 2, b = 3;
            Swap(ref a, ref b);
            Console.WriteLine(a - b);
        }
    }
}
