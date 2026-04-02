using System.Windows;
using System.Windows.Controls;
using Szakdoga.Resources;
using System.Windows.Media;
using Szakdoga.Models;


namespace Szakdoga.UI
{
    class InventoryMover : Window
    {
        private TextBox QuantityTextBox;
        public int Quantity;

        public InventoryMover(int? max)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;

            Width = 300;
            Height = 150;

            var grid = new Grid
            {
                Margin = new Thickness(10)
            };

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());


            var quantityLabel = new TextBlock
            {
                Foreground = Brushes.OrangeRed,
                Text = Strings.IEMQuantityLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };

            Grid.SetRow(quantityLabel, 0);
            Grid.SetColumn(quantityLabel, 0);
            grid.Children.Add(quantityLabel);

            string text = max.HasValue ? $"{Strings.IEMQuantityHint} (max: {max.Value})" : Strings.IEMQuantityHint;
            QuantityTextBox = CreateHintTextBox(text);
            QuantityTextBox.TextChanged += (s, e) =>
            {
                if (QuantityTextBox.Text == Strings.IEMQuantityHint || QuantityTextBox.Text == "")
                    quantityLabel.Foreground = Brushes.OrangeRed;
                if (int.TryParse(QuantityTextBox.Text, out int qty) && max.HasValue && qty > max.Value)
                {
                    quantityLabel.Foreground = Brushes.Red;
                    QuantityTextBox.Foreground = Brushes.Red;
                    QuantityTextBox.BorderBrush = Brushes.Red;
                }
                else
                    quantityLabel.Foreground = Brushes.Black;
            };

            Grid.SetRow(QuantityTextBox, 0);
            Grid.SetColumn(QuantityTextBox, 1);
            grid.Children.Add(QuantityTextBox);

            var saveButton = new Button
            {
                Content = Strings.SaveButton,
                Width = 90,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Grid.SetRow(saveButton, 2);
            Grid.SetColumn(saveButton, 1);
            grid.Children.Add(saveButton);

            saveButton.Click += (s, e) =>
            {
                Quantity = int.TryParse(QuantityTextBox.Text, out int qty) ? qty : 0;
                if (Quantity > max)
                {
                    MessageBox.Show(Strings.IEMExceedsMaxMessage, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (Quantity > 0)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show(Strings.IEMInvalidQuantityMessage, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

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
