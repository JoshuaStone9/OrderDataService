# OrderDataService
A small C# console app that manages customers, orders, and order items with SQLite and a dictionary-backed cache for fast lookups and demo-friendly flows.

## Table of Contents
- [Overview](#overview)
- [High Level Architecture](#high-level-architecture)
- [Data Model](#data-model)
- [Control Flow](#control-flow)
- [Randomization Logic](#randomization-logic)
- [Console Interface](#console-interface)
- [How to Run](#how-to-run)
- [Potential Improvements](#potential-improvements)
- [Personal Development](#personal-development)

---

## Overview
This project is a console-based data service that models customers and their orders. It writes to SQLite, then loads that data into in-memory dictionaries for quick lookups and practice with collections.

You can:

- Add customers, orders, and order items
- View a customer's orders and items (dictionary lookup)
- Find orders after a date (lambda + FindAll)
- Find items with Quantity > 2 (lambda + FindAll)
- Delete a customer and verify cascading deletes
- Find a customer by email (case-insensitive dictionary)

Data is stored in a local SQLite file (`customer_orders.db`) in the project folder, so records persist between runs.

---

## High-Level Architecture
The app is organized into a few small layers:

Presentation / UI Layer (Console)
Menu prompts and user interaction live in `Program`.

Domain Logic (Customers, Orders, Items)
Flows for add/view/find/delete are in `Cache` and operate on in-memory dictionaries.

Persistence Layer (SQLite)
`Repository` handles SQL commands and data mapping.

The cache holds the current snapshot of data in dictionaries for fast access.

---

## Data Model
The app uses a mix of SQLite tables and in-memory dictionaries:

SQLite Tables
- Customers (CustomerId, Name, Email)
- Orders (OrderId, CustomerId, OrderDate)
- OrderItems (OrderItemId, OrderId, ProductName, Quantity, Price)

In-Memory Cache
- `CustomersById: Dictionary<int, Customer>`
- `CustomersByEmail: Dictionary<string, Customer>` (case-insensitive)
- `OrdersByCustomerId: Dictionary<int, List<Order>>`
- `ItemsByOrderId: Dictionary<int, List<OrderItem>>`

Example:

OrdersByCustomerId[42] -> list of all orders for customer 42.

---

## Control Flow
Program Startup
- Creates the SQLite tables if needed with `Db.EnsureCreated()`.
- Initializes `Repository` and `Cache`.
- Loads all data into dictionaries with `cache.LoadFromDatabase(repo)`.
- Enters the main menu loop.

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

---

## Database Integration (SQLite)
SQLite is accessed through `Microsoft.Data.Sqlite`.

`Db.EnsureCreated()`:
- Enables foreign keys
- Creates Customers, Orders, and OrderItems tables
- Enforces cascading deletes on Orders and OrderItems

`Repository`:
- Inserts and queries rows
- Checks for existence by id/email
- Deletes customers and relies on cascade behavior for related data

---

## Local Persistence (customer_orders.db)
All data is stored in `customer_orders.db` in the project folder. The file is created on first run and reused across runs.

---

## Configuration & Environment
No environment variables are required. If you want to rename or move the database file, update the connection string in `Db.ConnectionString`.

---

## How to Run
```bash
dotnet restore
dotnet run
```
---

## Potential Improvements
- Add update flows (edit customers, orders, or items)
- Use async database calls
- Add validation and better error messaging
- Introduce reporting (totals by customer, top items, etc.)

---

## Personal Development

This project was created as part of my ongoing personal development in C# and software development. As I continue to learn more about C# through hands on projects and online learning resources I apply that knowledge directly to my work.

The project focuses on core concepts such as CRUD operations SQL backed data models and in memory caching using dictionaries within a console based C# application. By building real features that create read update and delete data I have strengthened my understanding of how C# works with relational databases and how data can be managed efficiently in memory.

During development I used key C# and .NET features including collections lambda expressions predicates and clear separation of responsibilities. This project shows my ability to take what I learn through continuous study and apply it in practical ways while improving my technical skills and problem solving ability.

As I continue learning and building more projects this application serves as a strong foundation that I can improve and expand over time.