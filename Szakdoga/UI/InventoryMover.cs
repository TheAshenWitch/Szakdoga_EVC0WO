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

        public InventoryMover()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

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
                Text = Strings.SMSheetLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };

            Grid.SetRow(QuantityTextBox, 0);
            Grid.SetColumn(QuantityTextBox, 0);
            grid.Children.Add(QuantityTextBox);

            QuantityTextBox = CreateHintTextBox(Strings.IEMQuantityHint);
            QuantityTextBox.TextChanged += (s, e) =>
            {
                if (QuantityTextBox.Text != Strings.IEMQuantityHint || QuantityTextBox.Text != "")
                    quantityLabel.Foreground = Brushes.OrangeRed;
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
