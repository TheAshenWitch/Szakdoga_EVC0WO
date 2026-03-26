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

        ComboBox customerNameBox;
        TextBox titleBox;
        ComboBox sheetBox;
        List<Customer> customers;
        List<Sheet> sheets;
        public string CustomerName => customerNameBox.Text;
        public string OrderTitle => titleBox.Text;
        public string Sheet => sheetBox.Text;

        string orderTitleHint = Strings.OIOrderTitleHint;

        private DispatcherTimer sheetSearchTimer;
        private DispatcherTimer nameSearchTimer;
        private string searchText;
        private bool isSelecting = false;
        public OrderInputWindow(string title, string? customerName, string? orderTitle, string? sheet)
        {
            sheetSearchTimer = new DispatcherTimer();
            sheetSearchTimer.Interval = TimeSpan.FromMilliseconds(300);
            sheetSearchTimer.Tick += sheetSearchTimer_Tick;

            nameSearchTimer = new DispatcherTimer();
            nameSearchTimer.Interval = TimeSpan.FromMilliseconds(300);
            nameSearchTimer.Tick += nameSearchTimer_Tick;

            using var DB = new DatabaseService();

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

            string customerNameText = customerName ?? "";

            customerNameBox = CreateSearchComboBox(customerNameText);
            customerNameBox.ItemsSource = customers.Select(c => c.Name + " - " + c.Phone).Distinct().ToList();

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

            string sheetNameText = sheet ?? "";

            sheetBox = CreateSearchComboBox(sheetNameText);
            sheetBox.ItemsSource = sheets.Select(s => s.Name + " - " + s.Width + "x" + s.Height).Distinct().ToList();

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
                if(titleBox.Text == orderTitleHint || string.IsNullOrWhiteSpace(titleBox.Text))
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
                IsTextSearchEnabled = false,
                Text = text
            };
            cb.SelectionChanged += (s, e) =>
            {
                isSelecting = true;
            };
            cb.AddHandler(TextBox.TextChangedEvent,
                new TextChangedEventHandler(ComboTextChanged));

            return cb;
        }



        private void ComboTextChanged(object sender, TextChangedEventArgs e)
        {
            if (isSelecting)
            {
                isSelecting = false;
                return;
            }

            var tb = e.OriginalSource as TextBox;

            if (tb == null)
                return;

            if(tb.Text == searchText)
                return;

            searchText = tb.Text;

            if(e.Source == customerNameBox)
            {
                nameSearchTimer.Stop();
                nameSearchTimer.Start();
            }
            else if(e.Source == sheetBox)
            {
                sheetSearchTimer.Stop();
                sheetSearchTimer.Start();
            }
        }

        private void sheetSearchTimer_Tick(object sender, EventArgs e)
        {
            sheetSearchTimer.Stop();

            var sheetNames = sheets.Select(s => s.Name).Where(s => s.ToLower().Contains(sheetBox.Text.ToLower())).Distinct().ToList();

            sheetBox.ItemsSource = sheetNames;

            sheetBox.IsDropDownOpen = true;
        }
        private void nameSearchTimer_Tick(object sender, EventArgs e)
        {
            nameSearchTimer.Stop();

            using var db = new DatabaseService();

            var customerNames = customers.Select(c => c.Name).Where(s => s.ToLower().Contains(customerNameBox.Text.ToLower())).Distinct().ToList();

            customerNameBox.ItemsSource = customerNames;

            customerNameBox.IsDropDownOpen = true;
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
