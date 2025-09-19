using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Szakdoga
{
    internal class Optimizer
    {
        public List<Piece> Test(List<Piece> Pieces, double SheetWidth, double SheetHeight, double SheetPadding, double BladeThickness)
        {
            int _SheetId = 1;
            double CurrentX, CurrentY, TempY;
            CurrentX = CurrentY = TempY = SheetPadding;
            foreach (var piece in Pieces)
            {
                if(piece.CutDirection == CutDirection.Keresztirány)
                {
                    double temp = piece.Height;
                    piece.Height = piece.Width;
                    piece.Width = temp;
                }    
            }
            Pieces.Sort((a, b) => 
            {
                int heightComparison = b.Height.CompareTo(a.Height);
                return heightComparison != 0 ? heightComparison : b.Width.CompareTo(a.Width);
            });
            foreach (var piece in Pieces)
            {
                if (CurrentX + piece.Height <= SheetWidth - SheetPadding)
                {
                    piece.x = CurrentX;
                    piece.y = CurrentY;
                    CurrentX += piece.Height + BladeThickness;
                    piece.SheetId = _SheetId;
                    if (piece.Width > TempY)
                        TempY = piece.Width;
                }
                else if (CurrentY + TempY + BladeThickness + piece.Width <= SheetHeight - SheetPadding)
                {
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
                    _SheetId++;
                }
            }
            return Pieces;
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
