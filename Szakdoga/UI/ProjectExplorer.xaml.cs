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
using Szakdoga.Resources;
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
            OrderInputWindow orderInputWindow = new(Strings.AddNewOrderLabel, null,null,null);
            if (orderInputWindow.ShowDialog() == true)
            {                
                Order order = new Order();
                if(orderInputWindow.retCustomer != null)
                    order.Customer = orderInputWindow.retCustomer;

                order.Title = orderInputWindow.OrderTitle ?? Strings.EmptyOrderTitle;

                if(orderInputWindow.retSheet != null)
                    order.Sheet = orderInputWindow.retSheet;

                order.CreatedAt = DateTime.Now;

                Db.AddOrder(order);
                Db.SaveAllChanges();
                Orders.Add(order);
            }
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
            if (OrderListView.SelectedItem == null)
            {
                MessageBox.Show(Strings.NoOrderSelectedError);
                return;
            }
            Order order = (Order)OrderListView.SelectedItem as Order;

            //teljes objektum helyett csak idt adj át

            OrderInputWindow orderInputWindow = new(Strings.UpdateOrderTitle, order.Customer, order.Title ?? "",  order.Sheet);
            if (orderInputWindow.ShowDialog() == true)
            {
                Order selectedOrder = (Order)OrderListView.SelectedItem;
                if (selectedOrder != null)
                {
                    selectedOrder.Title = orderInputWindow.OrderTitle;
                    selectedOrder.Customer = orderInputWindow.retCustomer;
                    selectedOrder.Sheet = orderInputWindow.retSheet;
                }
                

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
                MessageBox.Show(Strings.OpenOrderError);
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
