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
            OrderInputWindow orderInputWindow = new(Db, Strings.AddNewOrderLabel, null,null,null);
            if (orderInputWindow.ShowDialog() == true)
            {                
                Order order = new Order();
                if(orderInputWindow.retCustomerId != null)
                    order.CustomerId = orderInputWindow.retCustomerId;

                order.Title = orderInputWindow.OrderTitle ?? Strings.EmptyOrderTitle;

                if(orderInputWindow.retSheetId != null)
                    order.SheetId = orderInputWindow.retSheetId;

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
               if (customerInputWindow.customer == null)
                   return;             
                Db.AddCustomer(customerInputWindow.customer);
                Db.SaveAllChanges();
            }
        }
        private void AddNewSheet(object sender, RoutedEventArgs e)
        {
            SheetInputWindow sheetInputWindow = new SheetInputWindow();
            if (sheetInputWindow.ShowDialog() == true)
            {
                if (sheetInputWindow.sheet == null)
                    return;
                Db.AddSheet(sheetInputWindow.sheet);
                Db.SaveAllChanges();
            }
        }
        private void UpdateOrder(object sender, RoutedEventArgs e)
        {
            if (OrderListView.SelectedItem == null)
            {
                MessageBox.Show(Strings.NoOrderSelectedForModify);
                return;
            }
            Order order = (Order)OrderListView.SelectedItem;

            OrderInputWindow orderInputWindow = new(Db, Strings.UpdateOrderTitle, order.Customer, order.Title ?? "",  order.Sheet);
            if (orderInputWindow.ShowDialog() == true)
            { 
                if (order != null)
                {
                    order.Title = orderInputWindow.OrderTitle;
                    if(orderInputWindow.retCustomerId != null)
                        order.CustomerId = orderInputWindow.retCustomerId;
                    if(orderInputWindow.retSheetId != null)
                        order.SheetId = orderInputWindow.retSheetId;
                }

                Db.UpdateOrder(order!);
                Db.SaveAllChanges();
                CollectionViewSource.GetDefaultView(OrderListView.ItemsSource).Refresh();
            }
        }
        private void UpdateCustomer(object sender, RoutedEventArgs e)
        {
            CustomerModifyWindow customerModifyWindow = new CustomerModifyWindow(Db);
            if (customerModifyWindow.ShowDialog() == true)
            {
                Customer customer = customerModifyWindow.customer;

                if (customer != null)
                {
                    Db.UpdateCustomer(customer);
                    Db.SaveAllChanges();
                    CollectionViewSource.GetDefaultView(OrderListView.ItemsSource).Refresh();
                }
            }

        }
        private void UpdateSheet(object sender, RoutedEventArgs e)
        {
            SheetModifyWindow sheetModifyWindow = new SheetModifyWindow(Db);
            if (sheetModifyWindow.ShowDialog() == true)
            {
                Sheet sheet = sheetModifyWindow.sheet;
                if (sheet != null)
                {
                    Db.UpdateSheet(sheet);
                    Db.SaveAllChanges();
                    CollectionViewSource.GetDefaultView(OrderListView.ItemsSource).Refresh();
                }
            }
        }
        private void DeleteOrder(object sender, RoutedEventArgs e)
        {
            if (OrderListView.SelectedItem == null)
            {
                MessageBox.Show(Strings.NoOrderSelectedForDelete);
                return;
            }

            Order selectedOrder = (Order)OrderListView.SelectedItem;
            if (selectedOrder != null)
            {
                var result = MessageBox.Show(Strings.ConfrimDelete, Strings.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    Db.DeleteOrder(selectedOrder);
                    Orders.Remove(selectedOrder);
                    Db.SaveAllChanges();
                }
            }
        }
        private void DeleteCustomer(object sender, RoutedEventArgs e)
        {
            CustomerModifyWindow customerDeleteWindow = new CustomerModifyWindow(Db, true);
            if (customerDeleteWindow.ShowDialog() == true)
            {
                Customer customer = customerDeleteWindow.customer;

                if (customer != null)
                {
                    Db.DeleteCustomer(customer);
                    Db.SaveAllChanges();
                    CollectionViewSource.GetDefaultView(OrderListView.ItemsSource).Refresh();
                }
            }
        }
        private void DeleteSheet(object sender, RoutedEventArgs e)
        {
            SheetModifyWindow sheetDeleteWindow = new SheetModifyWindow(Db, true);
            if (sheetDeleteWindow.ShowDialog() == true)
            {
                Sheet sheet = sheetDeleteWindow.sheet;
                if (sheet != null)
                {
                    Db.DeleteSheet(sheet);
                    Db.SaveAllChanges();
                    CollectionViewSource.GetDefaultView(OrderListView.ItemsSource).Refresh();
                }
            }
        }
        private void OpenOrder(object sender, MouseButtonEventArgs e)
        {
            if (OrderListView.SelectedItem == null)
            {
                MessageBox.Show(Strings.OpenOrderError);
                return;
            }
            
            Order selectedOrder = (Order)OrderListView.SelectedItem;
            List<Piece> pieces = new List<Piece>();

            pieces = Piece.OrderPiecesToPieces(Db.GetOrderPiecesByOrderId(selectedOrder.Id));
            MainWindow mainWindow = new MainWindow(selectedOrder.Id,pieces);

            mainWindow.Show();

            


            this.Close();
        }
    }
}
