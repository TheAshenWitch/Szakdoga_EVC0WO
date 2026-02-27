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
            Order order = new Order{ Title = "New Order", CreatedAt = DateTime.Now, CustomerId = 1 };
            Db.AddOrder(order);
            Orders.Add(order);
            Db.SaveAllChanges();
        }

        private void AddNewCustomer(object sender, RoutedEventArgs e)
        {
            
        }

        private void UpdateOrder(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteOrder(object sender, RoutedEventArgs e)
        {

        }
    }
}
