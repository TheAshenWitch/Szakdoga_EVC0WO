using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using Szakdoga.Models;

public class DatabaseService : IDisposable
{
    private readonly AppDbContext _db;

    public DatabaseService()
    {
        _db = new AppDbContext();
    }

    // =====================
    // SHEETS
    // =====================

    public void AddSheet(Sheet sheet)
    {
        _db.Sheets.Add(sheet);
        _db.SaveChanges();
    }

    public void AddSheets(IEnumerable<Sheet> sheets)
    {
        _db.Sheets.AddRange(sheets);
        _db.SaveChanges();
    }

    public List<Sheet> GetAllSheets()
    {
        return _db.Sheets
            
            .ToList();
    }

    public Sheet? GetSheetById(int id)
    {
        return _db.Sheets
            
            .FirstOrDefault(s => s.Id == id);
    }
    public Sheet? GetSheetByName(string name)
    {
        return _db.Sheets
            
            .FirstOrDefault(s => s.Name == name);
    }
    public List<Sheet> GetSheetsByName(string text)
    {
        return _db.Sheets
            
            .Where(s => s.Name.Contains(text))
            .ToList();
    }
    public void UpdateSheet(Sheet sheet)
    {
        _db.Sheets.Update(sheet);
        _db.SaveChanges();
    }

    public void DeleteSheet(Sheet sheet)
    {
        _db.Sheets.Remove(sheet);
        _db.SaveChanges();
    }


    // =====================
    // CUSTOMERS
    // =====================

    public void AddCustomer(Customer customer)
    {
        _db.Customers.Add(customer);
        _db.SaveChanges();
    }

    public List<Customer> GetAllCustomers()
    {
        return _db.Customers
            //.Include(c => c.Orders)
            //    .ThenInclude(o => o.Sheet)
            //.Include(c => c.Orders)
            //    .ThenInclude(o => o.Pieces)
            .ToList();
    }

    public Customer? GetCustomerById(int id)
    {
        return _db.Customers
            
            .Include(c => c.Orders)
                .ThenInclude(o => o.Sheet)
            .Include(c => c.Orders)
                .ThenInclude(o => o.Pieces)
            .FirstOrDefault(c => c.Id == id);
    }

    public Customer? GetCustomerByName(string name)
    {
        return _db.Customers
            
            .Include(c => c.Orders)
                .ThenInclude(o => o.Sheet)
            .Include(c => c.Orders)
                .ThenInclude(o => o.Pieces)
            .FirstOrDefault(c => c.Name == name);
    }
    public List<Customer> GetCustomersByName(string text)
    {
        return _db.Customers
            
            .Include(c => c.Orders)
                .ThenInclude(o => o.Sheet)
            .Include(c => c.Orders)
                .ThenInclude(o => o.Pieces)
            .Where(c => c.Name.Contains(text))
            .ToList();
    }
    public void UpdateCustomer(Customer customer)
    {
        _db.Customers.Update(customer);
        _db.SaveChanges();
    }

    public void DeleteCustomer(Customer customer)
    {
        _db.Customers.Remove(customer);
        _db.SaveChanges();
    }


    // =====================
    // ORDERS
    // =====================

    public void AddOrder(Order order)
    {
        _db.Orders.Add(order);
        _db.SaveChanges();
    }

    public List<Order> GetAllOrders()
    {
        return _db.Orders
            
            .Include(o => o.Customer)
            .Include(o => o.Sheet)
            .Include(o => o.Pieces)
                .ThenInclude(p => p.Sheet)
            .ToList();
    }

    public Order? GetOrderById(int id)
    {
        return _db.Orders
            
            .Include(o => o.Customer)
            .Include(o => o.Sheet)
            .Include(o => o.Pieces)
                .ThenInclude(p => p.Sheet)
            .FirstOrDefault(o => o.Id == id);
    }

    public void UpdateOrder(Order order)
    {
        _db.Orders.Update(order);
        _db.SaveChanges();
    }

    public void DeleteOrder(Order order)
    {
        _db.Orders.Remove(order);
        _db.SaveChanges();
    }


    // =====================
    // ORDER PIECES
    // =====================

    public void AddOrderPiece(OrderPiece orderPiece)
    {
        _db.OrderPieces.Add(orderPiece);
        _db.SaveChanges();
    }

    public void AddOrderPieces(IEnumerable<OrderPiece> orderPieces)
    {
        _db.OrderPieces.AddRange(orderPieces);
        _db.SaveChanges();
    }

    public List<OrderPiece> GetAllOrderPieces()
    {
        return _db.OrderPieces
            
            .Include(op => op.Order)
                .ThenInclude(o => o.Customer)
            .Include(op => op.Sheet)
            .ToList();
    }
    public List<OrderPiece> GetOrderPiecesByOrderId(int orderId)
    {
        return _db.OrderPieces
            
            .Include(op => op.Order)
                .ThenInclude(o => o.Customer)
            .Include(op => op.Sheet)
            .Where(op => op.OrderId == orderId)
            .ToList();
    }

    public OrderPiece? GetOrderPieceById(int id)
    {
        return _db.OrderPieces
            
            .Include(op => op.Order)
                .ThenInclude(o => o.Customer)
            .Include(op => op.Sheet)
            .FirstOrDefault(op => op.Id == id);
    }

    public void UpdateOrderPiece(OrderPiece orderPiece)
    {
        _db.OrderPieces.Update(orderPiece);
        _db.SaveChanges();
    }

    public void DeleteOrderPiece(OrderPiece orderPiece)
    {
        _db.OrderPieces.Remove(orderPiece);
        _db.SaveChanges();
    }


    // =====================
    // INVENTORY
    // =====================

    public void AddInventoryItem(InventoryItem inventoryItem)
    {
        _db.InventoryItems.Add(inventoryItem);
        _db.SaveChanges();
    }

    public void AddInventoryItems(IEnumerable<InventoryItem> inventoryItems)
    {
        _db.InventoryItems.AddRange(inventoryItems);
        _db.SaveChanges();
    }

    public List<InventoryItem> GetAllInventoryItems()
    {
        return _db.InventoryItems
            
            .Include(ii => ii.Sheet)
            .ToList();
    }

    public InventoryItem? GetInventoryItemById(int id)
    {
        return _db.InventoryItems
            
            .Include(ii => ii.Sheet)
            .FirstOrDefault(ii => ii.Id == id);
    }

    public void UpdateInventoryItem(InventoryItem inventoryItem)
    {
        _db.InventoryItems.Update(inventoryItem);
        _db.SaveChanges();
    }

    public void DeleteInventoryItem(InventoryItem inventoryItem)
    {
        _db.InventoryItems.Remove(inventoryItem);
        _db.SaveChanges();
    }


    // =====================
    // SAVE
    // =====================

    public void SaveAllChanges()
    {
        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db?.Dispose();
    }
}