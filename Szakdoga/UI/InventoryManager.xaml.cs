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
using Szakdoga.Resources;
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

        public void AddToInventory(object sender, RoutedEventArgs e)
        {
            InventoryItem? inventoryItem = InventoryItemListView.SelectedItem as InventoryItem;
            if (inventoryItem == null)
            {
                MessageBox.Show(Strings.IENoSelectedItem, Strings.Error, MessageBoxButton.OK);
                return;
            }

            InventoryMover inventoryMover = new InventoryMover();
            if (inventoryMover.ShowDialog() == true)
            {
                inventoryItem.TotalQuantity += inventoryMover.Quantity;
                Db.UpdateInventoryItem(inventoryItem);
                CollectionViewSource.GetDefaultView(InventoryItemListView.ItemsSource).Refresh();
            }
        }
        public void AddToReservedInventory(object sender, RoutedEventArgs e)
        {
            InventoryItem ? inventoryItem = InventoryItemListView.SelectedItem as InventoryItem;
            if (inventoryItem == null)
            {
                MessageBox.Show(Strings.IENoSelectedItem, Strings.Error, MessageBoxButton.OK);
                return;
            }

            InventoryMover inventoryMover = new InventoryMover();
            if (inventoryMover.ShowDialog() == true)
            {
                inventoryItem.ReservedQuantity += inventoryMover.Quantity;
                Db.UpdateInventoryItem(inventoryItem);
                CollectionViewSource.GetDefaultView(InventoryItemListView.ItemsSource).Refresh();
            }
        }
        public void RemoveFromReserved(object sender, RoutedEventArgs e)
        {
            InventoryItem? inventoryItem = InventoryItemListView.SelectedItem as InventoryItem;
            if (inventoryItem == null)
            {
                MessageBox.Show(Strings.IENoSelectedItem, Strings.Error, MessageBoxButton.OK);
                return;
            }

            InventoryMover inventoryMover = new InventoryMover();
            if (inventoryMover.ShowDialog() == true)
            {
                inventoryItem.ReservedQuantity -= inventoryMover.Quantity;
                Db.UpdateInventoryItem(inventoryItem);
                CollectionViewSource.GetDefaultView(InventoryItemListView.ItemsSource).Refresh();
            }
        }
        public void RemoveFromInventory(object sender, RoutedEventArgs e)
        {
            InventoryItem? inventoryItem = InventoryItemListView.SelectedItem as InventoryItem;
            if (inventoryItem == null)
            {
                MessageBox.Show(Strings.IENoSelectedItem, Strings.Error, MessageBoxButton.OK);
                return;
            }

            InventoryMover inventoryMover = new InventoryMover();
            if (inventoryMover.ShowDialog() == true)
            {
                if(inventoryMover.Quantity > inventoryItem.TotalQuantity) 
                {
                    MessageBox.Show(Strings.IENotEnoughInInventory, Strings.Error, MessageBoxButton.OK);
                    return;
                }
                (int removeFromTotal , int removeFromReserved) = TryRemoveQuantity(inventoryItem.ReservedQuantity, inventoryMover.Quantity);
                inventoryItem.TotalQuantity -= removeFromTotal;
                Db.UpdateInventoryItem(inventoryItem);
                CollectionViewSource.GetDefaultView(InventoryItemListView.ItemsSource).Refresh();
            }
        }

        private (int,int) TryRemoveQuantity(int removeFrom, int toRemove)
        {
            int overFlow = 0;
            int removeFromReserved = 0;

            if (toRemove > removeFrom)
            {
                overFlow = toRemove - removeFrom;
                removeFromReserved = removeFrom;
            }
            else
            {
                removeFromReserved = toRemove;
            }

            return (overFlow, removeFromReserved);
        }

    }
}
