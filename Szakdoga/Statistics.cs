using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Szakdoga
{
    public class Statistics
    {
        public int NumberOfSheets { get; set; } = 0;
        public int NumberOfPieces { get; set; } = 0;
        public double PiecesArea { get; set; } = 0;
        public double WasteArea { get; set; } = 0;
        public double MaterialUtilization { get; set; } = 0;
        public double TotalCutLength { get; set; } = 0;
        public double? EdgeSealingNeeded { get; set; } = 0;
        public double? TotalSheetCost { get; set; } = 0;
        public double? TotalEdgeSealingCost { get; set; } = 0;
        public double? TotalCost { get; set; } = 0;
        public double PiecesThisSheet { get; set; } = 0;
        public double MaterialUtilizationThisSheet { get; set; } = 0;
        public double WasteAreaThisSheet { get; set; } = 0;
        public void CalculateStatistics(ObservableCollection<Piece> pieces, Settings settings)
        {
            NumberOfSheets = (int)pieces.Max(p => p.SheetId)!;
            NumberOfPieces = pieces.Count;
            PiecesArea = pieces.Sum(p => p.Height * p.Width) / 100000;
            TotalCutLength = Math.Round(pieces.Sum(p => 2 * (p.Height + p.Width))/1000, 3);
            WasteArea = Math.Round((double)((NumberOfSheets * settings.SheetWidth * settings.SheetHeight) - (PiecesArea * 100000))! / 100000, 2);
            MaterialUtilization = Math.Round((double)((PiecesArea * 100000) / (NumberOfSheets * settings.SheetWidth * settings.SheetHeight) * 100)! ,2);
            EdgeSealingNeeded = TotalCutLength;
            if(settings.SheetPrice != 0)
                TotalSheetCost = NumberOfSheets * settings.SheetPrice;
            if (settings.EdgeSealingPrice != 0)
                TotalEdgeSealingCost = TotalCutLength * settings.EdgeSealingPrice;
            if (TotalSheetCost != 0 || TotalEdgeSealingCost != 0)
                TotalCost = TotalSheetCost + TotalEdgeSealingCost;
        }
        public void CalculateStatisticsForSheet(ObservableCollection<Piece> pieces, Settings settings, int _sheetId)
        {
            PiecesThisSheet = pieces.Where(p => p.SheetId == _sheetId).Count();
            MaterialUtilizationThisSheet = Math.Round((double)((pieces.Where(p => p.SheetId == _sheetId).Sum(p => p.Height * p.Width) * 100000) / (settings.SheetWidth * settings.SheetHeight) * 100)! /100000, 2);
            WasteAreaThisSheet = (double)((settings.SheetWidth * settings.SheetHeight) - pieces.Where(p => p.SheetId == _sheetId).Sum(p => p.Height * p.Width))! / 1000000; 
        }
    }
}
