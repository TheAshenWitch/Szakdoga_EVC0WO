using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using Szakdoga.Models;
using System.Windows;

public class DatabaseService : IDisposable
{
    private readonly AppDbContext _db;

    public DatabaseService()
    {
        try
        {
            _db = new AppDbContext();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to initialize database connection: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    // =====================
    // SHEETS
    // =====================

    public void AddSheet(Sheet sheet)
    {
        try
        {
            if (sheet == null)
                throw new ArgumentNullException(nameof(sheet));
            
            _db.Sheets.Add(sheet);
            _db.SaveChanges();

            // Create corresponding InventoryItem for the new sheet
            InventoryItem inventoryItem = new InventoryItem
            {
                SheetId = sheet.Id,
                TotalQuantity = 0,
                ReservedQuantity = 0
            };
            _db.InventoryItems.Add(inventoryItem);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to add sheet: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error adding sheet: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void AddSheets(IEnumerable<Sheet> sheets)
    {
        try
        {
            if (sheets == null)
                throw new ArgumentNullException(nameof(sheets));
            
            _db.Sheets.AddRange(sheets);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to add sheets: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error adding sheets: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public List<Sheet> GetAllSheets()
    {
        try
        {
            return _db.Sheets.ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to retrieve sheets: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return new List<Sheet>();
        }
    }

    public Sheet? GetSheetById(int id)
    {
        try
        {
            return _db.Sheets.FirstOrDefault(s => s.Id == id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to retrieve sheet: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }

    public Sheet? GetSheetByName(string name)
    {
        try
        {
            if (string.IsNullOrEmpty(name))
                return null;
            
            return _db.Sheets.FirstOrDefault(s => s.Name == name);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to retrieve sheet by name: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }

    public List<Sheet> GetSheetsByName(string text)
    {
        try
        {
            if (string.IsNullOrEmpty(text))
                return new List<Sheet>();
            
            return _db.Sheets.Where(s => s.Name.Contains(text)).ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to search sheets: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return new List<Sheet>();
        }
    }

    public void UpdateSheet(Sheet sheet)
    {
        try
        {
            if (sheet == null)
                throw new ArgumentNullException(nameof(sheet));
            
            _db.Sheets.Update(sheet);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to update sheet: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error updating sheet: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void DeleteSheet(Sheet sheet)
    {
        try
        {
            if (sheet == null)
                throw new ArgumentNullException(nameof(sheet));
            
            // Delete the corresponding InventoryItem first (without deleting the Sheet reference in Orders)
            var inventoryItem = _db.InventoryItems.FirstOrDefault(ii => ii.SheetId == sheet.Id);
            if (inventoryItem != null)
            {
                _db.InventoryItems.Remove(inventoryItem);
                _db.SaveChanges();
            }

            // Delete the Sheet (Orders will keep their SheetId reference, even if it becomes orphaned)
            _db.Sheets.Remove(sheet);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to delete sheet: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error deleting sheet: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    // =====================
    // CUSTOMERS
    // =====================

    public void AddCustomer(Customer customer)
    {
        try
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));
            
            _db.Customers.Add(customer);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to add customer: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error adding customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public List<Customer> GetAllCustomers()
    {
        try
        {
            return _db.Customers.ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to retrieve customers: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return new List<Customer>();
        }
    }

    public Customer? GetCustomerById(int id)
    {
        try
        {
            return _db.Customers
                .Include(c => c.Orders)
                    .ThenInclude(o => o.Sheet)
                .Include(c => c.Orders)
                    .ThenInclude(o => o.Pieces)
                .FirstOrDefault(c => c.Id == id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to retrieve customer: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }

    public Customer? GetCustomerByName(string name)
    {
        try
        {
            if (string.IsNullOrEmpty(name))
                return null;
            
            return _db.Customers
                .Include(c => c.Orders)
                    .ThenInclude(o => o.Sheet)
                .Include(c => c.Orders)
                    .ThenInclude(o => o.Pieces)
                .FirstOrDefault(c => c.Name == name);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to retrieve customer by name: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }

    public List<Customer> GetCustomersByName(string text)
    {
        try
        {
            if (string.IsNullOrEmpty(text))
                return new List<Customer>();
            
            return _db.Customers
                .Include(c => c.Orders)
                    .ThenInclude(o => o.Sheet)
                .Include(c => c.Orders)
                    .ThenInclude(o => o.Pieces)
                .Where(c => c.Name.Contains(text))
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to search customers: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return new List<Customer>();
        }
    }

    public void UpdateCustomer(Customer customer)
    {
        try
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));
            
            _db.Customers.Update(customer);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to update customer: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error updating customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void DeleteCustomer(Customer customer)
    {
        try
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));
            
            _db.Customers.Remove(customer);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to delete customer: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error deleting customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    // =====================
    // ORDERS
    // =====================

    public void AddOrder(Order order)
    {
        try
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));
            
            _db.Orders.Add(order);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to add order: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error adding order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public List<Order> GetAllOrders()
    {
        try
        {
            return _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Sheet)
                .Include(o => o.Pieces)
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to retrieve orders: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return new List<Order>();
        }
    }

    public Order? GetOrderById(int id)
    {
        try
        {
            return _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Sheet)
                .Include(o => o.Pieces)
                .FirstOrDefault(o => o.Id == id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to retrieve order: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }

    public void UpdateOrder(Order order)
    {
        try
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));
            
            _db.Orders.Update(order);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to update order: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error updating order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void DeleteOrder(Order order)
    {
        try
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));
            
            _db.Orders.Remove(order);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to delete order: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error deleting order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    // =====================
    // ORDER PIECES
    // =====================

    public void AddOrderPiece(OrderPiece orderPiece)
    {
        try
        {
            if (orderPiece == null)
                throw new ArgumentNullException(nameof(orderPiece));
            
            _db.OrderPieces.Add(orderPiece);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to add order piece: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error adding order piece: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void AddOrderPieces(IEnumerable<OrderPiece> orderPieces)
    {
        try
        {
            if (orderPieces == null)
                throw new ArgumentNullException(nameof(orderPieces));
            
            _db.OrderPieces.AddRange(orderPieces);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to add order pieces: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error adding order pieces: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public List<OrderPiece> GetAllOrderPieces()
    {
        try
        {
            return _db.OrderPieces
                .Include(op => op.Order)
                    .ThenInclude(o => o.Customer)
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to retrieve order pieces: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return new List<OrderPiece>();
        }
    }

    public List<OrderPiece> GetOrderPiecesByOrderId(int orderId)
    {
        try
        {
            return _db.OrderPieces
                .Include(op => op.Order)
                    .ThenInclude(o => o.Customer)
                .Where(op => op.OrderId == orderId)
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to retrieve order pieces: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return new List<OrderPiece>();
        }
    }

    public OrderPiece? GetOrderPieceById(int id)
    {
        try
        {
            return _db.OrderPieces
                .Include(op => op.Order)
                    .ThenInclude(o => o.Customer)
                .FirstOrDefault(op => op.Id == id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to retrieve order piece: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }

    public void UpdateOrderPiece(OrderPiece orderPiece)
    {
        try
        {
            if (orderPiece == null)
                throw new ArgumentNullException(nameof(orderPiece));
            
            _db.OrderPieces.Update(orderPiece);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to update order piece: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error updating order piece: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void DeleteOrderPiece(OrderPiece orderPiece)
    {
        try
        {
            if (orderPiece == null)
                throw new ArgumentNullException(nameof(orderPiece));
            
            _db.OrderPieces.Remove(orderPiece);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to delete order piece: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error deleting order piece: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    // =====================
    // INVENTORY
    // =====================

    public void AddInventoryItem(InventoryItem inventoryItem)
    {
        try
        {
            if (inventoryItem == null)
                throw new ArgumentNullException(nameof(inventoryItem));
            
            _db.InventoryItems.Add(inventoryItem);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to add inventory item: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error adding inventory item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void AddInventoryItems(IEnumerable<InventoryItem> inventoryItems)
    {
        try
        {
            if (inventoryItems == null)
                throw new ArgumentNullException(nameof(inventoryItems));
            
            _db.InventoryItems.AddRange(inventoryItems);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to add inventory items: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error adding inventory items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public List<InventoryItem> GetAllInventoryItems()
    {
        try
        {
            return _db.InventoryItems
                .Include(ii => ii.Sheet)
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to retrieve inventory items: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return new List<InventoryItem>();
        }
    }

    public InventoryItem? GetInventoryItemById(int id)
    {
        try
        {
            return _db.InventoryItems
                .Include(ii => ii.Sheet)
                .FirstOrDefault(ii => ii.Id == id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to retrieve inventory item: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }

    public void UpdateInventoryItem(InventoryItem inventoryItem)
    {
        try
        {
            if (inventoryItem == null)
                throw new ArgumentNullException(nameof(inventoryItem));
            
            _db.InventoryItems.Update(inventoryItem);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to update inventory item: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error updating inventory item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void DeleteInventoryItem(InventoryItem inventoryItem)
    {
        try
        {
            if (inventoryItem == null)
                throw new ArgumentNullException(nameof(inventoryItem));
            
            _db.InventoryItems.Remove(inventoryItem);
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to delete inventory item: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error deleting inventory item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    // =====================
    // SAVE
    // =====================

    public void SaveAllChanges()
    {
        try
        {
            _db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            MessageBox.Show($"Failed to save changes: {ex.InnerException?.Message ?? ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error saving changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void Dispose()
    {
        try
        {
            _db?.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error disposing database context: {ex.Message}");
        }
    }
}