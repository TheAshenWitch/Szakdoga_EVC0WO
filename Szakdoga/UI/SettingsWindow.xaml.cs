using System;
using System.Collections.Generic;
using System.Globalization;
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
using Szakdoga.Resources;

namespace Szakdoga
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        public SettingsWindow(Settings settings)
        {
            InitializeComponent();
            SheetHeight.Text = settings.SheetHeight.ToString();
            SheetWidth.Text = settings.SheetWidth.ToString();
            BladeThickness.Text = settings.BladeThickness.ToString();
            SheetPadding.Text = settings.SheetPadding.ToString();
            SheetColor.Text = settings.SheetColor;
            SheetManufacturer.Text = settings.SheetManufacturer;
            SheetPrice.Text = settings.SheetPrice.ToString();
            EdgeSealingPrice.Text = settings.EdgeSealingPrice.ToString();
            Lang.SelectedItem = settings.Language;
            // Default language selection
            if (LocalizationManager.Instance.Culture.Name.StartsWith("hu"))
                Lang.SelectedIndex = 0;
            else
                Lang.SelectedIndex = 1;

            Title = Strings.SettingsTitle;

            SheetWidth.TextChanged += (s, e) =>
            {
                if (SheetWidth.Text == "")
                    SheetWidth.BorderBrush = Brushes.OrangeRed;
                else if (!double.TryParse(SheetWidth.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    SheetWidth.Foreground = Brushes.Red;
                else
                {
                    SheetWidth.Foreground = Brushes.Black;
                    SheetWidth.Foreground = Brushes.Black;
                    SheetWidth.BorderBrush = Brushes.Gray;
                }
            };

            SheetHeight.TextChanged += (s, e) =>
            {
                if (SheetHeight.Text == "")
                    SheetHeight.BorderBrush = Brushes.OrangeRed;
                else if (!double.TryParse(SheetHeight.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    SheetHeight.Foreground = Brushes.Red;
                else
                {
                    SheetHeight.Foreground = Brushes.Black;
                    SheetHeight.Foreground = Brushes.Black;
                    SheetHeight.BorderBrush = Brushes.Gray;
                }
            };

            BladeThickness.TextChanged += (s, e) =>
            {
                if (BladeThickness.Text == "")
                    BladeThickness.BorderBrush = Brushes.OrangeRed;
                else if (!double.TryParse(BladeThickness.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    BladeThickness.Foreground = Brushes.Red;
                else
                {
                    BladeThickness.Foreground = Brushes.Black;
                    BladeThickness.Foreground = Brushes.Black;
                    BladeThickness.BorderBrush = Brushes.Gray;
                }
            };

            SheetPadding.TextChanged += (s, e) =>
            {
                if (SheetPadding.Text == "")
                    SheetPadding.BorderBrush = Brushes.OrangeRed;
                else if (!double.TryParse(SheetPadding.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    SheetPadding.Foreground = Brushes.Red;
                else
                {
                    SheetPadding.Foreground = Brushes.Black;
                    SheetPadding.Foreground = Brushes.Black;
                    SheetPadding.BorderBrush = Brushes.Gray;
                }
            };

            SheetPrice.TextChanged += (s, e) =>
            {
                if (SheetPrice.Text == "")
                    SheetPrice.BorderBrush = Brushes.OrangeRed;
                else if (!double.TryParse(SheetPrice.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    SheetPrice.Foreground = Brushes.Red;
                else
                {
                    SheetPrice.Foreground = Brushes.Black;
                    SheetPrice.Foreground = Brushes.Black;
                    SheetPrice.BorderBrush = Brushes.Gray;
                }
            };

            EdgeSealingPrice.TextChanged += (s, e) =>
            {
                if (EdgeSealingPrice.Text == "")
                    EdgeSealingPrice.BorderBrush = Brushes.OrangeRed;
                else if (!double.TryParse(EdgeSealingPrice.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    EdgeSealingPrice.Foreground = Brushes.Red;
                else
                {
                    EdgeSealingPrice.Foreground = Brushes.Black;
                    EdgeSealingPrice.Foreground = Brushes.Black;
                    EdgeSealingPrice.BorderBrush = Brushes.Gray;
                }
            };

            this.Closing += (s, e) =>
            {
                if(SheetHeight.Text == "")
                    SheetHeight.Text = settings.SheetHeight.ToString();
                if(SheetWidth.Text == "")
                    SheetWidth.Text = settings.SheetWidth.ToString();
                if(BladeThickness.Text == "")
                    BladeThickness.Text = settings.BladeThickness.ToString();
                if(SheetPadding.Text == "")
                    SheetPadding.Text = settings.SheetPadding.ToString();
                if(SheetPrice.Text == "")
                    SheetPrice.Text = settings.SheetPrice.ToString();
                if(EdgeSealingPrice.Text == "")
                    EdgeSealingPrice.Text = settings.EdgeSealingPrice.ToString();
            };
        }
        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if(!double.TryParse(SheetWidth.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                MessageBox.Show(Strings.InvalidNumberFormat, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                SheetWidth.BorderBrush = Brushes.Red;
                return;
            }

            if(!double.TryParse(SheetHeight.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                MessageBox.Show(Strings.InvalidNumberFormat, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                SheetHeight.BorderBrush = Brushes.Red;
                return;
            }

            if(!double.TryParse(BladeThickness.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                MessageBox.Show(Strings.InvalidNumberFormat, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                BladeThickness.BorderBrush = Brushes.Red;
                return;
            }

            if(!double.TryParse(SheetPadding.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                MessageBox.Show(Strings.InvalidNumberFormat, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                SheetPadding.BorderBrush = Brushes.Red;
                return;
            }

            if(!double.TryParse(SheetPrice.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                MessageBox.Show(Strings.InvalidNumberFormat, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                SheetPrice.BorderBrush = Brushes.Red;
                return;
            }

            if(!double.TryParse(EdgeSealingPrice.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                MessageBox.Show(Strings.InvalidNumberFormat, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                EdgeSealingPrice.BorderBrush = Brushes.Red;
                return;
            }

            var selectedItem = (ComboBoxItem)Lang.SelectedItem;
            if (selectedItem != null)
            {
                string cultureCode = selectedItem.Tag.ToString();
                var culture = new CultureInfo(cultureCode);
                LocalizationManager.Instance.Culture = culture;
            }

            this.Close();
        }


    }
}
