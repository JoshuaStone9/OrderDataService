using System;
using System.Collections.Generic;
using System.Globalization;


public class Program
{
    public static void Main()
    {
        Db.EnsureCreated();

        var repo = new Repository(Db.ConnectionString);
        var cache = new Cache();

        cache.LoadFromDatabase(repo);

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Customer Orders Lookup ===");
            Console.WriteLine("1) Refresh cache (reload dictionaries from DB)");
            Console.WriteLine("2) Add customer");
            Console.WriteLine("3) Create order for customer");
            Console.WriteLine("4) Add item to order");
            Console.WriteLine("5) View customer orders (dictionary lookup)");
            Console.WriteLine("6) Find all orders after a date (lambda + FindAll)");
            Console.WriteLine("7) Find all items with Quantity > 2 (lambda + FindAll)");
            Console.WriteLine("8) Delete customer (verify removed from DB + cache)");
            Console.WriteLine("9) Find customer by email (dictionary, case-insensitive)");
            Console.WriteLine("0) Exit");
            Console.Write("Choose: ");

            var choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    cache.LoadFromDatabase(repo);
                    Console.WriteLine("Cache refreshed.");
                    Cache.PrintCacheCounts(cache);
                    break;

                case "2":
                    Cache.AddCustomerFlow(repo, cache);
                    break;

                case "3":
                    Cache.AddOrderFlow(repo, cache);
                    break;

                case "4":
                    Cache.AddItemFlow(repo, cache);
                    break;

                case "5":
                    Cache.ViewCustomerOrdersFlow(cache);
                    break;

                case "6":
                    Cache.FindOrdersAfterDateFlow(cache);
                    break;

                case "7":
                    Cache.FindItemsQuantityGreaterThanTwoFlow(cache);
                    break;

                case "8":
                    Cache.DeleteCustomerFlow(repo, cache);
                    break;

                case "9":
                    Cache.FindCustomerByEmailFlow(cache);
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Unknown option.");
                    break;
            }
        }
    }

}
