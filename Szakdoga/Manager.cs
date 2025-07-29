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
        public void AddPiece( double height, double width,CutDirection cutDirection)
        {
            Piece piece = new Piece
            {
                Id = nextId++,
                Width = width,
                Height = height,
                CutDirection = cutDirection
            };
            Pieces.Add(piece);
        }

        public void UpdatePiece(int id, double height, double width, CutDirection cutDirection)
        {
            var piece = Pieces.FirstOrDefault(p => p.Id == id);
            if (piece != null)
            {
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
