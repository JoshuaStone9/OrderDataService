# OrderDataService
A small C# console app that manages customers, orders, and order items with SQLite and a dictionary-backed cache for fast lookups and demo-friendly flows.

Table of Contents
Overview
High-Level Architecture
Data Model
Control Flow
Database Integration (SQLite)
Local Persistence (customer_orders.db)
Configuration & Environment
How to Run
Potential Improvements

Overview
This project is a console-based data service that models customers and their orders. It writes to SQLite, then loads that data into in-memory dictionaries for quick lookups and practice with collections.

You can:

View current customers, orders, and items via lookups
Add a customer
Create an order for a customer
Add an item to an order
Find orders after a date (lambda + FindAll)
Find items with Quantity > 2 (lambda + FindAll)
Delete a customer and verify cascading deletes
Find a customer by email (case-insensitive dictionary)

Data is stored in a local SQLite file (customer_orders.db) in the project folder, so records persist between runs.

High-Level Architecture
The app is organized into a few small layers:

Presentation / UI Layer (Console)
Menu prompts and user interaction live in Program.

Domain Logic (Customers, Orders, Items)
Flows for add/view/find/delete are in Cache and operate on in-memory dictionaries.

Persistence Layer (SQLite)
Repository handles SQL commands and data mapping.

The cache holds the current snapshot of data in dictionaries for fast access.

Data Model
The app uses a mix of SQLite tables and in-memory dictionaries:

SQLite Tables
Customers (CustomerId, Name, Email)
Orders (OrderId, CustomerId, OrderDate)
OrderItems (OrderItemId, OrderId, ProductName, Quantity, Price)

In-Memory Cache
CustomersById: Dictionary<int, Customer>
CustomersByEmail: Dictionary<string, Customer> (case-insensitive)
OrdersByCustomerId: Dictionary<int, List<Order>>
ItemsByOrderId: Dictionary<int, List<OrderItem>>

Example:

OrdersByCustomerId[42] -> list of all orders for customer 42.

Control Flow
Program Startup
Creates the SQLite tables if needed with Db.EnsureCreated().
Initializes Repository and Cache.
Loads all data into dictionaries with cache.LoadFromDatabase(repo).
Enters the main menu loop.

Menu Options
1. Refresh cache (reload dictionaries from DB)
2. Add customer
3. Create order for customer
4. Add item to order
5. View customer orders (dictionary lookup)
6. Find all orders after a date (lambda + FindAll)
7. Find all items with Quantity > 2 (lambda + FindAll)
8. Delete customer (verify removed from DB + cache)
9. Find customer by email (dictionary, case-insensitive)
0. Exit

Database Integration (SQLite)
SQLite is accessed through Microsoft.Data.Sqlite.

Db.EnsureCreated()
Enables foreign keys
Creates Customers, Orders, and OrderItems tables
Enforces cascading deletes on Orders and OrderItems

Repository
Inserts and queries rows
Checks for existence by id/email
Deletes customers and relies on cascade behavior for related data

Local Persistence (customer_orders.db)
All data is stored in customer_orders.db in the project folder. The file is created on first run and reused across runs.

Configuration & Environment
No environment variables are required. If you want to rename or move the database file, update the connection string in Db.ConnectionString.

How to Run
dotnet restore
dotnet run

Potential Improvements
Use async database calls
Add update flows (edit customers, orders, or items)
Add validation and better error messaging
Introduce reporting (totals by customer, top items, etc.)
