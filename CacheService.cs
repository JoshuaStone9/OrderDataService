using Microsoft.Data.Sqlite;
using System.Globalization;
public sealed class Cache
{
    internal Dictionary<int, List<Order>> OrdersByCustomerId { get; } = new();
    internal Dictionary<int, Customer> CustomersById { get; } = new();
    public Dictionary<int, List<OrderItem>> ItemsByOrderId { get; } = new();

    internal Dictionary<string, Customer> CustomersByEmail { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public void ClearAll()
    {
        CustomersById.Clear();
        OrdersByCustomerId.Clear();
        ItemsByOrderId.Clear();
        CustomersByEmail.Clear();
    }

    public void LoadFromDatabase(Repository repo)
    {
        ClearAll();

        var customers = repo.GetAllCustomers();
        foreach (var c in customers)
        {
            CustomersById[c.CustomerId] = c;
            CustomersByEmail[c.Email] = c;
        }

        var orders = repo.GetAllOrders();
        foreach (var o in orders)
        {
            if (!OrdersByCustomerId.ContainsKey(o.CustomerId))
                OrdersByCustomerId[o.CustomerId] = new List<Order>();

            OrdersByCustomerId[o.CustomerId].Add(o);
        }

        var items = repo.GetAllOrderItems();
        foreach (var i in items)
        {
            if (!ItemsByOrderId.ContainsKey(i.OrderId))
                ItemsByOrderId[i.OrderId] = new List<OrderItem>();

            ItemsByOrderId[i.OrderId].Add(i);
        }
    }



public static void PrintCacheCounts(Cache cache)
    {
        int customers = cache.CustomersById.Count;

        int orders = 0;
        foreach (var kv in cache.OrdersByCustomerId)
            orders += kv.Value.Count;

        int items = 0;
        foreach (var kv in cache.ItemsByOrderId)
            items += kv.Value.Count;

        Console.WriteLine($"Cache counts -> Customers: {customers}, Orders: {orders}, Items: {items}");
    }

    public static void AddCustomerFlow(Repository repo, Cache cache)
    {
        Console.Write("Name: ");
        var name = (Console.ReadLine() ?? "").Trim();

        Console.Write("Email: ");
        var email = (Console.ReadLine() ?? "").Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("Name and email are required.");
            return;
        }

        if (repo.CustomerExistsByEmail(email))
        {
            Console.WriteLine("A customer with that email already exists.");
            return;
        }

        int newId = repo.AddCustomer(name, email);

        bool existsInDb = repo.CustomerExistsById(newId);

        cache.CustomersById[newId] = new Customer { CustomerId = newId, Name = name, Email = email };
        cache.CustomersByEmail[email] = cache.CustomersById[newId];

        bool existsInCache = cache.CustomersById.ContainsKey(newId);

        Console.WriteLine($"Customer added. Id={newId}");
        Console.WriteLine($"Check -> existsInDb={existsInDb}, existsInCache={existsInCache}");
    }

    public static void AddOrderFlow(Repository repo, Cache cache)
    {
        Console.Write("CustomerId: ");
        if (!int.TryParse(Console.ReadLine(), out int customerId))
        {
            Console.WriteLine("Invalid CustomerId.");
            return;
        }

        if (!cache.CustomersById.ContainsKey(customerId))
        {
            Console.WriteLine("Customer not found in cache. Try refreshing cache.");
            return;
        }

        var orderDate = DateTime.Today;
        int orderId = repo.AddOrder(customerId, orderDate);

        bool orderExistsInDb = repo.OrderExists(orderId);

        var order = new Order { OrderId = orderId, CustomerId = customerId, OrderDate = orderDate };
        if (!cache.OrdersByCustomerId.ContainsKey(customerId))
            cache.OrdersByCustomerId[customerId] = new List<Order>();

        cache.OrdersByCustomerId[customerId].Add(order);

        bool orderExistsInCache = cache.OrdersByCustomerId[customerId].Exists(o => o.OrderId == orderId);

        Console.WriteLine($"Order created. OrderId={orderId} for CustomerId={customerId} on {orderDate:yyyy-MM-dd}");
        Console.WriteLine($"Check -> orderExistsInDb={orderExistsInDb}, orderExistsInCache={orderExistsInCache}");
    }

    public static void AddItemFlow(Repository repo, Cache cache)
    {
        Console.Write("OrderId: ");
        if (!int.TryParse(Console.ReadLine(), out int orderId))
        {
            Console.WriteLine("Invalid OrderId.");
            return;
        }

        if (!repo.OrderExists(orderId))
        {
            Console.WriteLine("Order not found in DB.");
            return;
        }

        Console.Write("Product name: ");
        var product = (Console.ReadLine() ?? "").Trim();

        Console.Write("Quantity: ");
        if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0)
        {
            Console.WriteLine("Invalid quantity.");
            return;
        }

        Console.Write("Price: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal price) || price < 0)
        {
            Console.WriteLine("Invalid price.");
            return;
        }

        int itemId = repo.AddOrderItem(orderId, product, qty, price);

        if (!cache.ItemsByOrderId.ContainsKey(orderId))
            cache.ItemsByOrderId[orderId] = new List<OrderItem>();

        cache.ItemsByOrderId[orderId].Add(new OrderItem
        {
            OrderItemId = itemId,
            OrderId = orderId,
            ProductName = product,
            Quantity = qty,
            Price = price
        });

        bool existsInCache = cache.ItemsByOrderId[orderId].Exists(i => i.OrderItemId == itemId);

        Console.WriteLine($"Item added. OrderItemId={itemId} -> {product} x{qty} @ {price}");
        Console.WriteLine($"Check -> existsInCache={existsInCache}");
    }

    public static void ViewCustomerOrdersFlow(Cache cache)
    {
        Console.Write("CustomerId: ");
        if (!int.TryParse(Console.ReadLine(), out int customerId))
        {
            Console.WriteLine("Invalid CustomerId.");
            return;
        }

        if (!cache.CustomersById.TryGetValue(customerId, out var customer))
        {
            Console.WriteLine("Customer not found in cache.");
            return;
        }

        Console.WriteLine($"Customer: {customer.Name} ({customer.Email})");

        if (!cache.OrdersByCustomerId.TryGetValue(customerId, out var orders) || orders.Count == 0)
        {
            Console.WriteLine("No orders found.");
            return;
        }

        orders.Sort((a, b) => a.OrderDate.CompareTo(b.OrderDate));

        foreach (var o in orders)
        {
            Console.WriteLine($"  OrderId={o.OrderId} Date={o.OrderDate:yyyy-MM-dd}");

            if (cache.ItemsByOrderId.TryGetValue(o.OrderId, out var items) && items.Count > 0)
            {
                foreach (var it in items)
                    Console.WriteLine($"    - {it.ProductName} Qty={it.Quantity} Price={it.Price}");
            }
            else
            {
                Console.WriteLine("    (no items yet)");
            }
        }
    }

    public static void FindOrdersAfterDateFlow(Cache cache)
    {
        Console.Write("Enter date (yyyy-MM-dd): ");
        var input = (Console.ReadLine() ?? "").Trim();

        if (!DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            Console.WriteLine("Invalid date format.");
            return;
        }

        var allOrders = new List<Order>();
        foreach (var kv in cache.OrdersByCustomerId)
            allOrders.AddRange(kv.Value);

        var filtered = allOrders.FindAll(o => o.OrderDate > date);

        bool foundAny = filtered.Count > 0;
        Console.WriteLine($"Orders after {date:yyyy-MM-dd}: {filtered.Count} (foundAny={foundAny})");

        foreach (var o in filtered)
            Console.WriteLine($"  OrderId={o.OrderId} CustomerId={o.CustomerId} Date={o.OrderDate:yyyy-MM-dd}");
    }

    public static void FindItemsQuantityGreaterThanTwoFlow(Cache cache)
    {
        var allItems = new List<OrderItem>();
        foreach (var kv in cache.ItemsByOrderId)
            allItems.AddRange(kv.Value);

        var filtered = allItems.FindAll(i => i.Quantity > 2);
        bool foundAny = filtered.Count > 0;

        Console.WriteLine($"Items with Quantity > 2: {filtered.Count} (foundAny={foundAny})");

        foreach (var it in filtered)
            Console.WriteLine($"  OrderId={it.OrderId} ItemId={it.OrderItemId} {it.ProductName} Qty={it.Quantity} Price={it.Price}");
    }

    public static void DeleteCustomerFlow(Repository repo, Cache cache)
    {
        Console.Write("CustomerId to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int customerId))
        {
            Console.WriteLine("Invalid CustomerId.");
            return;
        }

        bool deletedInDb = repo.DeleteCustomer(customerId);

        bool stillExistsInDb = repo.CustomerExistsById(customerId);

        bool removedFromCache = cache.CustomersById.Remove(customerId);

        string? emailKey = null;
        foreach (var kv in cache.CustomersByEmail)
        {
            if (kv.Value.CustomerId == customerId)
            {
                emailKey = kv.Key;
                break;
            }
        }
        if (emailKey != null)
            cache.CustomersByEmail.Remove(emailKey);

        if (cache.OrdersByCustomerId.TryGetValue(customerId, out var orders))
        {
            cache.OrdersByCustomerId.Remove(customerId);
            foreach (var o in orders)
                cache.ItemsByOrderId.Remove(o.OrderId);
        }

        bool removedFullyFromCache = !cache.CustomersById.ContainsKey(customerId);

        Console.WriteLine($"Delete attempted -> deletedInDb={deletedInDb}");
        Console.WriteLine($"Check -> stillExistsInDb={stillExistsInDb} (should be false)");
        Console.WriteLine($"Cache -> removedFromCache={removedFromCache}, removedFullyFromCache={removedFullyFromCache}");
    }

    public static void FindCustomerByEmailFlow(Cache cache)
    {
        Console.Write("Email: ");
        var email = (Console.ReadLine() ?? "").Trim();

        bool found = cache.CustomersByEmail.TryGetValue(email, out var customer);
        Console.WriteLine($"found={found}");

        if (found && customer != null)
            Console.WriteLine($"CustomerId={customer.CustomerId}, Name={customer.Name}, Email={customer.Email}");
    }
}
