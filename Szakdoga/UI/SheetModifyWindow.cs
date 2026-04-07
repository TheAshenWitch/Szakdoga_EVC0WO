using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Szakdoga.Models;
using Szakdoga.Resources;

namespace Szakdoga.UI
{
    internal class SheetModifyWindow : Window
    {
        private ComboBox sheetBox;
        private TextBox nameBox = new TextBox();
        private TextBox descriptionBox = new TextBox();
        private TextBox colorBox = new TextBox();
        private TextBox widthBox = new TextBox();
        private TextBox heightBox = new TextBox();
        private TextBox priceBox = new TextBox();
        List<Sheet> sheets;

        public Sheet sheet;
        public SheetModifyWindow(DatabaseService DB, bool? forDelete = false)
        {
            sheets = DB.GetAllSheets();

            if (forDelete == true)
                Title = Strings.SDTitle;
            else
                Title = Strings.SMTitle;

            Width = 350;
            Height = 300;
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
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition()); // spacer
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            // ===== Customer =======
            var sheetLabel = new TextBlock
            {
                Foreground = Brushes.OrangeRed,
                Text = Strings.SMSheetLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };
            
            sheetBox = CreateSearchComboBox();
            var sheetView = System.Windows.Data.CollectionViewSource.GetDefaultView(sheets);

            sheetView.Filter = obj =>
            {
                if (string.IsNullOrWhiteSpace(sheetBox.Text))
                    return true;

                var c = obj as Sheet;

                return c.Name.Contains(sheetBox.Text, StringComparison.OrdinalIgnoreCase)
                    || (c.Name != null);
            };

            sheetBox.ItemsSource = sheetView;

            sheetBox.AddHandler(TextBox.TextChangedEvent,
            new TextChangedEventHandler((s, e) =>
            {
                sheetView.Refresh();
                sheetBox.IsDropDownOpen = true;
            }));
            sheetBox.SelectionChanged += (s, e) =>
            {
                if (sheetBox.SelectedItem is Sheet selectedCustomer)
                {
                    sheetLabel.Foreground = Brushes.Black;
                    nameBox.Text = selectedCustomer.Name;
                    descriptionBox.Text = selectedCustomer.Description;
                    colorBox.Text = selectedCustomer.Color;
                    widthBox.Text = Convert.ToString(selectedCustomer.Width);
                    heightBox.Text = Convert.ToString(selectedCustomer.Height);
                    priceBox.Text = Convert.ToString(selectedCustomer.Price);
                }
                if(sheetBox.SelectedItem == null)
                {
                    sheetLabel.Foreground = Brushes.OrangeRed;
                    nameBox.Text = "";
                    descriptionBox.Text = "";
                    colorBox.Text = "";
                    widthBox.Text = "";
                    heightBox.Text = "";
                    priceBox.Text = "";
                }   
            };

            Grid.SetRow(sheetLabel, 0);
            Grid.SetColumn(sheetLabel, 0);

            Grid.SetRow(sheetBox, 0);
            Grid.SetColumn(sheetBox, 1);

            grid.Children.Add(sheetLabel);
            grid.Children.Add(sheetBox);

            // ===== Name =====
            var nameLabel = new TextBlock
            {
                Text = Strings.SINameLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };
            nameBox.TextChanged += (s, e) =>
            {
                if (nameBox.Text == "")
                    nameLabel.Foreground = Brushes.OrangeRed;
                else
                    nameLabel.Foreground = Brushes.Black;
            };

            Grid.SetRow(nameLabel, 1);
            Grid.SetColumn(nameLabel, 0);

            Grid.SetRow(nameBox, 1);
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

            Grid.SetRow(descriptionLabel, 2);
            Grid.SetColumn(descriptionLabel, 0);

            Grid.SetRow(descriptionBox, 2);
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

            Grid.SetRow(colorLabel, 3);
            Grid.SetColumn(colorLabel, 0);

            Grid.SetRow(colorBox, 3);
            Grid.SetColumn(colorBox, 1);

            grid.Children.Add(colorLabel);
            grid.Children.Add(colorBox);


            // ===== Width =====
            var widthLabel = new TextBlock
            {
                Text = Strings.SIWidthLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };
            widthBox.TextChanged += (s, e) =>
            {
                if (widthBox.Text == "")
                    widthLabel.Foreground = Brushes.OrangeRed;
                else if (!double.TryParse(widthBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    widthBox.Foreground = Brushes.Red;
                else
                {
                    widthLabel.Foreground = Brushes.Black;
                    widthBox.Foreground = Brushes.Black;
                }
            };

            Grid.SetRow(widthLabel, 4);
            Grid.SetColumn(widthLabel, 0);

            Grid.SetRow(widthBox, 4);
            Grid.SetColumn(widthBox, 1);

            grid.Children.Add(widthLabel);
            grid.Children.Add(widthBox);


            // ===== Height =====
            var heightLabel = new TextBlock
            {
                Text = Strings.SIHeightLabel,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 10)
            };
            heightBox.TextChanged += (s, e) =>
            {
                if (heightBox.Text == "")
                    heightLabel.Foreground = Brushes.OrangeRed;
                else if (!double.TryParse(heightBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    heightBox.Foreground = Brushes.Red;
                else
                {
                    heightLabel.Foreground = Brushes.Black;
                    heightBox.Foreground = Brushes.Black;
                }
            };

            Grid.SetRow(heightLabel, 5);
            Grid.SetColumn(heightLabel, 0);

            Grid.SetRow(heightBox, 5);
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
            priceBox.TextChanged += (s, e) => {
                if (!double.TryParse(priceBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    priceBox.Foreground = Brushes.Red;
                else
                    priceBox.Foreground = Brushes.Black;
            };

            Grid.SetRow(priceLabel, 6);
            Grid.SetColumn(priceLabel, 0);

            Grid.SetRow(priceBox, 6);
            Grid.SetColumn(priceBox, 1);

            grid.Children.Add(priceLabel);
            grid.Children.Add(priceBox);

            if (forDelete == false)
            {
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
                    if(sheetBox.SelectedItem == null)
                    {
                        MessageBox.Show(Strings.SMNoSheetSelectedForModify, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (string.IsNullOrEmpty(nameBox.Text))
                    {
                        MessageBox.Show(Strings.SINameIsEmpty, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (string.IsNullOrEmpty(widthBox.Text))
                    {
                        MessageBox.Show(Strings.SIWidthIsEmpty, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else if (!double.TryParse(widthBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    {
                        MessageBox.Show(Strings.SIInvalidInput, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (string.IsNullOrEmpty(heightBox.Text))
                    {
                        MessageBox.Show(Strings.SIHeightIsEmpty, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else if (!double.TryParse(heightBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    {
                        MessageBox.Show(Strings.SIInvalidInput, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (string.IsNullOrEmpty(priceBox.Text))
                        priceBox.Text = "0";
                    else if (!double.TryParse(priceBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    {
                        MessageBox.Show(Strings.SIInvalidInput, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    sheet = (Sheet)sheetBox.SelectedItem;

                    sheet.Name = nameBox.Text;
                    sheet.Description = descriptionBox.Text;
                    sheet.Color = colorBox.Text;
                    sheet.Width = Convert.ToDouble(widthBox.Text);
                    sheet.Height = Convert.ToDouble(heightBox.Text);
                    sheet.Price = Convert.ToDouble(priceBox.Text);

                    DialogResult = true;
                    Close();
                };

                Grid.SetRow(saveButton, 7);
                Grid.SetColumn(saveButton, 1);

                grid.Children.Add(saveButton);
            }

            if (forDelete == true)
            {
                SolidColorBrush LightGrey = new SolidColorBrush(Color.FromRgb(230, 230, 230));
                nameBox.IsReadOnly = true;
                nameBox.Background = LightGrey;
                descriptionBox.IsReadOnly = true;
                descriptionBox.Background = LightGrey;
                colorBox.IsReadOnly = true;
                colorBox.Background = LightGrey;
                widthBox.IsReadOnly = true;
                widthBox.Background = LightGrey;
                heightBox.IsReadOnly = true;
                heightBox.Background = LightGrey;
                priceBox.IsReadOnly = true;
                priceBox.Background = LightGrey;

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
                    if (sheetBox.SelectedItem == null)
                    {
                        MessageBox.Show(Strings.SDNoSheetSelectedForDelete, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    sheet = (Sheet)sheetBox.SelectedItem;
                    var result = MessageBox.Show(Strings.ConfrimDelete, Strings.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        DialogResult = true;
                        Close();
                    }
                };
                Grid.SetRow(deleteButton, 7);
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
