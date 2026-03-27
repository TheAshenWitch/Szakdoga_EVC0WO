using Microsoft.IdentityModel.Tokens;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Szakdoga.Models;
using System.Linq;
using Szakdoga.Resources;

namespace Szakdoga.UI
{

    internal class OrderInputWindow : Window
    {
        DatabaseService _db = new DatabaseService();

        Customer? retCustomer;
        Sheet? retSheet;

        ComboBox customerNameBox;
        TextBox titleBox;
        ComboBox sheetBox;
        List<Customer> customers;
        List<Sheet> sheets;

        public int? retCustomerId;
        public int? retSheetId;
        public string OrderTitle => titleBox.Text;

        string orderTitleHint = Strings.OIOrderTitleHint;

        private string searchText;
        private bool isSelecting = false;
        public OrderInputWindow(DatabaseService DB, string title, Customer? customer, string? orderTitle, Sheet? sheet)
        {
            customers = DB.GetAllCustomers();
            sheets = DB.GetAllSheets();

            Title = title;
            Width = 350;
            Height = 220;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            var grid = new Grid
            {
                Margin = new Thickness(10)
            };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition()); // spacer
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            

            // ===== Ügyfél neve =====
            var customerNameLabel = new TextBlock
            {
                Text = Strings.OICustomerNameLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };

            string customerNameText;
            if(customer != null)
                customerNameText = customer.Name;
            else
                customerNameText = "";

            customerNameBox = CreateSearchComboBox(customerNameText);

            var customerView = System.Windows.Data.CollectionViewSource.GetDefaultView(customers);

            customerView.Filter = obj =>
            {
                if (string.IsNullOrWhiteSpace(customerNameBox.Text))
                    return true;

                var c = obj as Customer;

                return c.Name.Contains(customerNameBox.Text, StringComparison.OrdinalIgnoreCase)
                    || (c.Phone != null && c.Phone.Contains(customerNameBox.Text));
            };

            customerNameBox.ItemsSource = customerView;


            customerNameBox.AddHandler(TextBox.TextChangedEvent,
            new TextChangedEventHandler((s, e) =>
            {
                customerView.Refresh();
                customerNameBox.IsDropDownOpen = true;
            }));


            Grid.SetRow(customerNameLabel, 0);
            Grid.SetColumn(customerNameLabel, 0);

            Grid.SetRow(customerNameBox, 0);
            Grid.SetColumn(customerNameBox, 1);

            grid.Children.Add(customerNameLabel);
            grid.Children.Add(customerNameBox);


            // ===== Order Title =====
            var orderTitleLabel = new TextBlock
            {
                Text = Strings.OIOrderTitleLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };

            string orderTitleText = orderTitle ?? orderTitleHint;
            titleBox = CreateHintTextBox(orderTitleText, orderTitle.IsNullOrEmpty());

            Grid.SetRow(orderTitleLabel, 1);
            Grid.SetColumn(orderTitleLabel, 0);
            Grid.SetRow(titleBox, 1);
            Grid.SetColumn(titleBox, 1);

            grid.Children.Add(orderTitleLabel);
            grid.Children.Add(titleBox);



            // ===== Sheet neve =====
            var sheetNameLabel = new TextBlock
            {
                Text = Strings.OISheetNameLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };

            string sheetNameText;
            if (sheet != null)
                sheetNameText = sheet.Name;
            else
                sheetNameText = "";

            sheetBox = CreateSearchComboBox(sheetNameText);
            var sheetView = System.Windows.Data.CollectionViewSource.GetDefaultView(sheets);

            sheetView.Filter = obj =>
            {
                if (string.IsNullOrWhiteSpace(sheetBox.Text))
                    return true;

                var s = obj as Sheet;

                return s.Name.Contains(sheetBox.Text, StringComparison.OrdinalIgnoreCase);
            };

            sheetBox.ItemsSource = sheetView;

            // frissítés gépeléskor
            sheetBox.AddHandler(TextBox.TextChangedEvent,
                new TextChangedEventHandler((s, e) =>
                {
                    sheetView.Refresh();
                    sheetBox.IsDropDownOpen = true;
                }));


            Grid.SetRow(sheetNameLabel, 2);
            Grid.SetColumn(sheetNameLabel, 0);

            Grid.SetRow(sheetBox, 2);
            Grid.SetColumn(sheetBox, 1);

            grid.Children.Add(sheetNameLabel);
            grid.Children.Add(sheetBox);



            // ===== Mentés gomb =====
            var saveButton = new Button
            {
                Content = Strings.SaveButton,
                Width = 90,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            saveButton.Click += (s, e) =>
            {
                retCustomer = customerNameBox.SelectedItem as Customer;
                retSheet = sheetBox.SelectedItem as Sheet;

                retCustomerId = retCustomer?.Id;
                retSheetId = retSheet?.Id;

                if (titleBox.Text == orderTitleHint || string.IsNullOrWhiteSpace(titleBox.Text))
                {
                    orderTitle = null;
                }
                DialogResult = true;
                Close();
            };

            Grid.SetRow(saveButton, 4);
            Grid.SetColumn(saveButton, 1);

            grid.Children.Add(saveButton);

            Content = grid;
        }

        private ComboBox CreateSearchComboBox(string text)
        {
            var cb = new ComboBox
            {
                Height = 25,
                IsEditable = true,
                IsTextSearchEnabled = true, 
                Text = text,
                StaysOpenOnEdit = true
            };

            // melyik mező alapján keressen
            cb.SetValue(TextSearch.TextPathProperty, "Name");

            return cb;
        }

        private TextBox CreateHintTextBox(string text, bool isHint)
        {
            if (isHint)
            {
                var tb = new TextBox
                {
                    Height = 25,
                    Foreground = Brushes.Gray,
                    Text = text
                };

                tb.GotFocus += (s, e) =>
                {
                    if (tb.Text == text)
                    {
                        tb.Text = "";
                        tb.Foreground = Brushes.Black;
                    }
                };

                tb.LostFocus += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(tb.Text))
                    {
                        tb.Text = text;
                        tb.Foreground = Brushes.Gray;
                    }
                };

                return tb;
            }
            else
            {
                var tb = new TextBox
                {
                    Height = 25,
                    Foreground = Brushes.Black,
                    Text = text
                };
                return tb;
            }
        }
    }
}
