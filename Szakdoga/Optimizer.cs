using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Szakdoga
{
    internal class Optimizer
    {
        public List<Piece> Test(List<Piece> Pieces, double SheetX = 2800, double SheetY = 2070, double SheetPadding = 10, double BladeThickness = 3)
        {
            List<Piece> PiecesRemaining = new List<Piece>(Pieces);
            int _SheetId = 1;
            double CurrentX, CurrentY, TempY;
            CurrentX = CurrentY = TempY = SheetPadding;
            foreach (var piece in Pieces)
            {
                if(piece.CutDirection == CutDirection.Keresztirány && piece.VirtualCutDirection == CutDirection.Keresztirány)
                {
                    (piece.Height, piece.Width) = (piece.Width, piece.Height);
                    piece.VirtualCutDirection = CutDirection.Szálirány;
                }    
            }
            Pieces.Sort((a, b) => 
            {
                int widthComparison = b.Width.CompareTo(a.Width);
                return widthComparison != 0 ? widthComparison : b.Height.CompareTo(a.Height);
            });
            foreach (var piece in Pieces)
            {
                if (piece.x == null || piece.y == null)
                {
                    if (CurrentX + piece.Height <= SheetX - SheetPadding && piece.Width + CurrentY < SheetY) // sheetheight(x) <->, sheetwidth(y) ↕, piece width(x) <->, piece height(y) ↕
                    {
                        piece.x = CurrentX;
                        piece.y = CurrentY;
                        CurrentX += piece.Height + BladeThickness;
                        piece.SheetId = _SheetId;
                        if (piece.Width > TempY)
                            TempY = piece.Width;
                        PiecesRemaining.Remove(piece);
                    }
                    else if (CurrentY + TempY + BladeThickness + piece.Width <= SheetY - SheetPadding)
                    {
                        Piece? StillFittingPiece = TryFitMorePiece(PiecesRemaining, (SheetX - SheetPadding - CurrentX), TempY);
                        if (StillFittingPiece != null )
                        {
                            var FittedPiece = Pieces.FirstOrDefault(p => p.Id == StillFittingPiece.Id);
                            FittedPiece!.x = CurrentX;
                            FittedPiece!.y = CurrentY;
                            FittedPiece!.SheetId = _SheetId;
                            PiecesRemaining.Remove(StillFittingPiece);
                        }
                        CurrentX = SheetPadding;
                        CurrentY += TempY + BladeThickness;
                        piece.x = CurrentX;
                        piece.y = CurrentY;
                        piece.SheetId = _SheetId;
                        CurrentX += piece.Height + BladeThickness;
                        TempY = piece.Width;
                    }
                    else
                    {
                        // No more space on the sheet
                        piece.x = SheetPadding;
                        piece.y = SheetPadding;
                        TempY = SheetPadding;
                        CurrentX = SheetPadding;
                        CurrentY = SheetPadding;
                        _SheetId++;
                    }
                }
            }
            return Pieces;
        }
        public Piece? TryFitMorePiece(List<Piece> PiecesRemaining, double RemainingX, double RemainingY)
        {
            foreach (var piece in PiecesRemaining)
            {
                if (piece.Height <= RemainingX && piece.Width <= RemainingY)
                    return piece;
            }
            return null;
        }
        public List<Piece> Guillotine(List<Piece> Pieces, double SheetWidth, double SheetHeight, double SheetPadding, double BladeThickness)
        {
            // Placeholder for optimization logic
            // This method should implement an algorithm to arrange pieces on the sheet
            // For now, it simply returns the input list without any changes
            return Pieces;
        }
        public List<Piece> Heuristic(List<Piece> Pieces, double SheetWidth, double SheetHeight, double SheetPadding, double BladeThickness)
        {
            // Placeholder for optimization logic
            // This method should implement an algorithm to arrange pieces on the sheet
            // For now, it simply returns the input list without any changes
            return Pieces;
        }
    }
}
