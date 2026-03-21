using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Szakdoga.Models;
using Szakdoga.Services;
using Szakdoga.UI;
namespace Szakdoga
{
    /// <summary>
    /// Interaction logic for ProjectExplorer.xaml
    /// </summary>
    public partial class ProjectExplorer : Window
    {
        ProjectExplorerViewModel viewModel;
        DatabaseService Db;
        ObservableCollection<Order> Orders;
        Settings settings;
        public ProjectExplorer()
        {
            InitializeComponent();
            Db = new DatabaseService();

            try
            {
                settings = new Settings(
                        Properties.Settings.Default.Language,
                        Properties.Settings.Default.DarkMode,
                        Properties.Settings.Default.SheetHeight,
                        Properties.Settings.Default.SheetWidth,
                        Properties.Settings.Default.BladeThickness,
                        Properties.Settings.Default.SheetPadding,
                        Properties.Settings.Default.SheetColor,
                        Properties.Settings.Default.SheetManufacturer,
                        Properties.Settings.Default.SheetPrice,
                        Properties.Settings.Default.EdgeSealingPrice,
                        Properties.Settings.Default.Currency
                    );
            }
            catch (Exception)
            {
                settings = new Settings();
            }

            CultureInfo culture = new CultureInfo(settings.Language);
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;

            LocalizationManager.Instance.Culture = culture;

            Orders = new ObservableCollection<Order>(Db.GetAllOrders());

            viewModel = new ProjectExplorerViewModel(Orders);
            DataContext = viewModel;
            OrderListView.DataContext = viewModel;
        }
        public class ProjectExplorerViewModel(ObservableCollection<Order> orders) : INotifyPropertyChanged
        {
            public ObservableCollection<Order> Orders =>orders;
            
            public event PropertyChangedEventHandler? PropertyChanged;
        }
        private void MainScreen(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            
            this.Close();
        }
        private void AddNewOrder(object sender, RoutedEventArgs e)
        {
            string? customerName = null;
            string? customerPhone = null;
            string? orderTitle = null;
            string? sheetName = null;
            double? sheetWidth = null, sheetHeight = null;

            OrderInputWindow orderInputWindow = new("Add New Order", null,null,null);
            if (orderInputWindow.ShowDialog() == true)
            {
                // - nél splittelni majd trimmelni kell
                customerName = orderInputWindow.CustomerName.Split(" ")[0];
                customerPhone = orderInputWindow.CustomerName.Split(" ").Length > 2 ? orderInputWindow.CustomerName.Split(" ")[1] : null;
                orderTitle = orderInputWindow.OrderTitle;
                sheetName = orderInputWindow.Sheet.Split(" ")[0];
                sheetWidth = double.TryParse(orderInputWindow.Sheet.Split(" ").Length > 2 ? orderInputWindow.Sheet.Split(" ")[2].Split("x")[0] : null, out double width) ? width : null;
                sheetHeight = double.TryParse(orderInputWindow.Sheet.Split(" ").Length > 2 ? orderInputWindow.Sheet.Split(" ")[2].Split("x")[1] : null, out double height) ? height : null;
            }
            Order order = new Order();
            if(customerName != null)
                order.Customer = Db.GetCustomerByName(customerName);
            order.Title = orderTitle ?? "new order";
            if(sheetName != null)
                order.Sheet = Db.GetSheetByName(sheetName);
            order.CreatedAt = DateTime.Now;
            Db.AddOrder(order);
            Db.SaveAllChanges();
            Orders.Add(order);
        }
        private void AddNewCustomer(object sender, RoutedEventArgs e)
        {
            CustomerInputWindow customerInputWindow = new CustomerInputWindow();
            if (customerInputWindow.ShowDialog() == true)
            {
                Customer newCustomer = new Customer
                {
                    Name = customerInputWindow.CustomerName,
                    Email = customerInputWindow.Email,
                    Phone = customerInputWindow.Phone
                };
                Db.AddCustomer(newCustomer);
                Db.SaveAllChanges();
            }
        }
        private void AddNewSheet(object sender, RoutedEventArgs e)
        {
            SheetInputWindow sheetInputWindow = new SheetInputWindow();
            if (sheetInputWindow.ShowDialog() == true)
            {
                Sheet newSheet = new Sheet
                {
                    Name = sheetInputWindow.SheetName,
                    Description = sheetInputWindow.Description,
                    Height = sheetInputWindow.height,
                    Width = sheetInputWindow.width,
                    Color = sheetInputWindow.Color,
                    Price = sheetInputWindow.Price
                };
                Db.AddSheet(newSheet);
                Db.SaveAllChanges();
            }
        }
        private void UpdateOrder(object sender, RoutedEventArgs e)
        {
            string? customerName = null;
            string? orderTitle = null;
            string? sheetName = null;
            if (OrderListView.SelectedItem == null)
            {
                MessageBox.Show("Please select an order to update.");
                return;
            }
            Order order = (Order)OrderListView.SelectedItem as Order;
            if(order.Customer == null)
                order.Customer = new Customer { Name = "" };
            if(order.Sheet == null)
                order.Sheet = new Sheet { Name = "" };
            OrderInputWindow orderInputWindow = new("Update Order", order.Customer.Name, order.Title,  order.Sheet.Name);
            if (orderInputWindow.ShowDialog() == true)
            {
                Order selectedOrder = (Order)OrderListView.SelectedItem;
                if (selectedOrder != null)
                {
                    orderTitle= orderInputWindow.OrderTitle;
                    customerName = orderInputWindow.CustomerName;
                    sheetName = orderInputWindow.Sheet;
                }
                if (customerName != null)
                    selectedOrder.Customer = Db.GetCustomerByName(customerName);
                if (orderTitle != null)
                    selectedOrder.Title = orderTitle;
                if (sheetName != null)
                    selectedOrder.Sheet = Db.GetSheetByName(sheetName);

                Db.UpdateOrder(selectedOrder);
                Db.SaveAllChanges();
            }
        }
        private void UpdateCustomer(object sender, RoutedEventArgs e)
        {
            //if (OrderListView.SelectedItem == null)
            //{
            //    MessageBox.Show("Please select an order to update its customer.");
            //    return;
            //}
            //Order order = (Order)OrderListView.SelectedItem as Order;
            //if(order.Customer == null)
            //{
            //    MessageBox.Show("Selected order has no customer to update.");
            //    return;
            //}
            //CustomerInputWindow customerInputWindow = new CustomerInputWindow(order.Customer.Name, order.Customer.Email, order.Customer.Phone);
            //if (customerInputWindow.ShowDialog() == true)
            //{
            //    Customer selectedCustomer = order.Customer;
            //    if (selectedCustomer != null)
            //    {
            //        selectedCustomer.Name = customerInputWindow.CustomerName;
            //        selectedCustomer.Email = customerInputWindow.Email;
            //        selectedCustomer.Phone = customerInputWindow.Phone;
            //        Db.UpdateCustomer(selectedCustomer);
            //        Db.SaveAllChanges();
            //    }
            //}
        }
        private void UpdateSheet(object sender, RoutedEventArgs e)
        {
            
        }
        private void DeleteOrder(object sender, RoutedEventArgs e)
        {
            Order selectedOrder = (Order)OrderListView.SelectedItem;
            if (selectedOrder != null)
            {
                Db.DeleteOrder(selectedOrder);
                Orders.Remove(selectedOrder);
                Db.SaveAllChanges();
            }
        }
        private void DeleteCustomer(object sender, RoutedEventArgs e)
        {
            //if (OrderListView.SelectedItem == null)
            //{
            //    MessageBox.Show("Please select an order to delete its customer.");
            //    return;
            //}
            //Order order = (Order)OrderListView.SelectedItem as Order;
            //if(order.Customer == null)
            //{
            //    MessageBox.Show("Selected order has no customer to delete.");
            //    return;
            //}
            //Db.DeleteCustomer(order.Customer);
            //order.Customer = null;
            //Db.SaveAllChanges();
        }
        private void DeleteSheet(object sender, RoutedEventArgs e)
        {

        }
        private void OpenOrder(object sender, MouseButtonEventArgs e)
        {
            if (OrderListView.SelectedItem == null)
            {
                MessageBox.Show("Please select an order to view details.");
                return;
            }
            
            Order selectedOrder = (Order)OrderListView.SelectedItem as Order;
            List<Piece> pieces = new List<Piece>();

            Piece piece = new Piece();//??????????
            pieces = piece.OrderPiecesToPieces(Db.GetOrderPiecesByOrderId(selectedOrder.Id));
            MainWindow mainWindow = new MainWindow(selectedOrder.Id,pieces);

            mainWindow.Show();

            


            this.Close();
        }
    }
}
