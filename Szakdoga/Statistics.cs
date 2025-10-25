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
        public double PiecesArea { get; set; } = 0; // in m^2
        public double WasteArea { get; set; } = 0;  // in m^2
        public double MaterialUtilization { get; set; } = 0; // percent
        public double TotalCutLength { get; set; } = 0; // in m
        public double? EdgeSealingNeeded { get; set; } = 0; // in m
        public double? TotalSheetCost { get; set; } = 0;
        public double? TotalEdgeSealingCost { get; set; } = 0;
        public double? TotalCost { get; set; } = 0;
        public double PiecesThisSheet { get; set; } = 0;
        public double MaterialUtilizationThisSheet { get; set; } = 0; // percent
        public double WasteAreaThisSheet { get; set; } = 0; // in m^2

        // Convert mm^2 -> m^2: multiply by 1e-6 (or divide by 1_000_000)
        const double MM2_TO_M2 = 1e-6;

        public void CalculateStatistics(ObservableCollection<Piece> pieces, Settings settings)
        {
            if (pieces == null || settings == null) return;

            NumberOfPieces = pieces.Count;
            NumberOfSheets = pieces.Any(p => p.SheetId != null) ? (int)pieces.Max(p => p.SheetId)! : 0;

            // sum in mm^2
            double totalPiecesAreaMm2 = pieces.Sum(p => p.Height * p.Width);

            // pieces area in m^2
            PiecesArea = totalPiecesAreaMm2 * MM2_TO_M2;

            // total cut length in meters (perimeter in mm -> m)
            TotalCutLength = Math.Round(pieces.Sum(p => 2 * (p.Height + p.Width)) / 1000.0, 3);
            EdgeSealingNeeded = TotalCutLength;

            // total sheet area in mm^2
            double sheetWidthMm = settings.SheetWidth ?? 0;
            double sheetHeightMm = settings.SheetHeight ?? 0;
            double totalSheetsAreaMm2 = NumberOfSheets * sheetWidthMm * sheetHeightMm;

            // waste area in m^2
            double wasteMm2 = Math.Max(0.0, totalSheetsAreaMm2 - totalPiecesAreaMm2);
            WasteArea = Math.Round(wasteMm2 * MM2_TO_M2, 2);

            // material utilization percent
            if (totalSheetsAreaMm2 > 0)
                MaterialUtilization = Math.Round((totalPiecesAreaMm2 / totalSheetsAreaMm2) * 100.0, 2);
            else
                MaterialUtilization = 0;

            // costs
            if ((settings.SheetPrice ?? 0) != 0)
                TotalSheetCost = NumberOfSheets * (settings.SheetPrice ?? 0);
            else
                TotalSheetCost = 0;

            if ((settings.EdgeSealingPrice ?? 0) != 0)
                TotalEdgeSealingCost = TotalCutLength * (settings.EdgeSealingPrice ?? 0);
            else
                TotalEdgeSealingCost = 0;

            if (TotalSheetCost != 0 || TotalEdgeSealingCost != 0)
                TotalCost = (TotalSheetCost ?? 0) + (TotalEdgeSealingCost ?? 0);
            else
                TotalCost = 0;
        }

        public void CalculateStatisticsForSheet(ObservableCollection<Piece> pieces, Settings settings, int _sheetId)
        {
            if (pieces == null || settings == null) return;

            var piecesOnSheet = pieces.Where(p => p.SheetId == _sheetId).ToList();
            PiecesThisSheet = piecesOnSheet.Count;

            double sheetWidthMm = settings.SheetWidth ?? 0;
            double sheetHeightMm = settings.SheetHeight ?? 0;
            double sheetAreaMm2 = sheetWidthMm * sheetHeightMm;

            double piecesAreaMm2 = piecesOnSheet.Sum(p => p.Height * p.Width);

            if (sheetAreaMm2 > 0)
                MaterialUtilizationThisSheet = Math.Round((piecesAreaMm2 / sheetAreaMm2) * 100.0, 2);
            else
                MaterialUtilizationThisSheet = 0;

            double wasteMm2 = Math.Max(0.0, sheetAreaMm2 - piecesAreaMm2);
            WasteAreaThisSheet = Math.Round(wasteMm2 * MM2_TO_M2, 4); // show small values if needed
        }

        public void ClearStatistics()
        {
            NumberOfSheets = 0;
            NumberOfPieces = 0;
            PiecesArea = 0;
            WasteArea = 0;
            MaterialUtilization = 0;
            TotalCutLength = 0;
            EdgeSealingNeeded = 0;
            TotalSheetCost = 0;
            TotalEdgeSealingCost = 0;
            TotalCost = 0;
            PiecesThisSheet = 0;
            MaterialUtilizationThisSheet = 0;
            WasteAreaThisSheet = 0;
        }
    }
}
