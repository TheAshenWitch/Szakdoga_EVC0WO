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
using System.Windows.Threading;

namespace Szakdoga
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public static Settings settings;
        public static Statistics statistics;
        MainViewModel viewModel;
        Manager manager;

        int sheetId;
        double _pcw;
        double _pch;
        string OptMode;
        public MainWindow()
        {
            InitializeComponent();
            manager = new Manager();
            settings = new Settings();
            statistics = new Statistics();
            viewModel = new MainViewModel(manager.Pieces);

            DataContext = viewModel;
            sheetId = 1;
            
            SizeChanged += MainWindow_SizeChanged;
            SheetIdBox.Text = sheetId.ToString();

            _pch = PieceCanvas.Height;
            _pcw = PieceCanvas.Width;
            OptMode = "Test";
            //StateChanged += MainWindow_Maximized;
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

            public event PropertyChangedEventHandler? PropertyChanged;

            protected void OnPropertyChanged(string nev) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nev));
        }
        private void OptSelected(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
            {
                OptMode = rb.Content.ToString()!;
            }
        }
        private void Optimize(object sender, RoutedEventArgs e)
        {
            manager.optimize(OptMode, (double)settings.SheetWidth!, (double)settings.SheetHeight!,settings.SheetPadding,settings.BladeThickness);
            //manager.optimize("Guillotine", (double)settings.SheetWidth!, (double)settings.SheetHeight!,settings.SheetPadding,settings.BladeThickness);
            sheetId = 1;
            PlacePieces();
            SheetIdBox.Text = sheetId.ToString();
            statistics.CalculateStatistics(manager.Pieces, settings);
            statistics.CalculateStatisticsForSheet(manager.Pieces, settings, sheetId);
        }


        //ez amúgy nem kell
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Width == 0 || e.PreviousSize.Height == 0)
                return;

            //double wscale = _pcw / PieceCanvas.Width;// e.NewSize.Width / e.PreviousSize.Width;
            //double hscale = _pch / PieceCanvas.Height; // e.NewSize.Height / e.PreviousSize.Height;
            //_pcw = PieceCanvas.Width;
            //_pch = PieceCanvas.Height;
            //foreach (var child in PieceCanvas.Children.OfType<Rectangle>())
            //{
            //    double left = Canvas.GetLeft(child);
            //    double top = Canvas.GetTop(child);
            //    if (e.PreviousSize.Width != e.NewSize.Width)
            //    {
            //        child.Width = child.Width * wscale;
            //        child.Height = child.Height * wscale;
            //        Canvas.SetTop(child, top * wscale);
            //        Canvas.SetLeft(child, left * wscale);
            //    }
            //    else if (e.PreviousSize.Height != e.NewSize.Height)
            //    {
            //        child.Height = child.Height * hscale;
            //        child.Width = child.Width * hscale;
            //        Canvas.SetLeft(child, left * hscale);
            //        Canvas.SetTop(child, top * hscale);
            //    }
            //}
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
                FileStream fs = (FileStream)saveFileDialog.OpenFile();
                try
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        foreach (var piece in manager.Pieces)
                        {
                            if (piece.x == null || piece.y == null || piece.SheetId == null)
                                sw.WriteLine($"{piece.Id};{piece.Name};{piece.Height};{piece.Width};{piece.CutDirection}");
                            else
                                sw.WriteLine($"{piece.Id};{piece.Name};{piece.Height};{piece.Width};{piece.CutDirection};{Math.Round((double)piece.x,2)};{Math.Round((double)piece.y,2)};{piece.SheetId}");
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
            
            OpenFileDialog saveFileDialog = new OpenFileDialog();
            saveFileDialog.Filter = "Text File|*.txt";
            saveFileDialog.Title = "Save pieces list";
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                FileStream fs = (FileStream)saveFileDialog.OpenFile();
                using (StreamReader sr = new StreamReader(fs))
                {
                    manager.ClearPieces();
                    PieceCanvas.Children.Clear();
                    string line;
                    try
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            var parts = line.Split(';');
                            if (parts.Length == 5 && int.TryParse(parts[0], out int id) && double.TryParse(parts[2], out double height) && double.TryParse(parts[3], out double width))
                            {
                                CutDirection cutDirection = (CutDirection)Enum.Parse(typeof(CutDirection), parts[4]);
                                string name = parts[1];
                                manager.AddPiece(height, width, cutDirection, name, fromLoad: true);
                            }
                            else if(parts.Length == 8 && int.TryParse(parts[0], out int oid) && double.TryParse(parts[2], out double oheight) && double.TryParse(parts[3], out double owidth) && double.TryParse(parts[5], out double x) && double.TryParse(parts[6], out double y) && double.TryParse(parts[7], out double sheetId))
                            {
                                CutDirection cutDirection = (CutDirection)Enum.Parse(typeof(CutDirection), parts[4]);
                                string name = parts[1];
                                manager.AddPiece(oheight, owidth, cutDirection, name, fromLoad: true, optimised : true, x, y, sheetId);
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
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(settings);

            settingsWindow.Closed += (s, ev) =>
            {
                settings.SheetWidth = Convert.ToDouble(settingsWindow.SheetWidth.Text);
                settings.SheetHeight = Convert.ToDouble(settingsWindow.SheetHeight.Text);
                settings.BladeThickness = Convert.ToDouble(settingsWindow.BladeThickness.Text);
                settings.SheetPadding = Convert.ToDouble(settingsWindow.SheetPadding.Text);
                settings.SheetColor = settingsWindow.SheetColor.Text;
                settings.SheetManufacturer = settingsWindow.SheetManufacturer.Text;
                settings.SheetPrice = Convert.ToDouble(settingsWindow.SheetPrice.Text);
                settings.EdgeSealingPrice = Convert.ToDouble(settingsWindow.EdgeSealingPrice.Text);
            };
            
            settingsWindow.Show();

        }

        public void PlacePieces()
        {
            PieceCanvas.Children.Clear();
            
            double wscale = PieceCanvas.ActualHeight / (double)settings.SheetHeight;
            double hscale = PieceCanvas.ActualWidth / (double)settings.SheetWidth;
            foreach (var piece in manager.Pieces)
            {
                if (piece.SheetId == sheetId)
                {
                    Rectangle rect = new Rectangle
                    {
                        Width = (piece.Height * hscale),
                        Height = (piece.Width * wscale),
                        Stroke = Brushes.Black,
                        Fill = Brushes.LightGray,
                        StrokeThickness = 1,
                        ToolTip = $"ID: {piece.Id}\nName: {piece.Name}\nHeight: {piece.Height}\nWidth: {piece.Width}\nDirection: {piece.CutDirection}"
                    };
                    Canvas.SetLeft(rect, (double)(piece.x * hscale)!);
                    Canvas.SetTop(rect, (double)(piece.y * wscale)!);
                    
                    PieceCanvas.Children.Add(rect);
                }
            }
        }
        private void NextSheet(object sender, RoutedEventArgs e)
        {
            if (manager.Pieces.Max(p => p.SheetId) > sheetId)
            {
                sheetId++;
                PlacePieces();
                SheetIdBox.Text = sheetId.ToString();
                statistics.CalculateStatisticsForSheet(manager.Pieces, settings, sheetId);
            }
            else
            {
                sheetId = 1;
                PlacePieces();
                SheetIdBox.Text = sheetId.ToString();
                statistics.CalculateStatisticsForSheet(manager.Pieces, settings, sheetId);
            }
        }
        private void PrevSheet(object sender, RoutedEventArgs e)
        {
            if (sheetId > 1)
            {
                sheetId--;
                PlacePieces();
                SheetIdBox.Text = sheetId.ToString();
                statistics.CalculateStatisticsForSheet(manager.Pieces, settings, sheetId);
            }
            else
            {
                sheetId = manager.Pieces.Max(p => p.SheetId) ?? 1;
                PlacePieces();
                SheetIdBox.Text = sheetId.ToString();
                statistics.CalculateStatisticsForSheet(manager.Pieces, settings, sheetId);
            }
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
                PieceCanvas.Children.Clear();
            }
        }

    }  
}