using System.Collections.ObjectModel;
using System.Windows;
using Szakdoga.Resources;

namespace Szakdoga.Services
{
    class Manager {
        public ObservableCollection<Piece> Pieces { get; set; }
       
        private int nextId;

        public Manager()
        {
            Pieces = new ObservableCollection<Piece>();
            nextId = 1;
        }

        public void AddPiece(double height, double width, CutDirection cutDirection, string? name, int count = 1, bool fromLoad = false, bool optimised = false, double x = 0, double y = 0, double sheetId = 0)
        {
            bool duplicateExists = Pieces.Any(p => p.Height == height && p.Width == width && p.CutDirection == cutDirection && p.Name == name);
            if (duplicateExists && count == 1 && !fromLoad && !optimised && MessageBox.Show(Strings.SamePiecePromt, Strings.Confirm, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No ) 
            {
                return;
            }

            if(fromLoad && optimised)
            {
                if (name == "" || name == string.Empty)
                    name = Strings.UnknownPieceName;

                Piece optimizedPiece = new Piece
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
                Pieces.Add(optimizedPiece);
                return;
            }

            if (height <= 0 || width <= 0)
            {
                MessageBox.Show(Strings.DimensionErrorText, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            for (int i = 0; i < count; i++)
            {
                if (name == "" || name == string.Empty)
                    name = Strings.UnknownPieceName;

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

        public bool Optimize(string method, double sheetWidth, double sheetHeight, double sheetPadding, double bladeThickness)
        {
            if (Pieces.Count == 0)
            {
                MessageBox.Show(Strings.EmptyListError, Strings.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
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
                _ => throw new ArgumentException("Invalid optimization method"),
            };
            Pieces.Clear();
            foreach (var piece in optimizedPieces)
            {
                Pieces.Add(piece);
            }
            return true;
        }
    }
}
