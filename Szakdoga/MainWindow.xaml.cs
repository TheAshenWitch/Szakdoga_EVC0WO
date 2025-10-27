using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Resources;
using System.Globalization;
using System.Threading;

namespace Szakdoga
{
    public partial class MainWindow : Window
    {
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        #pragma warning disable CA2211 // Non-constant fields should not be visible
        
        public static Settings settings;
        public static Statistics statistics;

        #pragma warning restore CA2211 // Non-constant fields should not be visible
        #pragma warning restore CS8618 // Will get initialized in constructor

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

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
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
                OptMode = rb.Tag.ToString()!;
            }
        }
        
        private void Optimize(object sender, RoutedEventArgs e)
        {
            manager.optimize(OptMode, (double)settings.SheetWidth!, (double)settings.SheetHeight!,settings.SheetPadding,settings.BladeThickness);
            //manager.optimize("Guillotine", (double)settings.SheetWidth!, (double)settings.SheetHeight!,settings.SheetPadding,settings.BladeThickness);
            sheetId = 1;
            PlacePieces();
            SheetIdBox.Text = sheetId.ToString();
            try
            {
                FillStatistics();
                FillStatisticsThisSheet();
            }
            catch (Exception)
            {}
            //statistics.CalculateStatistics(manager.Pieces, settings);
            //statistics.CalculateStatisticsForSheet(manager.Pieces, settings, sheetId);
        }

        private void Export(object sender, RoutedEventArgs e)
        {
            if (PieceCanvas.Children.Count == 0)
            {
                MessageBox.Show("To export, optimize first", "Export error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int maxSheet = (int)manager.Pieces.Max(m => m.SheetId)!;
            sheetId = 1;
            double scalePercent = 0.7;     // 80% méret
            double offsetXPercent = 0.01;  // 1% balról
            double offsetYPercent = 0.01;  // 1% felülről
            ProgressBarWindow progbar = new ProgressBarWindow(1, maxSheet);
            progbar.Show();
            using (PdfDocument pdf = new PdfDocument())
            {
                for (int i = 1; i <= maxSheet; i++)
                {
                    double canvasWidth = PieceCanvas.ActualWidth;
                    double canvasHeight = PieceCanvas.ActualHeight;

                    RenderTargetBitmap? rtb = new RenderTargetBitmap(
                        (int)canvasWidth,
                        (int)canvasHeight,
                        96, 96,
                        PixelFormats.Pbgra32);

                    // Ideiglenes cache a gyorsabb renderhez
                    var oldCache = PieceCanvas.CacheMode;
                    PieceCanvas.CacheMode = new BitmapCache();

                    // FONTOS: Canvas teljes elrendezés frissítése
                    PieceCanvas.Measure(new System.Windows.Size(PieceCanvas.ActualWidth, PieceCanvas.ActualHeight));
                    PieceCanvas.Arrange(new System.Windows.Rect(0, 0, PieceCanvas.ActualWidth, PieceCanvas.ActualHeight));
                    PieceCanvas.UpdateLayout();

                    // Render a bitmapre
                    rtb.Render(PieceCanvas);

                    PieceCanvas.CacheMode = oldCache;


                    PdfPage page = pdf.AddPage();
                    page.Width = canvasWidth;
                    page.Height = canvasHeight;
                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    using (XImage img = XImage.FromBitmapSource(rtb))
                    {
                        // Skálázás a PDF oldalon
                        double scaledWidth = page.Width * scalePercent;
                        double scaledHeight = page.Height * scalePercent;

                        double offsetX = page.Width * offsetXPercent;
                        double offsetY = page.Height * offsetYPercent;

                        gfx.DrawImage(img, offsetX, offsetY, scaledWidth, scaledHeight);

                        var sheetPieces = manager.Pieces.Where(p => p.SheetId == sheetId).ToList();
                        int totalItems = sheetPieces.Count;

                        // Oszlopszám meghatározása
                        int columns = 1;
                        if (totalItems > 150)
                            columns = 3;
                        else if (totalItems > 75)
                            columns = 2;

                        int rowsPerColumn = (int)Math.Ceiling((double)totalItems / columns);

                        XFont font = new XFont("Arial", 9, XFontStyle.Regular);
                        double lineSpacing = font.Size * 1.2;

                        double listOffsetX = offsetX + scaledWidth + 10; // kezdő X koordináta
                        double listOffsetY = offsetY;

                        for (int j = 0; j < totalItems; j++)
                        {
                            int column = j / rowsPerColumn;
                            int row = j % rowsPerColumn;

                            double x = listOffsetX + column * 150; // 150 pont távolság oszloponként, állítható
                            double y = listOffsetY + row * lineSpacing;

                            var piece = sheetPieces[j];
                            string line = $"{piece.Name}  W:{piece.Width}  H:{piece.Height}";

                            gfx.DrawString(line, font, XBrushes.Black, new XPoint(x, y));
                        }
                    }

                    // RenderTargetBitmap elengedése memória felszabadításra
                    rtb.Clear();
                    rtb = null;

                    progbar.SetProgress(sheetId);
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Background);
                    NextSheet();
                }
                progbar.Close();
                // Mentés
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF File|*.pdf",
                    Title = "Cut diagram"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write))
                    {
                        pdf.Save(fs);
                    }
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        
        private void FillStatistics()
        {
            statistics.CalculateStatistics(manager.Pieces, settings);
            NoOfSheets.Text = statistics.NumberOfSheets.ToString();
            NoOfPices.Text = statistics.NumberOfPieces.ToString();
            TotalAreaOfPieces.Text = statistics.PiecesArea.ToString() + " m2";
            TotalWasteArea.Text = statistics.WasteArea.ToString() + " m2";
            MaterialUtilization.Text = statistics.MaterialUtilization.ToString() + " %";
            TotalCutLength.Text = statistics.TotalCutLength.ToString() + " m";
            EdgeSealingNeeded.Text = statistics.EdgeSealingNeeded.ToString() + " m";
            TotalSheetCost.Text = statistics.TotalSheetCost.ToString() + " " + settings.Currency;
            TotalEdgeSealingCost.Text = statistics.TotalEdgeSealingCost.ToString() + " " + settings.Currency;
            TotalCost.Text = statistics.TotalCost.ToString() + " " + settings.Currency;
        }
        
        private void FillStatisticsThisSheet()
        {
            statistics.CalculateStatisticsForSheet(manager.Pieces, settings,sheetId);
            PiecesThisSheet.Text = statistics.PiecesThisSheet.ToString();
            MaterialUtilThisSheet.Text = statistics.MaterialUtilizationThisSheet.ToString() + " %";
            WasteAreaThisShet.Text = statistics.WasteAreaThisSheet.ToString() + " m2";
        }
        
        private void ClearStatistics()
        {
            statistics.ClearStatistics();
            NoOfSheets.Clear();
            NoOfPices.Clear();
            TotalAreaOfPieces.Clear();
            TotalWasteArea.Clear();
            MaterialUtilization.Clear();
            TotalCutLength.Clear();
            EdgeSealingNeeded.Clear();
            TotalSheetCost.Clear();
            TotalEdgeSealingCost.Clear();
            TotalCost.Clear();
            PiecesThisSheet.Clear();
            MaterialUtilThisSheet.Clear();
            WasteAreaThisShet.Clear();
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
                        #pragma warning disable CS8600 // sr.Readline() could return null
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
                        #pragma warning restore CS8600 // sr.Readline() could return null
                        ClearStatistics();
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
                var selectedItem = (ComboBoxItem)settingsWindow.Lang.SelectedItem;
                if (selectedItem != null)
                {
                    string cultureCode = selectedItem.Tag.ToString();
                    CultureInfo culture = new CultureInfo(cultureCode);
                    Thread.CurrentThread.CurrentUICulture = culture;
                    Thread.CurrentThread.CurrentCulture = culture;
                }
            };

            settingsWindow.ShowDialog();

        }

        public void PlacePieces()
        {
            PieceCanvas.Children.Clear();
            
            double wscale = PieceCanvas.ActualHeight / (double)(settings.SheetHeight ?? 2070.0);
            double hscale = PieceCanvas.ActualWidth / (double)(settings.SheetWidth ?? 2800.0);
            foreach (var piece in manager.Pieces)
            {
                if (piece.SheetId == sheetId)
                {
                    System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle
                    {
                        Width = (piece.Height * hscale),
                        Height = (piece.Width * wscale),
                        Stroke = System.Windows.Media.Brushes.Black,
                        Fill = System.Windows.Media.Brushes.LightGray,
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
            FillStatisticsThisSheet();
        }

        private void NextSheet()
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
            FillStatisticsThisSheet();
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
            FillStatisticsThisSheet();
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
                ClearStatistics();
            }
        }

    }  
}