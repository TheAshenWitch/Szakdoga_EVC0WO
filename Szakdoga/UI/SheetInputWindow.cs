using Microsoft.IdentityModel.Tokens;
using Sprache;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Szakdoga.Models;
using Szakdoga.Resources;

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

        public Sheet sheet;

        public SheetInputWindow()
        {
            Title = Strings.SITitle;
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
                Text = Strings.SINameLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10),
                Foreground = Brushes.OrangeRed
            };

            var nameHint = Strings.SINameHint;
            nameBox = CreateHintTextBox(nameHint);
            nameBox.TextChanged += (s, e) =>
            {
                if (nameBox.Text == nameHint || nameBox.Text == "")
                    nameBox.Foreground = Brushes.Black;
                else
                    nameBox.Foreground = Brushes.Black;
            };


            Grid.SetRow(nameLabel, 0);
            Grid.SetColumn(nameLabel, 0);

            Grid.SetRow(nameBox, 0);
            Grid.SetColumn(nameBox, 1);

            grid.Children.Add(nameLabel);
            grid.Children.Add(nameBox);


            // ===== Description =====
            var descriptionLabel = new TextBlock
            {
                Text = Strings.SIDescriptionLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };

            var descriptionHint = Strings.SIDescriptionHint;
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
                Text = Strings.SIColorLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };

            var colorHint = Strings.SIColorHint;
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
                Text = Strings.SIWidthLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10),
                Foreground = Brushes.OrangeRed
            };

            var widthHint = Strings.SIWidthHint;
            widthBox = CreateHintTextBox(widthHint);
            widthBox.TextChanged += (s, e) =>
            {
                if (widthBox.Text == widthHint || widthBox.Text == "")
                    widthLabel.Foreground = Brushes.OrangeRed;
                else if (!double.TryParse(widthBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    widthBox.Foreground = Brushes.Red;
                else
                {
                    widthLabel.Foreground = Brushes.Black;
                    widthBox.Foreground = Brushes.Black;
                }
            };

            Grid.SetRow(widthLabel, 3);
            Grid.SetColumn(widthLabel, 0);

            Grid.SetRow(widthBox, 3);
            Grid.SetColumn(widthBox, 1);

            grid.Children.Add(widthLabel);
            grid.Children.Add(widthBox);


            // ===== Height =====
            var heightLabel = new TextBlock
            {
                Text = Strings.SIHeightLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10),
                Foreground = Brushes.OrangeRed
            };

            var heightHint = Strings.SIHeightHint;
            heightBox = CreateHintTextBox(heightHint);
            heightBox.TextChanged += (s, e) =>
            {
                if (heightBox.Text == heightHint || heightBox.Text == "")
                    widthLabel.Foreground = Brushes.OrangeRed;
                else if (!double.TryParse(heightBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    heightBox.Foreground = Brushes.Red;
                else
                {
                    heightBox.Foreground = Brushes.Black;
                    widthLabel.Foreground = Brushes.Black;
                }
            };

            Grid.SetRow(heightLabel, 4);
            Grid.SetColumn(heightLabel, 0);

            Grid.SetRow(heightBox, 4);
            Grid.SetColumn(heightBox, 1);

            grid.Children.Add(heightLabel);
            grid.Children.Add(heightBox);


            // ===== Price ====
            var priceLabel = new TextBlock
            {
                Text = Strings.SIPriceLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };

            var priceHint = Strings.SIPriceHint;
            priceBox = CreateHintTextBox(priceHint);
            priceBox.TextChanged += (s, e) => {
                if (!double.TryParse(priceBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    priceBox.Foreground = Brushes.Red;
                else
                {
                    priceBox.Foreground = Brushes.Black;
                }
            };

            Grid.SetRow(priceLabel, 5);
            Grid.SetColumn(priceLabel, 0);

            Grid.SetRow(priceBox, 5);
            Grid.SetColumn(priceBox, 1);

            grid.Children.Add(priceLabel);
            grid.Children.Add(priceBox);


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
                double result;
                double _width, _height, _price;
                if (string.IsNullOrEmpty(nameBox.Text) || nameBox.Text == nameHint)
                {
                    MessageBox.Show(Strings.SINameIsEmpty, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _width = double.TryParse(widthBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out result) ? result : 2800.0;
                _height = double.TryParse(heightBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out result) ? result : 2070.0;
                _price = double.TryParse(priceBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out result) ? result : 10000.0;
                if (_width <= 0 || _height <= 0 || _price <= 0)
                {
                    MessageBox.Show(Strings.SIDimensionError, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrEmpty(widthBox.Text) || widthBox.Text == widthHint)
                {
                    MessageBox.Show(Strings.SIWidthIsEmpty, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (!double.TryParse(widthBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                {
                    MessageBox.Show(Strings.SIInvalidInput, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (string.IsNullOrEmpty(heightBox.Text) || heightBox.Text == heightHint)
                {
                    MessageBox.Show(Strings.SIHeightIsEmpty, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (!double.TryParse(heightBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                {
                    MessageBox.Show(Strings.SIInvalidInput, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (descriptionBox.Text == descriptionHint)
                    descriptionBox.Text = "";
                
                if (colorBox.Text == colorHint)
                    colorBox.Text = "";
                
                if (string.IsNullOrEmpty(priceBox.Text) || priceBox.Text == priceHint)
                    priceBox.Text = "0";
                else if (!double.TryParse(priceBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                {
                    MessageBox.Show(Strings.SIInvalidInput, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                sheet = new Sheet
                {
                    Name = nameBox.Text,
                    Description = descriptionBox.Text,
                    Color = colorBox.Text,
                    Width = _width,
                    Height = _height,
                    Price = _price,
                };

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