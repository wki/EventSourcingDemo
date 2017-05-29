using System;

namespace SandBox
{
    public class Program
    {
        static void Main(string[] args)
        {
            var x = new Product();
            var d = new Product.DoSomething();
            Console.WriteLine("Hello World!");
        }

        static (int x, int y) Point => (42, 43);
    }


    // a sample Aggregate root
    public class Product
    {
        public class Command { }

        public class DoSomething: Command { }


        public class Event { }

        public class SomethingDone : Event { }
    }
}