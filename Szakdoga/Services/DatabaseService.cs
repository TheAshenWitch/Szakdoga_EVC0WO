using Szakdoga.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Szakdoga.Services
{
    public class DatabaseService : IDisposable
    {
        private readonly AppDbContext _db;

        public DatabaseService()
        {
            _db = new AppDbContext();
        }


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
            return _db.Sheets.ToList();
        }
        public Sheet? GetSheetById(int id)
        {
            return _db.Sheets.FirstOrDefault(s => s.Id == id);
        }
        public void UpdateSheet(Sheet sheet)
        {
            _db.Sheets.Update(sheet);
            _db.SaveChanges();
        }
        public void deleteSheet(Sheet sheet)
        {
            _db.Sheets.Remove(sheet);
            _db.SaveChanges();
        }


        public void AddCustomer(Customer customer)
        {
            _db.Customers.Add(customer);
            _db.SaveChanges();
        }
        public List<Customer> GetAllCustomers()
        {
            return _db.Customers.ToList();
        }
        public Customer? GetCustomerById(int id)
        {
            return _db.Customers.FirstOrDefault(c => c.Id == id);
        }
        public void UpdateCustomer(Customer customer)
        {
            _db.Customers.Update(customer);
            _db.SaveChanges();
        }
        public void deleteCustomer(Customer customer)
        {
            _db.Customers.Remove(customer);
            _db.SaveChanges();
        }


        public void AddOrder(Order order)
        {
            _db.Orders.Add(order);
            _db.SaveChanges();
        }
        public List<Order> GetAllOrders()
        {
            return _db.Orders.ToList();
        }
        public Order? GetOrderById(int id)
        {
            return _db.Orders.FirstOrDefault(o => o.Id == id);
        }
        public void UpdateOrder(Order order)
        {
            _db.Orders.Update(order);
            _db.SaveChanges();
        }
        public void deleteOrder(Order order)
        {
            _db.Orders.Remove(order);
            _db.SaveChanges();
        }


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
            return _db.OrderPieces.ToList();
        }
        public OrderPiece? GetOrderPieceById(int id)
        {
            return _db.OrderPieces.FirstOrDefault(op => op.Id == id);
        }
        public void UpdateOrderPiece(OrderPiece orderPiece)
        {
            _db.OrderPieces.Update(orderPiece);
            _db.SaveChanges();
        }
        public void deleteOrderPiece(OrderPiece orderPiece)
        {
            _db.OrderPieces.Remove(orderPiece);
            _db.SaveChanges();
        }


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
            return _db.InventoryItems.ToList();
        }
        public InventoryItem? GetInventoryItemById(int id)
        {
            return _db.InventoryItems.FirstOrDefault(ii => ii.Id == id);
        }
        public void UpdateInventoryItem(InventoryItem inventoryItem)
        {
            _db.InventoryItems.Update(inventoryItem);
            _db.SaveChanges();
        }
        public void deleteInventoryItem(InventoryItem inventoryItem)
        {
            _db.InventoryItems.Remove(inventoryItem);
            _db.SaveChanges();
        }


        public void SaveAllChanges()
        {
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}