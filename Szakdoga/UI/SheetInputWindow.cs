using Microsoft.IdentityModel.Tokens;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Szakdoga
{
    internal class SheetInputWindow : Window
    {
        private TextBox nameBox;
        private TextBox descriptionBox;
        private TextBox colorBox;
        private TextBox widthBox;
        private TextBox heightBox;
        private TextBox priceBox;

        public string SheetName => nameBox.Text;
        public string Description => descriptionBox.Text;
        public string Color => colorBox.Text;
        public double width => Convert.ToDouble(widthBox.Text ?? "0.0");
        public double height => Convert.ToDouble(heightBox.Text ?? "0.0");
        public double Price => Convert.ToDouble(priceBox.Text ?? "0.0");

        public SheetInputWindow()
        {
            Title = "Új lap";
            Width = 350;
            Height = 270;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var grid = new Grid
            {
                Margin = new Thickness(10)
            };

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition()); // spacer
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            // ===== Name =====

            var nameLabel = new TextBlock
            {
                Text = "Név:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };

            var nameHint = "Add meg a nevet...";
            nameBox = CreateHintTextBox(nameHint);

            Grid.SetRow(nameLabel, 0);
            Grid.SetColumn(nameLabel, 0);

            Grid.SetRow(nameBox, 0);
            Grid.SetColumn(nameBox, 1);

            grid.Children.Add(nameLabel);
            grid.Children.Add(nameBox);

            // ===== Description =====

            var descriptionLabel = new TextBlock
            {
                Text = "Description:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };

            var descriptionHint = "Add meg a leírást...";
            descriptionBox = CreateHintTextBox(descriptionHint);

            Grid.SetRow(descriptionLabel, 1);
            Grid.SetColumn(descriptionLabel, 0);

            Grid.SetRow(descriptionBox, 1);
            Grid.SetColumn(descriptionBox, 1);

            grid.Children.Add(descriptionLabel);
            grid.Children.Add(descriptionBox);

            // ===== Color =====

            var colorLabel = new TextBlock
            {
                Text = "Szín:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };

            var colorHint = "Add meg a színt...";
            colorBox = CreateHintTextBox(colorHint);

            Grid.SetRow(colorLabel, 2);
            Grid.SetColumn(colorLabel, 0);

            Grid.SetRow(colorBox, 2);
            Grid.SetColumn(colorBox, 1);

            grid.Children.Add(colorLabel);
            grid.Children.Add(colorBox);

            // ===== Width =====

            var widthLabel = new TextBlock
            {
                Text = "Szélesség:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };

            var widthHint = "Add meg a szélességet...";
            widthBox = CreateHintTextBox(widthHint);

            Grid.SetRow(widthLabel, 3);
            Grid.SetColumn(widthLabel, 0);

            Grid.SetRow(widthBox, 3);
            Grid.SetColumn(widthBox, 1);

            grid.Children.Add(widthLabel);
            grid.Children.Add(widthBox);

            // ===== Height =====

            var heightLabel = new TextBlock
            {
                Text = "Magasság:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };

            var heightHint = "Add meg a magasságot...";
            heightBox = CreateHintTextBox(heightHint);

            Grid.SetRow(heightLabel, 4);
            Grid.SetColumn(heightLabel, 0);

            Grid.SetRow(heightBox, 4);
            Grid.SetColumn(heightBox, 1);

            grid.Children.Add(heightLabel);
            grid.Children.Add(heightBox);

            // ===== Price =====

            var priceLabel = new TextBlock
            {
                Text = "Ár:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };

            var priceHint = "Add meg az árt...";
            priceBox = CreateHintTextBox(priceHint);

            Grid.SetRow(priceLabel, 5);
            Grid.SetColumn(priceLabel, 0);

            Grid.SetRow(priceBox, 5);
            Grid.SetColumn(priceBox, 1);

            grid.Children.Add(priceLabel);
            grid.Children.Add(priceBox);
            // ===== Mentés gomb =====

            var saveButton = new Button
            {
                Content = "Mentés",
                Width = 90,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            saveButton.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(nameBox.Text) || nameBox.Text == nameHint)
                {
                    MessageBox.Show("A név megadása kötelező!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrEmpty(widthBox.Text) || widthBox.Text == widthHint)
                {
                    MessageBox.Show("A szélesség megadása kötelező!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrEmpty(heightBox.Text) || heightBox.Text == heightHint)
                {
                    MessageBox.Show("A magasság megadása kötelező!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (descriptionBox.Text == descriptionHint)
                    descriptionBox.Text = "";
                
                if (colorBox.Text == colorHint)
                    colorBox.Text = "";
                
                if (string.IsNullOrEmpty(priceBox.Text) || priceBox.Text == priceHint)
                    priceBox.Text = "0.0";

                DialogResult = true;
                Close();
            };

            Grid.SetRow(saveButton, 7);
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