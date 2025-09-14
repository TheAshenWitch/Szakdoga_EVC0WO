using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Szakdoga
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        MainViewModel viewModel;
        Manager manager;
        public MainWindow()
        {
            InitializeComponent();
            manager = new Manager();
            viewModel = new MainViewModel(manager.Pieces);
            this.DataContext = viewModel;
            
        }
        public class MainViewModel(ObservableCollection<Piece> pieces) : INotifyPropertyChanged
        {
            private CutDirection _direction;

            public CutDirection Direction
            {
                get => _direction;
                set
                {
                    if (_direction != value)
                    {
                        _direction = value;
                        OnPropertyChanged(nameof(Direction));
                    }
                }
            }
            public ObservableCollection<Piece> PieceList => pieces;

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string nev) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nev));
        }
       
        private void Add(object sender, RoutedEventArgs e)
        {

            if(HeighgtTxt.Text == string.Empty || WidthTxt.Text == string.Empty)
            {
                MessageBox.Show("Please enter both height and width.","Warning",MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }
            string name= NameTxt.Text.Trim();
            double height;
            double width;
            try
            {
                width = Convert.ToDouble(WidthTxt.Text.Replace('.',','));
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid width format. Please enter a valid number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                height = Convert.ToDouble(HeighgtTxt.Text.Replace('.',','));
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid height format. Please enter a valid number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            CutDirection direction = viewModel.Direction;
            manager.AddPiece(height, width, direction, name);
        }

         public void Update(object sender, RoutedEventArgs e)
         {
            if (PiecesListView.SelectedItem is Piece selectedPiece)
            {
                if (HeighgtTxt.Text == string.Empty || WidthTxt.Text == string.Empty)
                {
                    MessageBox.Show("Please enter both height and width.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                string name = NameTxt.Text.Trim();
                double height; 
                double width;
                try
                {
                    width = Convert.ToDouble(WidthTxt.Text.Replace('.', ','));
                }
                catch (FormatException)
                {
                    MessageBox.Show("Invalid width format. Please enter a valid number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                try
                {
                    height = Convert.ToDouble(HeighgtTxt.Text.Replace('.', ','));
                }
                catch (FormatException)
                {
                    MessageBox.Show("Invalid height format. Please enter a valid number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                CutDirection direction = viewModel.Direction;
                manager.UpdatePiece(selectedPiece.Id ?? 0, height, width, direction, name);
                // Refresh the ListView to reflect the change
                PiecesListView.Items.Refresh();
            }
        }

        public void Delete(object sender, RoutedEventArgs e)
        {
            if (PiecesListView.SelectedItem is Piece selectedPiece)
            {
                manager.RemovePiece(selectedPiece.Id ?? 0);
            }
        }

        public void Clear(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear all pieces?", "Confirm Clear", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                manager.ClearPieces();
            }
        }

        private void PiecesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NameTxt.Text = string.Empty;
            HeighgtTxt.Text = string.Empty;
            WidthTxt.Text = string.Empty;
            viewModel.Direction = 0; // Reset to default direction

            if (PiecesListView.SelectedItem is Piece selectedPiece)
            {
                NameTxt.Text = selectedPiece.Name ?? string.Empty;
                HeighgtTxt.Text = selectedPiece.Height.ToString();
                WidthTxt.Text = selectedPiece.Width.ToString();
                viewModel.Direction = selectedPiece.CutDirection;
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text File|*.txt";
            saveFileDialog.Title = "Save pieces list";
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog.OpenFile();
                try
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        foreach (var piece in manager.Pieces)
                        {
                            sw.WriteLine($"{piece.Id},{piece.Name},{piece.Height},{piece.Width},{piece.CutDirection}");
                        }
                    }
                    MessageBox.Show("Pieces saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving pieces: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                fs.Close();
            }
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            manager.ClearPieces();
            OpenFileDialog saveFileDialog = new OpenFileDialog();
            saveFileDialog.Filter = "Text File|*.txt";
            saveFileDialog.Title = "Save pieces list";
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog.OpenFile();
                using (StreamReader sr = new StreamReader(fs))
                {
                    string line;
                    try
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            var parts = line.Split(',');
                            if (parts.Length == 5 && int.TryParse(parts[0], out int id) && double.TryParse(parts[2], out double height) && double.TryParse(parts[3], out double width))
                            {
                                CutDirection cutDirection = (CutDirection)Enum.Parse(typeof(CutDirection), parts[4]);
                                string name = parts[1];
                                manager.AddPiece(height, width, cutDirection, name, fromLoad: true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading pieces: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    fs.Close();
                }
            }
            //try
            //{
            //    if (!File.Exists("pieces.txt"))
            //    {
            //        MessageBox.Show("No saved pieces found.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            //        return;
            //    }
            //    using (StreamReader sr = new StreamReader("pieces.txt"))
            //    {
            //        string line;
            //        while ((line = sr.ReadLine()) != null)
            //        {
            //            var parts = line.Split(',');
            //            if (parts.Length == 5 && int.TryParse(parts[0], out int id) && double.TryParse(parts[2], out double height) && double.TryParse(parts[3], out double width))
            //            {
            //                CutDirection cutDirection = (CutDirection)Enum.Parse(typeof(CutDirection), parts[4]);
            //                string name = parts[1];
            //                manager.AddPiece(height, width, cutDirection, name, fromLoad: true);
            //            }
            //        }
            //    }
            //    MessageBox.Show("Pieces loaded successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Error loading pieces: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            settings.Show();
        }
    }  
}