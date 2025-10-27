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
            // Alapértelmezett nyelv kiválasztása
            if (LocalizationManager.Instance.Culture.Name.StartsWith("hu"))
                Lang.SelectedIndex = 0;
            else
                Lang.SelectedIndex = 1;
        }
        private void Apply_Click(object sender, RoutedEventArgs e)
        {
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
