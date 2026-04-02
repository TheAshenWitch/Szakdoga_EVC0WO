using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Szakdoga.Models;
using Szakdoga.Resources;

namespace Szakdoga
{
    internal class CustomerInputWindow : Window
    {
        private TextBox nameBox;
        private TextBox emailBox;
        private TextBox phoneBox;

        public Customer customer;

        public CustomerInputWindow()
        {
            Title = Strings.CITitle;
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

            // ===== Név =====

            var nameLabel = new TextBlock
            {
                Text = Strings.CINameLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10),
                Foreground = Brushes.OrangeRed
            };

            var nameHint = Strings.CINameHint;
            nameBox = CreateHintTextBox(nameHint);
            nameBox.TextChanged += (s, e) =>
            {
                if (nameBox.Text == nameHint || nameBox.Text == "")
                    nameLabel.Foreground = Brushes.OrangeRed;
                else
                    nameLabel.Foreground = Brushes.Black;
            };


            Grid.SetRow(nameLabel, 0);
            Grid.SetColumn(nameLabel, 0);

            Grid.SetRow(nameBox, 0);
            Grid.SetColumn(nameBox, 1);

            grid.Children.Add(nameLabel);
            grid.Children.Add(nameBox);

            // ===== Email =====

            var emailLabel = new TextBlock
            {
                Text = Strings.CIEmailLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };

            var emailHint = Strings.CIEmailHint;
            emailBox = CreateHintTextBox(emailHint);
            emailBox.TextChanged += (s, e) => {
                if(Regex.IsMatch(emailBox.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
                {
                    emailBox.Foreground = Brushes.Black;
                }
                else
                {
                    emailBox.Foreground = Brushes.Red;
                }
            };


            Grid.SetRow(emailLabel, 1);
            Grid.SetColumn(emailLabel, 0);

            Grid.SetRow(emailBox, 1);
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

            var phoneHint = Strings.CIPhoneHint;
            phoneBox = CreateHintTextBox(phoneHint);
            phoneBox.TextChanged += (s, e) => {
                if (Regex.IsMatch(phoneBox.Text, @"^\+?\d{11}$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
                {
                    phoneBox.Foreground = Brushes.Black;
                }
                else
                {
                    phoneBox.Foreground = Brushes.Red;
                }
            };

            Grid.SetRow(phoneLabel, 2);
            Grid.SetColumn(phoneLabel, 0);

            Grid.SetRow(phoneBox, 2);
            Grid.SetColumn(phoneBox, 1);

            grid.Children.Add(phoneLabel);
            grid.Children.Add(phoneBox);

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
                if (string.IsNullOrWhiteSpace(nameBox.Text) || nameBox.Text == nameHint)
                {
                    MessageBox.Show(Strings.CINameIsEmpty, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrWhiteSpace(emailBox.Text) || emailBox.Text == emailHint)
                {
                    emailBox.Text = "";
                }
                else if (!Regex.IsMatch(emailBox.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)) { 
                    MessageBox.Show(Strings.CIEmailInvalid, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrWhiteSpace(phoneBox.Text) || phoneBox.Text == phoneHint)
                {
                    phoneBox.Text = "";
                }
                else if(!Regex.IsMatch(phoneBox.Text, @"^\+?\d{11}$", RegexOptions.IgnoreCase))
                {
                    MessageBox.Show(Strings.CIPhoneInvalid, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                customer = new Customer
                {
                    Name = nameBox.Text,
                    Email = emailBox.Text,
                    Phone = phoneBox.Text
                };

                DialogResult = true;
                Close();
            };

            Grid.SetRow(saveButton, 4);
            Grid.SetColumn(saveButton, 1);

            grid.Children.Add(saveButton);

            Content = grid;
        }

        private TextBox CreateHintTextBox(string hint)
        {
            var tb = new TextBox
            {
                Height = 25,
                Foreground = Brushes.Gray,
                Text = hint
            };

            tb.GotFocus += (s, e) =>
            {
                if (tb.Text == hint)
                {
                    tb.Text = "";
                    tb.Foreground = Brushes.Black;
                }
            };

            tb.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = hint;
                    tb.Foreground = Brushes.Gray;
                }
            };

            return tb;
        }
    }
}