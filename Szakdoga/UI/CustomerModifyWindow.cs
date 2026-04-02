using System.Windows;
using System.Windows.Controls;
using Szakdoga.Resources;
using System.Windows.Media;
using Szakdoga.Models;
using SixLabors.ImageSharp.ColorSpaces;

namespace Szakdoga.UI
{
    internal class CustomerModifyWindow : Window
    {
        private ComboBox customerBox;

        private TextBox customerNameBox = new TextBox();
        private TextBox emailBox = new TextBox();
        private TextBox phoneBox = new TextBox();
        List<Customer> customers;

        public Customer? customer;
        public CustomerModifyWindow(DatabaseService DB, bool? forDelete = false)
        {
            customers = DB.GetAllCustomers();

            if (forDelete == true)
                Title = Strings.CDTitle;
            else
                Title = Strings.CMTitle;

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
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition()); // spacer
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            // ===== Customer =======
            var customerLabel = new TextBlock
            {
                Text = Strings.CMCustomerLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10),
                Foreground = Brushes.OrangeRed
            };
            
            customerBox = CreateSearchComboBox();
            var customerView = System.Windows.Data.CollectionViewSource.GetDefaultView(customers);

            customerView.Filter = obj =>
            {
                if (string.IsNullOrWhiteSpace(customerBox.Text))
                    return true;

                var c = obj as Customer;

                return c.Name.Contains(customerBox.Text, StringComparison.OrdinalIgnoreCase)
                    || (c.Phone != null && c.Phone.Contains(customerBox.Text));
            };

            customerBox.ItemsSource = customerView;

            customerBox.AddHandler(TextBox.TextChangedEvent,
            new TextChangedEventHandler((s, e) =>
            {
                customerView.Refresh();
                customerBox.IsDropDownOpen = true;
            }));
            customerBox.SelectionChanged += (s, e) =>
            {
                if (customerBox.SelectedItem is Customer selectedCustomer)
                {
                    customerLabel.Foreground = Brushes.Black;
                    customerNameBox.Text = selectedCustomer.Name;
                    emailBox.Text = selectedCustomer.Email;
                    phoneBox.Text = selectedCustomer.Phone;
                }
                if (customerBox.SelectedItem == null)
                {
                    customerLabel.Foreground = Brushes.OrangeRed;
                    customerNameBox.Text = "";
                    emailBox.Text = "";
                    phoneBox.Text = "";
                }
            };

            Grid.SetRow(customerLabel, 0);
            Grid.SetColumn(customerLabel, 0);

            Grid.SetRow(customerBox, 0);
            Grid.SetColumn(customerBox, 1);

            grid.Children.Add(customerLabel);
            grid.Children.Add(customerBox);

            // ===== Név =====

            var nameLabel = new TextBlock
            {
                Text = Strings.CINameLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };

            Grid.SetRow(nameLabel, 1);
            Grid.SetColumn(nameLabel, 0);

            Grid.SetRow(customerNameBox, 1);
            Grid.SetColumn(customerNameBox, 1);

            grid.Children.Add(nameLabel);
            grid.Children.Add(customerNameBox);

            // ===== Email =====

            var emailLabel = new TextBlock
            {
                Text = Strings.CIEmailLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };

            Grid.SetRow(emailLabel, 2);
            Grid.SetColumn(emailLabel, 0);

            Grid.SetRow(emailBox, 2);
            Grid.SetColumn(emailBox, 1);

            grid.Children.Add(emailLabel);
            grid.Children.Add(emailBox);

            // ===== Telefon =====

            var phoneLabel = new TextBlock
            {
                Text = Strings.CIPhoneLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };

            Grid.SetRow(phoneLabel, 3);
            Grid.SetColumn(phoneLabel, 0);

            Grid.SetRow(phoneBox, 3);
            Grid.SetColumn(phoneBox, 1);

            grid.Children.Add(phoneLabel);
            grid.Children.Add(phoneBox);

            // ===== Mentés gomb =====
            if(forDelete == false)
            {
                var saveButton = new Button
                {
                    Content = Strings.SaveButton,
                    Width = 90,
                    Height = 30,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                saveButton.Click += (s, e) =>
                {
                    if(customerBox.SelectedItem == null)
                    {
                        MessageBox.Show(Strings.NoCustomerSelectedForModify, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(customerNameBox.Text))
                    {
                        MessageBox.Show(Strings.CINameIsEmpty, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(emailBox.Text))
                    {
                        emailBox.Text = "";
                    }
                    if (string.IsNullOrWhiteSpace(phoneBox.Text))
                    {
                        phoneBox.Text = "";
                    }

                    customer = (Customer)customerBox.SelectedItem;

                    customer.Name = customerNameBox.Text;
                    customer.Email = emailBox.Text;
                    customer.Phone = phoneBox.Text;

                    DialogResult = true;
                    Close();
                };

                Grid.SetRow(saveButton, 4);
                Grid.SetColumn(saveButton, 1);

                grid.Children.Add(saveButton);
            }
            // ===== Törlés gomb =====
            if (forDelete == true)
            {
                SolidColorBrush LightGrey = new SolidColorBrush(Color.FromRgb(230, 230, 230));
                customerNameBox.IsReadOnly = true;
                customerNameBox.Background = LightGrey;
                phoneBox.IsReadOnly = true;
                phoneBox.Background = LightGrey;
                emailBox.IsReadOnly = true;
                emailBox.Background = LightGrey;

                var deleteButton = new Button
                {
                    Content = Strings.DeleteButton,
                    Width = 90,
                    Height = 30,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Foreground = Brushes.Red,
                    BorderBrush = Brushes.Red,
                    FontWeight = FontWeights.Bold,
                    BorderThickness = new Thickness(2)
                };

                deleteButton.Click += (s, e) =>
                {
                    if (customerBox.SelectedItem == null)
                    {
                        MessageBox.Show(Strings.NoCustomerSelectedForDelete, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    customer = (Customer)customerBox.SelectedItem;

                    customer.Name = customerNameBox.Text;
                    customer.Email = emailBox.Text;
                    customer.Phone = phoneBox.Text;

                    var result = MessageBox.Show(string.Format(Strings.DeleteButton, customer.Name), Strings.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        DialogResult = true;
                        Close();
                    }

                };
                Grid.SetRow(deleteButton, 4);
                Grid.SetColumn(deleteButton, 1);

                grid.Children.Add(deleteButton);
            }
            Content = grid;
        }

        

        private ComboBox CreateSearchComboBox(string? text = "")
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
    }
}
