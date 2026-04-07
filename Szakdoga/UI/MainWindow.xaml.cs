using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Szakdoga.Models;
using Szakdoga.Resources;
using Szakdoga.Services;
using static Szakdoga.App; 

namespace Szakdoga.UI
{
    public partial class MainWindow : Window
    {
        //db con string := DB_CONNECTION_STRING="Server=MSI\\LOCALDB;Database=RakLapDb;Trusted_Connection=True;TrustServerCertificate=True"
        //pc?              DB_CONNECTION_STRING="Server=MÁRK-PC\\LOCALDB;Database=szakdoga;Trusted_Connection=True;TrustServerCertificate=True"


        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        #pragma warning disable CA2211 // Non-constant fields should not be visible

        public static Settings settings;
        public static Statistics statistics;

        #pragma warning restore CA2211 // Non-constant fields should not be visible
        #pragma warning restore CS8618 // Will get initialized in constructor

        MainViewModel viewModel;
        Manager manager;
        DatabaseService DB;

        int sheetId;
        int orderId;
        double _pcw;
        double _pch;
        string OptMode;

        public MainWindow()
        {
            InitializeComponent();
            manager = new Manager();
            DB = new DatabaseService();

            try
            {
                settings = new Settings(
                        Properties.Settings.Default.Language,
                        Properties.Settings.Default.DarkMode,
                        2070.0,
                        2800.0,
                        Properties.Settings.Default.BladeThickness,
                        Properties.Settings.Default.SheetPadding,
                        Properties.Settings.Default.SheetColor,
                        Properties.Settings.Default.SheetManufacturer,
                        Properties.Settings.Default.SheetPrice,
                        Properties.Settings.Default.EdgeSealingPrice,
                        Properties.Settings.Default.Currency
                    );
            }
            catch (Exception)
            {
                settings = new Settings();
            }

            CultureInfo culture = new CultureInfo(settings.Language);
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;

            LocalizationManager.Instance.Culture = culture;

            statistics = new Statistics();
            viewModel = new MainViewModel(manager.Pieces);

            DataContext = viewModel;
            PiecesListView.DataContext = viewModel;
            sheetId = 1;
            SheetIdBox.Text = sheetId.ToString();

            _pch = PieceCanvas.Height;
            _pcw = PieceCanvas.Width;
            OptMode = "Test";

            PreviewMouseDown += (s, e) =>
            {
                bool isMouseOverRadioButton = RadioGrainButton.IsMouseOver || RadioCrossButton.IsMouseOver || RadioVarButton.IsMouseOver;
                bool isMouseOverListView = PiecesListView.IsMouseOver;
                bool isMouseOverInputFields = NameTxt.IsMouseOver || HeighgtTxt.IsMouseOver || WidthTxt.IsMouseOver || UpdateButton.IsMouseOver;

                if (!isMouseOverInputFields && !isMouseOverListView && !isMouseOverRadioButton)
                    PiecesListView.SelectedItem = null;
            };

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ClearInputs();

            PiecesListView.ItemsSource = viewModel.PieceList;
        }

        public MainWindow(int OrderId,List<Piece>? pieces, Sheet? orderSheet)
        {
            InitializeComponent();
            manager = new Manager();
            DB = new DatabaseService();

            orderId = OrderId;

            try
            {
                settings = new Settings(
                        Properties.Settings.Default.Language,
                        Properties.Settings.Default.DarkMode,
                        2070.0,
                        2800.0,
                        Properties.Settings.Default.BladeThickness,
                        Properties.Settings.Default.SheetPadding,
                        Properties.Settings.Default.SheetColor,
                        Properties.Settings.Default.SheetManufacturer,
                        Properties.Settings.Default.SheetPrice,
                        Properties.Settings.Default.EdgeSealingPrice,
                        Properties.Settings.Default.Currency
                    );
            }catch (Exception)
            {
                settings = new Settings();
            }

            if(orderSheet != null)
            {
                if(orderSheet.Height > 0)
                settings.SheetHeight = orderSheet.Height;
                if(orderSheet.Width > 0)
                    settings.SheetWidth = orderSheet.Width;
                if(orderSheet.Color != null)
                    settings.SheetColor = orderSheet.Color;
                if(orderSheet.Price != null)
                    settings.SheetPrice = orderSheet.Price;
            }

            CultureInfo culture = new CultureInfo(settings.Language);
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;

            LocalizationManager.Instance.Culture = culture;

            statistics = new Statistics();
            viewModel = new MainViewModel(manager.Pieces);

            DataContext = viewModel;
            PiecesListView.DataContext = viewModel;
            sheetId = 1;
            
            SheetIdBox.Text = sheetId.ToString();

            _pch = PieceCanvas.Height;
            _pcw = PieceCanvas.Width;
            OptMode = "Test";

            PreviewMouseDown += (s, e) =>
            {
                bool isMouseOverRadioButton = RadioGrainButton.IsMouseOver || RadioCrossButton.IsMouseOver || RadioVarButton.IsMouseOver;
                bool isMouseOverListView = PiecesListView.IsMouseOver;
                bool isMouseOverInputFields = NameTxt.IsMouseOver || HeighgtTxt.IsMouseOver || WidthTxt.IsMouseOver || CountTxt.IsMouseOver || UpdateButton.IsMouseOver;

                if (!isMouseOverInputFields && !isMouseOverListView && !isMouseOverRadioButton)
                    PiecesListView.SelectedItem = null;
            };

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ClearInputs();

            PiecesListView.ItemsSource = viewModel.PieceList;

            if (pieces != null)
            {
                foreach (var piece in pieces)
                {
                    if (piece.x != null && piece.y != null && piece.SheetId != null)
                        manager.AddPiece(piece.Height, piece.Width, piece.CutDirection, piece.Name, 1, true, true, (double)piece.x, (double)piece.y, (double)piece.SheetId);
                    else
                        manager.AddPiece(piece.Height, piece.Width, piece.CutDirection, piece.Name, fromLoad:true);
                }
               
            }
        }

        public void WindowLoaded(object sender, RoutedEventArgs e)
        {
            bool hasOptimizedPieces = manager.Pieces.Any(p => p.x != null && p.y != null && p.SheetId != null);
            if (hasOptimizedPieces)
            {
                PlacePieces();
                SheetIdBox.Text = "1";
                FillStatistics();
                FillStatisticsThisSheet();
            }
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
            if (!manager.Optimize(OptMode, (double)settings.SheetWidth!, (double)settings.SheetHeight!, settings.SheetPadding, settings.BladeThickness))
                return;
            sheetId = 1;
            PlacePieces();
            SheetIdBox.Text = sheetId.ToString();
            FillStatistics();
            FillStatisticsThisSheet();
        }

        private void Export(object sender, RoutedEventArgs e)
        {
            if (PieceCanvas.Children.Count == 0)
            {
                MessageBox.Show(Strings.ExportError, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int maxSheet = (int)manager.Pieces.Max(m => m.SheetId)!;
            SetSheet(1);
            double scalePercent = 0.7;     // 70% méret
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
                        (int)canvasWidth + 10,
                        (int)canvasHeight + 10,
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
                        int columns = (int)Math.Ceiling((double)totalItems/80);           

                        int rowsPerColumn = (int)Math.Ceiling((double)totalItems / columns);

                        XFont font = new XFont("Arial", 9, XFontStyle.Regular);
                        double lineSpacing = font.Size * 1.2;

                        double listOffsetX = offsetX + scaledWidth + 10; // kezdő X koordináta
                        double listOffsetY = offsetY + 10;

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
                    Title = Strings.CutDiagramTitle
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write))
                    {
                        pdf.Save(fs);
                    }
                }
            }

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            GC.WaitForPendingFinalizers();
            GC.Collect();
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

        private void PiecesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NameTxt.Text = string.Empty;
            HeighgtTxt.Text = string.Empty;
            WidthTxt.Text = string.Empty;


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
            var result = MessageBox.Show(Strings.SaveToTxtOrOnlyDb, Strings.SavePromptText, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            
            if(result == MessageBoxResult.Cancel)
                return;

            DB.AddOrderPieces(Piece.PiecesToOrderPieces(manager.GetPiecesList(), orderId));

            if (result == MessageBoxResult.Yes)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text File|*.txt";
                saveFileDialog.Title = Strings.SavePromptText;
                saveFileDialog.ShowDialog();
                if (saveFileDialog.FileName != "")
                {
                    using (FileStream fs = (FileStream)saveFileDialog.OpenFile())
                    {
                        try
                        {
                            using (StreamWriter sw = new StreamWriter(fs))
                            {
                                foreach (var piece in manager.Pieces)
                                {
                                    if (piece.x == null || piece.y == null || piece.SheetId == null)
                                        sw.WriteLine($"{piece.Id};{piece.Name};{piece.Height};{piece.Width};{piece.CutDirection}");
                                    else
                                        sw.WriteLine($"{piece.Id};{piece.Name};{piece.Height};{piece.Width};{piece.CutDirection};{Math.Round((double)piece.x, 2)};{Math.Round((double)piece.y, 2)};{piece.SheetId}");
                                }
                            }
                            MessageBox.Show(Strings.SaveSuccessText, Strings.Success, MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{Strings.SaveErrorText}: {ex.Message}", Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        fs.Close();
                    }
                }
            }
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            OpenFileDialog saveFileDialog = new OpenFileDialog();
            saveFileDialog.Filter = "Text File|*.txt";
            saveFileDialog.Title =  Strings.LoadPromptText;
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                using (FileStream fs = (FileStream)saveFileDialog.OpenFile()) {

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
                                    CutDirection cutDirection;
                                    try
                                    {
                                        cutDirection = (CutDirection)Enum.Parse(typeof(CutDirection), parts[4]);
                                    }
                                    catch (Exception)
                                    {
                                        cutDirection = CutDirection.Szálirány;
                                    }
                                    string name = parts[1];
                                    manager.AddPiece(height, width, cutDirection, name, fromLoad: true);
                                }
                                else if (parts.Length == 8 && int.TryParse(parts[0], out int oid) && double.TryParse(parts[2], out double oheight) && double.TryParse(parts[3], out double owidth) && double.TryParse(parts[5], out double x) && double.TryParse(parts[6], out double y) && double.TryParse(parts[7], out double sheetId))
                                {
                                    CutDirection cutDirection;
                                    try
                                    {
                                        cutDirection = (CutDirection)Enum.Parse(typeof(CutDirection), parts[4]);
                                    }
                                    catch(Exception)
                                    {
                                        cutDirection = CutDirection.Szálirány;
                                    }
                                    string name = parts[1];
                                    manager.AddPiece(oheight, owidth, cutDirection, name, 1, fromLoad: true, optimised: true, x, y, sheetId);
                                }
                            }
#pragma warning restore CS8600 // sr.Readline() could return null
                            ClearStatistics();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{Strings.LoadErrorText}: {ex.Message}", Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        fs.Close();
                    }
                }
            }
            bool hasOptimizedPieces = manager.Pieces.Any(p => p.x != null && p.y != null && p.SheetId != null);
            if (hasOptimizedPieces)
            {
                PlacePieces();
                SheetIdBox.Text = "1";
                FillStatistics();
                FillStatisticsThisSheet();
            }
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(settings);

            settingsWindow.Closed += (s, ev) =>
            {
                double.TryParse(settingsWindow.SheetWidth.Text, out double _sheetWidth);
                double.TryParse(settingsWindow.SheetHeight.Text, out double _sheetHeight);
                double.TryParse(settingsWindow.BladeThickness.Text, out double _bladeThickness);
                double.TryParse(settingsWindow.SheetPadding.Text, out double _sheetPadding);
                double.TryParse(settingsWindow.SheetPrice.Text, out double _sheetPrice);
                double.TryParse(settingsWindow.EdgeSealingPrice.Text, out double _edgeSealingPrice);

                settings.SheetWidth = _sheetWidth;
                settings.SheetHeight = _sheetHeight;
                settings.BladeThickness = _bladeThickness;
                settings.SheetPadding = _sheetPadding;
                settings.SheetColor = settingsWindow.SheetColor.Text;
                settings.SheetManufacturer = settingsWindow.SheetManufacturer.Text;
                settings.SheetPrice = _sheetPrice;
                settings.EdgeSealingPrice = _edgeSealingPrice;

                var selectedItem = (ComboBoxItem)settingsWindow.Lang.SelectedItem;
                if (selectedItem != null)
                {
                    string cultureCode = selectedItem.Tag.ToString()!;
                    CultureInfo culture = new CultureInfo(cultureCode);
                    Thread.CurrentThread.CurrentUICulture = culture;
                    Thread.CurrentThread.CurrentCulture = culture;
                    settings.Language = cultureCode;

                    LocalizationManager.Instance.Culture = culture;

                    CollectionViewSource.GetDefaultView(PiecesListView.ItemsSource).Refresh();
                }

                Properties.Settings.Default.Language = settings.Language;
                Properties.Settings.Default.DarkMode = settings.DarkMode;
                Properties.Settings.Default.SheetHeight = settings.SheetHeight ?? 2070.0;
                Properties.Settings.Default.SheetWidth = settings.SheetWidth ?? 2800.0;
                Properties.Settings.Default.BladeThickness = settings.BladeThickness;
                Properties.Settings.Default.SheetPadding = settings.SheetPadding;
                Properties.Settings.Default.SheetColor = settings.SheetColor;
                Properties.Settings.Default.SheetManufacturer = settings.SheetManufacturer;
                Properties.Settings.Default.SheetPrice = settings.SheetPrice ?? 10000.0;
                Properties.Settings.Default.EdgeSealingPrice = settings.EdgeSealingPrice ?? 150.0;
                Properties.Settings.Default.Currency = settings.Currency;
                Properties.Settings.Default.Save();
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

                    string virtualPieceName = FormatPieceName(piece);
                    int fontSize = CalculateFontSize(piece);

                    Label label = new Label
                    {
                        Content = virtualPieceName,
                        FontSize = fontSize,
                        Foreground = System.Windows.Media.Brushes.Black,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    Canvas.SetLeft(rect, (double)(piece.x * hscale)!);
                    Canvas.SetTop(rect, (double)(piece.y * wscale)!);
                    Canvas.SetLeft(label, (double)(piece.x * hscale)!);
                    Canvas.SetTop(label, (double)(piece.y * wscale)!);
                    PieceCanvas.Children.Add(rect);
                    PieceCanvas.Children.Add(label);
                }
            }
        }

        private string FormatPieceName(Piece piece)
        {
            if (piece.Name == null)
                return $"{piece.Height}x{piece.Width}";

            string baseName = $"{piece.Name}\n{piece.Height}x{piece.Width}";
            
            if (piece.Name!.Length <= 10)
                return baseName;

            if (piece.Width >= 150 && piece.Height <= 350)
            {
                int halfLength = piece.Name.Length / 2;
                return $"{piece.Name.Substring(0, (int)Math.Floor((double)halfLength))}\n" +
                       $"{piece.Name.Substring(halfLength, (int)Math.Ceiling((double)(piece.Name.Length - halfLength)))}\n" +
                       $"{piece.Height}x{piece.Width}";
            }

            return baseName;
        }

        private int CalculateFontSize(Piece piece)
        {
            int baseSize = 14;
            
            if (piece.Name!.Length <= 10)
                return baseSize;

            if (piece.Width >= 150 && piece.Height <= 350)
                return baseSize;

            if (piece.Name.Length > 20)
            {
                return piece.Height switch
                {
                    <= 100 => 5,
                    <= 150 => 6,
                    <= 200 => 7,
                    <= 250 => 8,
                    _ => baseSize
                };
            }

            return piece.Height switch
            {
                <= 100 => 6,
                <= 150 => 8,
                <= 200 => 10,
                <= 250 => 12,
                _ => baseSize
            };
        }
        
        private void NextSheet(object sender, RoutedEventArgs e)
        {
            if (PieceCanvas.Children.Count == 0)
            {
                sheetId = 1;
                return;
            }
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
            if (PieceCanvas.Children.Count == 0)
            {
                sheetId = 1;
                return;
            }
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
            if (PieceCanvas.Children.Count == 0)
            {
                sheetId = 1;
                return;
            }
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

        private void SetSheet(int _sheetId)
        {
            if(manager.Pieces.Count == 0)
            {
                sheetId = 1;
                return;
            }
            if (_sheetId >= 1 && _sheetId <= (manager.Pieces.Max(p => p.SheetId) ?? 1))
            {
                sheetId = _sheetId;
                PlacePieces();
                SheetIdBox.Text = sheetId.ToString();
                statistics.CalculateStatisticsForSheet(manager.Pieces, settings, sheetId);
                FillStatisticsThisSheet();
            }
            
        }

        private void Add(object sender, RoutedEventArgs e)
        {
            if(HeighgtTxt.Text == string.Empty || WidthTxt.Text == string.Empty)
            {
                MessageBox.Show(Strings.AddHeightWidthPrompt, Strings.Warning, MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }
            string name= NameTxt.Text.Trim();
            double height;
            double width;
            int count;
            try
            {
                width = Convert.ToDouble(WidthTxt.Text.Replace('.',','));
            }
            catch (FormatException)
            {
                MessageBox.Show(Strings.InvalidNumberWidth, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                height = Convert.ToDouble(HeighgtTxt.Text.Replace('.',','));
            }
            catch (FormatException)
            {
                MessageBox.Show(Strings.InvalidNumberHeight, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                count = Convert.ToInt32(CountTxt.Text);
                
            }
            catch (FormatException)
            {
                MessageBox.Show(Strings.InvalidNumberCount, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            CutDirection direction = viewModel.Direction;
            manager.AddPiece(height, width, direction, name, count);
            ClearInputs();
        }

         public void Update(object sender, RoutedEventArgs e)
         {
            if (PiecesListView.SelectedItem is Piece selectedPiece)
            {
                if (HeighgtTxt.Text == string.Empty || WidthTxt.Text == string.Empty)
                {
                    MessageBox.Show(Strings.AddHeightWidthPrompt, Strings.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    MessageBox.Show(Strings.InvalidNumberWidth, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                try
                {
                    height = Convert.ToDouble(HeighgtTxt.Text.Replace('.', ','));
                }
                catch (FormatException)
                {
                    MessageBox.Show(Strings.InvalidNumberHeight, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                CutDirection direction = viewModel.Direction;
                manager.UpdatePiece(selectedPiece.Id ?? 0, height, width, direction, name);
                // Refresh the ListView to reflect the change
                PiecesListView.Items.Refresh();
                PiecesListView.SelectedItem = null;
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
            if (MessageBox.Show(Strings.ConfirmClearText, Strings.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                manager.ClearPieces();
                PieceCanvas.Children.Clear();
                ClearStatistics();
            }
        }

        public void ClearInputs()
        {
            NameTxt.Text = string.Empty;
            HeighgtTxt.Text = string.Empty;
            WidthTxt.Text = string.Empty;
            CountTxt.Text = "1";
            viewModel.Direction = CutDirection.Szálirány; // Reset to default direction
        }

        public void BackToExplorer(object sender, RoutedEventArgs e)
        {
            ProjectExplorer projectExplorer = new ProjectExplorer();
            projectExplorer.Show();

            this.Close();
        }
    }  
}