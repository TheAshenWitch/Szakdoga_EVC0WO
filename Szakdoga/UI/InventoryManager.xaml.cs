using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static Szakdoga.ProjectExplorer;

namespace Szakdoga.UI
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class InventoryManager : Window
    {
        ProjectExplorerViewModel viewModel;
        DatabaseService Db;
        ObservableCollection<InventoryItem> InventoryItems;
        public InventoryManager()
        {
            InitializeComponent();
            Db = new DatabaseService();
            InventoryItems = new ObservableCollection<InventoryItem>(Db.GetAllInventoryItems());

            viewModel = new ProjectExplorerViewModel(InventoryItems);
            DataContext = viewModel;
            InventoryItemListView.DataContext = viewModel;

            InventoryItemListView.ItemsSource = viewModel.InventoryItems;
        }
        public class ProjectExplorerViewModel(ObservableCollection<InventoryItem> inventoryItems) : INotifyPropertyChanged
        {
            public ObservableCollection<InventoryItem> InventoryItems => inventoryItems;

            public event PropertyChangedEventHandler? PropertyChanged;

        }

        public void AddNewInventoryItem(object sender, RoutedEventArgs e)
        {/*
            var addWindow = new InventoryItemInputWindow();
            if (addWindow.ShowDialog() == true)
            {
                var newItem = addWindow.InventoryItem;
                Db.AddInventoryItem(newItem);
                InventoryItems.Add(newItem);
            }
            */
        }
    }
}
