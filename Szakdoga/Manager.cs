using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Szakdoga.MainWindow;

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
        public void AddPiece(double height, double width, CutDirection cutDirection, string? name, bool fromLoad = false)
        {
            if ((Pieces.Any(p => p.Height == height && p.Width == width && p.CutDirection == cutDirection) && !fromLoad)
                && MessageBox.Show("A piece with the same dimensions and cut direction already exists. Continue?", "Duplicate Piece", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No ) 
            {
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
                CutDirection = cutDirection
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
    }
}
