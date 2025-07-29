using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
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
            double height = Convert.ToDouble(HeighgtTxt.Text.Replace('.',','));
            double width = Convert.ToDouble(WidthTxt.Text.Replace('.',','));
            CutDirection direction = viewModel.Direction;
            manager.AddPiece(height, width, direction);
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
                double height = Convert.ToDouble(HeighgtTxt.Text.Replace('.', ','));
                double width = Convert.ToDouble(WidthTxt.Text.Replace('.', ','));
                CutDirection direction = viewModel.Direction;
                Console.WriteLine($"Updating piece {selectedPiece.Id} with Height: {height}, Width: {width}, Direction: {direction}");
                manager.UpdatePiece(selectedPiece.Id ?? 0, height, width, direction);
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
            HeighgtTxt.Text = string.Empty;
            WidthTxt.Text = string.Empty;
            viewModel.Direction = 0; // Reset to default direction

            if (PiecesListView.SelectedItem is Piece selectedPiece)
            {
                HeighgtTxt.Text = selectedPiece.Height.ToString();
                WidthTxt.Text = selectedPiece.Width.ToString();
                viewModel.Direction = selectedPiece.CutDirection;
            }
        }
    }  
}