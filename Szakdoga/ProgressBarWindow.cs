using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Szakdoga
{
    internal class ProgressBarWindow : Window
    {
        private ProgressBar progressBar;

        public ProgressBarWindow(int start, int end)
        {
            Title = "Exporting";
            Width = 300;
            Height = 100;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Layout elem (Grid vagy StackPanel)
            var grid = new Grid
            {
                Margin = new Thickness(10)
            };

            // ProgressBar létrehozása
            progressBar = new ProgressBar
            {
                Minimum = start,
                Maximum = end,
                Height = 25,
                Value = 0,
                Foreground = Brushes.DodgerBlue,
                IsIndeterminate = false 
            };

            grid.Children.Add(progressBar);

            Content = grid;
        }

        public void SetProgress(double value)
        {
            progressBar.Value = value;
        }
    }
}

