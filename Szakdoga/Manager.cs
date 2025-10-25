using System.Collections.ObjectModel;
using System.Windows;

namespace Szakdoga
{
    class Manager {
        public ObservableCollection<Piece> Pieces { get; }
       
        private int nextId;
        public Manager()
        {
            Pieces = new ObservableCollection<Piece>();
            nextId = 1;
        }
        public void AddPiece(double height, double width, CutDirection cutDirection, string? name, bool fromLoad = false, bool optimised = false, double x = 0, double y = 0, double sheetId = 0)
        {
            if ((Pieces.Any(p => p.Height == height && p.Width == width && p.CutDirection == cutDirection && p.Name == name) && !fromLoad && !optimised)
                && MessageBox.Show("A piece with the same dimensions and cut direction already exists. Continue?", "Duplicate Piece", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No ) 
            {
                return;
            }
            if(fromLoad && optimised)
            {
                Piece optimisedPiece = new Piece
                {
                    Name = name,
                    Id = nextId++,
                    Width = width,
                    Height = height,
                    CutDirection = cutDirection,
                    VirtualCutDirection = cutDirection,
                    x = x,
                    y = y,
                    SheetId = (int)sheetId
                };
                Pieces.Add(optimisedPiece);
                return;
            }
            if (height <= 0 || width <= 0)
            {
                MessageBox.Show("Height and width must be greater than zero.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (name == "" && name == string.Empty)
            {
                name = "Unknown";
            }

            Piece piece = new Piece
            {
                Name = name,
                Id = nextId++,
                Width = width,
                Height = height,
                CutDirection = cutDirection,
                VirtualCutDirection = cutDirection
            };
            Pieces.Add(piece);
        }

        public void UpdatePiece(int id, double height, double width, CutDirection cutDirection, string? name)
        {
            var piece = Pieces.FirstOrDefault(p => p.Id == id);
            if (piece != null)
            {
                piece.Name = name ?? piece.Name;
                piece.Height = height;
                piece.Width = width;
                piece.CutDirection = cutDirection;
            }
        }
        public List<Piece> GetPiecesList()
        {
            return Pieces.ToList();
        }
        public ObservableCollection<Piece> GetPieces()
        {
            return Pieces;
        }
        public void RemovePiece(int id)
        {
            var piece = Pieces.FirstOrDefault(p => p.Id == id);
            if (piece != null)
            {
                Pieces.Remove(piece);
            }
        }
        public void ClearPieces()
        {
            Pieces.Clear();
            nextId = 1; // Reset ID counter
        } 
        public void optimize(string method, double sheetWidth, double sheetHeight, double sheetPadding, double bladeThickness)
        {
            if (Pieces.Count == 0)
            {
                MessageBox.Show("No pieces to optimize.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            foreach (var piece in Pieces)
            {
                piece.x = null;
                piece.y = null;
                piece.SheetId = null;
            }
            Optimizer optimizer = new Optimizer();
            List<Piece> optimizedPieces = method switch
            {
                "Test" => optimizer.Test(GetPiecesList(), sheetWidth, sheetHeight, sheetPadding, bladeThickness),
                "Guillotine" => optimizer.Guillotine(GetPiecesList(), sheetWidth, sheetHeight, sheetPadding, bladeThickness),
                "Heuristic" => optimizer.Heuristic(GetPiecesList(), sheetWidth, sheetHeight, sheetPadding, bladeThickness),
                _ => throw new ArgumentException("Invalid optimization method"),
            };
            Pieces.Clear();
            foreach (var piece in optimizedPieces)
            {
                Pieces.Add(piece);
            }
        }
    }
}
