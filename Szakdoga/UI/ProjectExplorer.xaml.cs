using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
        public ProjectExplorer()
        {
            InitializeComponent();
            Db = new DatabaseService();
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
            OrderInputWindow orderInputWindow = new("Update Order", null,null,null);
            if (orderInputWindow.ShowDialog() == true)
            {
                Order selectedOrder = (Order)OrderListView.SelectedItem;
                if (selectedOrder != null)
                {
                    selectedOrder.Title = orderInputWindow.OrderTitle;
                    selectedOrder.Customer.Name = orderInputWindow.CustomerName;
                    //selectedOrder.Sheet = orderInputWindow.Sheet;
                    Db.UpdateOrder(selectedOrder);
                    Db.SaveAllChanges();
                }
            }
            Order order = new Order{ Title = "New Order", CreatedAt = DateTime.Now, CustomerId = 1 };
            Db.AddOrder(order);
            Orders.Add(order);
            Db.SaveAllChanges();
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

        private void UpdateOrder(object sender, RoutedEventArgs e)
        {
            if (OrderListView.SelectedItem == null)
            {
                MessageBox.Show("Please select an order to update.");
                return;
            }
            Order order = (Order)OrderListView.SelectedItem as Order;
            OrderInputWindow orderInputWindow = new("Update Order", order.Customer.Name, order.Title,  order.Sheet.Name);
            if (orderInputWindow.ShowDialog() == true)
            {
                Order selectedOrder = (Order)OrderListView.SelectedItem;
                if (selectedOrder != null)
                {
                    selectedOrder.Title = orderInputWindow.OrderTitle;
                    selectedOrder.Customer.Name = orderInputWindow.CustomerName;
                    //selectedOrder.Sheet = orderInputWindow.Sheet;
                    Db.UpdateOrder(selectedOrder);
                    Db.SaveAllChanges();
                }
            }
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
