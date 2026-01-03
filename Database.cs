using Microsoft.Data.Sqlite;
using System.Globalization;
internal static class Db
{
    public static string ConnectionString => "Data Source=customer_orders.db";

    public static void EnsureCreated()
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Customers (
    CustomerId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Email TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS Orders (
    OrderId INTEGER PRIMARY KEY AUTOINCREMENT,
    CustomerId INTEGER NOT NULL,
    OrderDate TEXT NOT NULL,
    FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS OrderItems (
    OrderItemId INTEGER PRIMARY KEY AUTOINCREMENT,
    OrderId INTEGER NOT NULL,
    ProductName TEXT NOT NULL,
    Quantity INTEGER NOT NULL,
    Price REAL NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE
);
";
        cmd.ExecuteNonQuery();
    }
}

public sealed class Repository
{
    private readonly string _cs;

    public Repository(string connectionString)
    {
        _cs = connectionString;
    }

    private SqliteConnection Open()
    {
        var conn = new SqliteConnection(_cs);
        conn.Open();
        using var pragma = conn.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();
        return conn;
    }


    public int AddCustomer(string name, string email)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO Customers (Name, Email)
VALUES ($name, $email);
SELECT last_insert_rowid();
";
        cmd.Parameters.AddWithValue("$name", name);
        cmd.Parameters.AddWithValue("$email", email);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool CustomerExistsById(int customerId)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM Customers WHERE CustomerId = $id LIMIT 1;";
        cmd.Parameters.AddWithValue("$id", customerId);
        return cmd.ExecuteScalar() != null;
    }

    public bool CustomerExistsByEmail(string email)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM Customers WHERE lower(Email) = lower($email) LIMIT 1;";
        cmd.Parameters.AddWithValue("$email", email);
        return cmd.ExecuteScalar() != null;
    }

    public bool DeleteCustomer(int customerId)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Customers WHERE CustomerId = $id;";
        cmd.Parameters.AddWithValue("$id", customerId);
        return cmd.ExecuteNonQuery() > 0;
    }

    public int AddOrder(int customerId, DateTime orderDate)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO Orders (CustomerId, OrderDate)
VALUES ($customerId, $orderDate);
SELECT last_insert_rowid();
";
        cmd.Parameters.AddWithValue("$customerId", customerId);
        cmd.Parameters.AddWithValue("$orderDate", orderDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool OrderExists(int orderId)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM Orders WHERE OrderId = $id LIMIT 1;";
        cmd.Parameters.AddWithValue("$id", orderId);
        return cmd.ExecuteScalar() != null;
    }

    public int AddOrderItem(int orderId, string productName, int quantity, decimal price)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO OrderItems (OrderId, ProductName, Quantity, Price)
VALUES ($orderId, $productName, $quantity, $price);
SELECT last_insert_rowid();
";
        cmd.Parameters.AddWithValue("$orderId", orderId);
        cmd.Parameters.AddWithValue("$productName", productName);
        cmd.Parameters.AddWithValue("$quantity", quantity);
        cmd.Parameters.AddWithValue("$price", price);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    internal List<Customer> GetAllCustomers()
    {
        var result = new List<Customer>();
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT CustomerId, Name, Email FROM Customers;";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Customer
            {
                CustomerId = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2)
            });
        }
        return result;
    }

    internal List<Order> GetAllOrders()
    {
        var result = new List<Order>();
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT OrderId, CustomerId, OrderDate FROM Orders;";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Order
            {
                OrderId = reader.GetInt32(0),
                CustomerId = reader.GetInt32(1),
                OrderDate = DateTime.ParseExact(reader.GetString(2), "yyyy-MM-dd", CultureInfo.InvariantCulture)
            });
        }
        return result;
    }

    public List<OrderItem> GetAllOrderItems()
    {
        var result = new List<OrderItem>();
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT OrderItemId, OrderId, ProductName, Quantity, Price FROM OrderItems;";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new OrderItem
            {
                OrderItemId = reader.GetInt32(0),
                OrderId = reader.GetInt32(1),
                ProductName = reader.GetString(2),
                Quantity = reader.GetInt32(3),
                Price = Convert.ToDecimal(reader.GetDouble(4))
            });
        }
        return result;
    }
}