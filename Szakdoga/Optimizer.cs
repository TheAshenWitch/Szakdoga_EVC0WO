using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;


namespace Szakdoga
{
    internal class Optimizer
    {
        // Main entry: prepares and sorts pieces, then arranges them on sheets
        public List<Piece> Test(List<Piece> Pieces, double SheetX = 2800, double SheetY = 2070, double SheetPadding = 10, double BladeThickness = 3)
        {
            // Create deep copies of pieces to avoid modifying input
            List<Piece> PiecesRemaining = Pieces.Select(p => new Piece(p)).ToList();
            foreach (var piece in PiecesRemaining)
            {
                // If both cut directions are 'Keresztirány', swap width/height and set direction to 'Szálirány'
                if (piece.CutDirection == CutDirection.Keresztirány && piece.VirtualCutDirection == CutDirection.Keresztirány)
                {
                    (piece.Height, piece.Width) = (piece.Width, piece.Height);
                    piece.VirtualCutDirection = CutDirection.Szálirány;
                }
            }
            // Sort pieces by width (descending), then by height
            PiecesRemaining.Sort((a, b) =>
            {
                int widthComparison = b.Width.CompareTo(a.Width);
                return widthComparison != 0 ? widthComparison : b.Height.CompareTo(a.Height);
            });

            // Arrange pieces on sheets
            return ArragePieces(PiecesRemaining, SheetX, SheetY, SheetPadding, BladeThickness);
        }

        //Places pieces row by row on sheets, trying to minimize waste
        public List<Piece> ArragePieces(List<Piece> Pieces, double SheetX = 2800, double SheetY = 2070, double SheetPadding = 10, double BladeThickness = 3)
        {
            List<Piece> PiecesRemaining = [.. Pieces];
            int _SheetId = 1;
            double CurrentX, CurrentY, TempY;
            CurrentX = CurrentY = TempY = SheetPadding;
            foreach (var piece in Pieces)
            {
                bool placed;
                do
                {
                    placed = false;
                    if (piece.x == null && piece.y == null && piece.SheetId == null)
                    {
                        if (CurrentX + piece.Height + BladeThickness <= SheetX && piece.Width + CurrentY + BladeThickness <= SheetY) // sheetheight(x) <->, sheetwidth(y) ↕, piece width(x) <->, piece height(y) ↕
                        {
                            piece.x = CurrentX;
                            piece.y = CurrentY;
                            CurrentX += piece.Height + BladeThickness;
                            piece.SheetId = _SheetId;
                            TempY = Math.Max(TempY, piece.Width);
                            PiecesRemaining.Remove(piece);
                        }
                        // If it doesn't fit, try to fit a leftover piece in the remaining space of the row
                        else if (CurrentY + TempY + BladeThickness + piece.Width <= SheetY - SheetPadding)
                        {
                            // Try to fill the gap at the end of the row with a smaller piece
                            Piece? StillFittingPiece = TryFitMorePiece(PiecesRemaining, (SheetX - SheetPadding - CurrentX), TempY, (int)piece.Id!);
                            if (StillFittingPiece != null)
                            {
                                Piece FittedPiece = Pieces.First(p => p.Id == StillFittingPiece.Id);
                                FittedPiece.x = CurrentX;
                                FittedPiece.y = CurrentY;
                                FittedPiece.SheetId = _SheetId;
                                CurrentX += FittedPiece.Height + BladeThickness;
                                TempY = Math.Max(TempY, FittedPiece.Width);
                                PiecesRemaining.Remove(StillFittingPiece);
                                placed = true;
                            }
                            else
                            {
                                // Move to new row
                                CurrentX = SheetPadding;
                                CurrentY += TempY + BladeThickness;
                                piece.x = CurrentX;
                                piece.y = CurrentY;
                                piece.SheetId = _SheetId;
                                CurrentX += piece.Height + BladeThickness;
                                TempY = piece.Width;
                                PiecesRemaining.Remove(piece);
                            }
                        }
                        else
                        {
                            // Try to fill the last gap before starting a new sheet
                            Piece? StillFittingPiece = TryFitMorePiece(PiecesRemaining, (SheetX - SheetPadding - CurrentX), TempY, (int)piece.Id!);
                            if (StillFittingPiece != null)
                            {
                                Piece FittedPiece = Pieces.First(p => p.Id == StillFittingPiece.Id);
                                FittedPiece.x = CurrentX;
                                FittedPiece.y = CurrentY;
                                FittedPiece.SheetId = _SheetId;
                                CurrentX += FittedPiece.Height + BladeThickness;
                                TempY = Math.Max(TempY, FittedPiece.Width);
                                PiecesRemaining.Remove(StillFittingPiece);
                                placed = true;
                            }
                            else
                            {
                                List<Piece> fittedRow = TryFitMorePieces(PiecesRemaining, _SheetId, SheetX, (SheetY - (CurrentY + TempY+SheetPadding)), BladeThickness);
                                if (fittedRow.Count > 0)
                                {
                                    CurrentX = SheetPadding;
                                    CurrentY += TempY + BladeThickness;
                                    foreach (var item in fittedRow)
                                    {
                                        Piece pieceToPlace = Pieces.First(p => p.Id == item.Id);
                                        pieceToPlace.x = CurrentX;
                                        pieceToPlace.y = CurrentY;
                                        pieceToPlace.SheetId = _SheetId;
                                        CurrentX += pieceToPlace.Height + BladeThickness;
                                        TempY = Math.Max(TempY, pieceToPlace.Width);
                                        PiecesRemaining.Remove(pieceToPlace);
                                        placed = true;
                                    }
                                }
                                else
                                {
                                    // No more space: start a new sheet
                                    piece.x = SheetPadding;
                                    piece.y = SheetPadding;
                                    CurrentX = SheetPadding + piece.Height;
                                    CurrentY = SheetPadding;
                                    TempY = CurrentY;
                                    _SheetId++;
                                    piece.SheetId = _SheetId;
                                    PiecesRemaining.Remove(piece);
                                }
                            }
                        }
                    }
                } while (placed);
            }

            int problem = ValidatePlacements(Pieces, SheetX, SheetY);
            if (problem > 0)
            {
                MessageBox.Show($"Placement validation failed:\n{problem}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return Pieces;
        }

        public static int ValidatePlacements(IEnumerable<Piece> pieces, double sheetWidth, double sheetHeight)
        {
            var problems = new List<string>();
            var placed = pieces.Where(p => p.SheetId != null).ToList();
            var unplaced = pieces.Where(p => p.SheetId == null).ToList();
            if (unplaced.Any())
                problems.Add($"Unassigned pieces: {string.Join(',', unplaced.Select(p => p.Id))}");

            foreach (var g in placed.GroupBy(p => p.SheetId))
            {
                var list = g.ToList();
                for (int i = 0; i < list.Count; ++i)
                    for (int j = i + 1; j < list.Count; ++j)
                    {
                        var a = list[i]; var b = list[j];
                        double ax0 = a.x ?? 0, ay0 = a.y ?? 0, ax1 = ax0 + a.Height, ay1 = ay0 + a.Width;
                        double bx0 = b.x ?? 0, by0 = b.y ?? 0, bx1 = bx0 + b.Height, by1 = by0 + b.Width;
                        bool overlap = ax0 < bx1 && ax1 > bx0 && ay0 < by1 && ay1 > by0;
                        if (overlap)
                            problems.Add($"Overlap on sheet {g.Key}: {a.Id} <-> {b.Id}");
                    }
            }
            return unplaced.Count;
        }

        public Piece? TryFitMorePiece(List<Piece> PiecesRemaining, double RemainingX, double RemainingY, int PieceId)
        {
            return PiecesRemaining
                .Where(piece => piece.Height <= RemainingX && 
                               piece.Width <= RemainingY && 
                               piece.Id != PieceId)
                .OrderByDescending(piece => piece.Width * piece.Height)
                .FirstOrDefault();
        }


        //double availWidth = (SheetX - SheetPadding) - CurrentX;
        //double availHeight = TempY - SheetPadding; 

        //if (availWidth > 0 && availHeight > 0)
        //{
        //    var fitted = TryFitMorePieces(PiecesRemaining, _SheetId, CurrentX, CurrentY, availWidth, availHeight, SheetPadding, BladeThickness);
        //    if (fitted.Count > 0)
        //    {
        //        foreach (var fp in fitted)
        //        {
        //            CurrentX += fp.Height;
        //            TempY = Math.Max(TempY, fp.Width);
        //        }
        //    }
        //}
        public List<Piece> TryFitMorePieces(List<Piece> pieces, int sheetId, double availWidth, double availHeight, double BladeThickness, double offsetX = 0, double offsetY = 0)
        {
            var fitted = new List<Piece>();

            var candidates = new List<Piece>(pieces);

            double curX = 0;
            double curY = 0;
            double rowMaxHeight = 0; 

            foreach (var piece in candidates)
            {
                if (piece.x != null && piece.y != null) continue; 

                if (curX + piece.Height + BladeThickness <= availWidth && curY + piece.Width + BladeThickness <= availHeight)
                {
                    piece.x = offsetX + curX;
                    piece.y = offsetY + curY;
                    piece.SheetId = sheetId;

                    curX += piece.Height + BladeThickness;
                    rowMaxHeight = Math.Max(rowMaxHeight, piece.Width);

                    fitted.Add(piece);
                    pieces.Remove(piece); 
                    continue;
                }

                if (curY + rowMaxHeight + piece.Width <= availHeight)
                {
                    // új sor
                    curX = 0;
                    curY += rowMaxHeight;
                    rowMaxHeight = 0;

                    if (curX + piece.Height + BladeThickness <= availWidth && curY + piece.Width + BladeThickness <= availHeight)
                    {
                        piece.x = offsetX + curX;
                        piece.y = offsetY + curY;
                        piece.SheetId = sheetId;

                        curX += piece.Height + BladeThickness;
                        rowMaxHeight = Math.Max(rowMaxHeight, piece.Width);

                        fitted.Add(piece);
                        pieces.Remove(piece);
                        continue;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            return fitted;
        }




        /*
         ===================================================================================================================================================
        */

        double _binWidth;
        double _binHeight;
        double _bladeThickness;
        List<Rect> _freeRectangles;
        const double EPS = 1e-9;
        public List<Piece> Guillotine(List<Piece> Pieces, double SheetX = 2800, double SheetY = 2070, double SheetPadding = 10, double BladeThickness = 3)
        {
            _binWidth = SheetX;
            _binHeight = SheetY;
            _bladeThickness = Math.Max(0, BladeThickness);
            _freeRectangles = new List<Rect> { new Rect(0, 0, _binWidth, _binHeight) };
            if (Pieces == null) throw new ArgumentNullException(nameof(Pieces));
            if (SheetX <= 0 || SheetY <= 0) throw new ArgumentException("Sheet méretek pozitívak legyenek.");

            var PiecesRemaining = new List<Piece>(Pieces);

            PiecesRemaining.Sort((a, b) =>
            {
                var am = Math.Max(a.Width, a.Height);
                var bm = Math.Max(b.Width, b.Height);
                int cmp = bm.CompareTo(am);
                if (cmp != 0) return cmp;
                double aa = a.Width * a.Height;
                double bb = b.Width * b.Height;
                return bb.CompareTo(aa);
            });

            int sheetId = 1;
            double innerWidth = SheetX - 2 * SheetPadding;  // használható vízszintes tartomány
            double innerHeight = SheetY - 2 * SheetPadding; // használható függőleges tartomány

            if (innerWidth <= 0 || innerHeight <= 0) throw new ArgumentException("SheetPadding túl nagy a lapmérethez képest.");

            while (PiecesRemaining.Count > 0)
            {
                var packer = new MaxRectsBinPack(innerWidth, innerHeight, BladeThickness);
                bool placedAny = false;

                foreach (var piece in PiecesRemaining.ToList())
                {
                    // --- FONTOS:---
                    // placementWidth = hogyan foglal helyet a packer vízszintesen => piece.Height
                    // placementHeight = packer függőleges foglalása => piece.Width
                    double placementWidth = piece.Height;
                    double placementHeight = piece.Width;

                    bool allowRotate = piece.VirtualCutDirection == CutDirection.Vegyes;

                    // Ha forgatás engedélyezett, akkor a packer tudja megpróbálni a 90°-os elforgatást (w<->h).
                    var placed = packer.Insert(placementWidth, placementHeight, allowRotate);
                    if (placed != null)
                    {
                        // returned rect's X/Y refer to packer coordinate system (0..innerWidth / 0..innerHeight)
                        piece.x = placed.Value.X + SheetPadding;
                        piece.y = placed.Value.Y + SheetPadding;
                        piece.SheetId = sheetId;
                        PiecesRemaining.Remove(piece);
                        placedAny = true;
                    }
                }

                if (!placedAny)
                {
                    // Ha ezen a lapon egyszer sem tudtunk elhelyezni semmit, akkor megnézzük darabonként,
                    // hogy fizikailag belefér-e (semmilyen orientációban). Ha egy darab SE forgatva, SE nem fér be,
                    // akkor fallback: külön lapra tesszük (az eredeti viselkedéshez hasonló), és növeljük a sheetId-et.
                    bool advancedPlaced = false;

                    foreach (var piece in PiecesRemaining.ToList())
                    {
                        double placementWidth = piece.Height;
                        double placementHeight = piece.Width;
                        bool allowRotate = piece.VirtualCutDirection == CutDirection.Vegyes;

                        bool fitsNoRotate = placementWidth <= innerWidth + 1e-9 && placementHeight <= innerHeight + 1e-9;
                        bool fitsRotate = allowRotate && (placementHeight <= innerWidth + 1e-9 && placementWidth <= innerHeight + 1e-9);

                        if (!fitsNoRotate && !fitsRotate)
                        {
                            piece.x = SheetPadding;
                            piece.y = SheetPadding;
                            piece.SheetId = sheetId;
                            PiecesRemaining.Remove(piece);
                            sheetId++;
                            advancedPlaced = true;
                        }
                        else
                        {
                            // ha elvileg belefér (forgatva vagy nem), próbáljuk meg ismét a packerrel:
                            var placed = packer.Insert(placementWidth, placementHeight, allowRotate);
                            if (placed != null)
                            {
                                piece.x = placed.Value.X + SheetPadding;
                                piece.y = placed.Value.Y + SheetPadding;
                                piece.SheetId = sheetId;
                                PiecesRemaining.Remove(piece);
                                advancedPlaced = true;
                            }
                        }
                    }

                    if (!advancedPlaced)
                    {
                        // Ha semmi sem történt, lépjünk tovább új lapra, hogy ne ragadjunk végtelen ciklusban.
                        sheetId++;
                    }
                    else
                    {
                        // volt előrelépés, köv. iteráción új lapot kezdünk
                        sheetId++;
                    }
                }
                else
                {
                    // legalább egy darabot elhelyeztünk ezen a lapon -> következő lap
                    sheetId++;
                }
            }

            return Pieces;
        }

        private struct Rect
        {
            public double X, Y, Width, Height;
            public Rect(double x, double y, double w, double h) { X = x; Y = y; Width = w; Height = h; }
        }

        private class MaxRectsBinPack
        {
            private readonly double _binWidth;
            private readonly double _binHeight;
            private readonly double _bladeThickness;
            private readonly List<Rect> _freeRectangles;

            public MaxRectsBinPack(double width, double height, double bladeThickness)
            {
                _binWidth = width;
                _binHeight = height;
                _bladeThickness = Math.Max(0, bladeThickness);
                _freeRectangles = new List<Rect> { new Rect(0, 0, _binWidth, _binHeight) };
            }

            public Rect? Insert(double width, double height, bool allowRotate)
            {
                double reqW = width + _bladeThickness;
                double reqH = height + _bladeThickness;

                int bestIndex = -1;
                Rect bestFree = default;
                double bestShortSide = double.MaxValue;
                double bestAreaWaste = double.MaxValue;
                bool bestRotated = false;

                for (int i = 0; i < _freeRectangles.Count; i++)
                {
                    var fr = _freeRectangles[i];

                    // try without rotation
                    if (reqW <= fr.Width + EPS && reqH <= fr.Height + EPS)
                    {
                        double leftoverW = fr.Width - reqW;
                        double leftoverH = fr.Height - reqH;
                        double shortSide = Math.Min(leftoverW, leftoverH);
                        double areaWaste = fr.Width * fr.Height - reqW * reqH;

                        if (shortSide < bestShortSide - EPS || (Math.Abs(shortSide - bestShortSide) < EPS && areaWaste < bestAreaWaste - EPS))
                        {
                            bestIndex = i;
                            bestFree = fr;
                            bestShortSide = shortSide;
                            bestAreaWaste = areaWaste;
                            bestRotated = false;
                        }
                    }

                    // try rotated if allowed
                    if (allowRotate)
                    {
                        double reqWr = height + _bladeThickness;
                        double reqHr = width + _bladeThickness;
                        if (reqWr <= fr.Width + EPS && reqHr <= fr.Height + EPS)
                        {
                            double leftoverW = fr.Width - reqWr;
                            double leftoverH = fr.Height - reqHr;
                            double shortSide = Math.Min(leftoverW, leftoverH);
                            double areaWaste = fr.Width * fr.Height - reqWr * reqHr;

                            if (shortSide < bestShortSide - EPS || (Math.Abs(shortSide - bestShortSide) < EPS && areaWaste < bestAreaWaste - EPS))
                            {
                                bestIndex = i;
                                bestFree = fr;
                                bestShortSide = shortSide;
                                bestAreaWaste = areaWaste;
                                bestRotated = true;
                            }
                        }
                    }
                }

                if (bestIndex == -1) return null;

                // Determine placed rect dims (without blade)
                Rect placed;
                Rect usedRect;
                if (!bestRotated)
                {
                    placed = new Rect(bestFree.X, bestFree.Y, width, height);
                    usedRect = new Rect(bestFree.X, bestFree.Y, width + _bladeThickness, height + _bladeThickness);
                }
                else
                {
                    // swapped dims for placed
                    placed = new Rect(bestFree.X, bestFree.Y, height, width);
                    usedRect = new Rect(bestFree.X, bestFree.Y, height + _bladeThickness, width + _bladeThickness);
                }

                // Remove the chosen free rectangle and split it by guillotine heuristic
                _freeRectangles.RemoveAt(bestIndex);
                SplitFreeRectByHeuristic(bestFree, usedRect);

                // Remove any resulting degenerate or contained rectangles and attempt simple merges
                PruneFreeList();
                MergeFreeList();

                return placed;
            }
            private void SplitFreeRectByHeuristic(Rect freeRect, Rect usedRect)
            {
                // leftover dimensions
                double leftoverRight = freeRect.Width - usedRect.Width;
                double leftoverBottom = freeRect.Height - usedRect.Height;

                // Heurisztika: nagyobb maradék irányában vágunk teljesen (vertikálisan vagy horizontálisan)
                if (leftoverRight > leftoverBottom)
                {
                    // vertikális vágás — jobb oldali nagy csík + alul egy keskeny csík a használt rész szélességével
                    var right = new Rect(freeRect.X + usedRect.Width, freeRect.Y, freeRect.Width - usedRect.Width, freeRect.Height);
                    var bottom = new Rect(freeRect.X, freeRect.Y + usedRect.Height, usedRect.Width, freeRect.Height - usedRect.Height);

                    if (right.Width > EPS && right.Height > EPS) _freeRectangles.Add(right);
                    if (bottom.Width > EPS && bottom.Height > EPS) _freeRectangles.Add(bottom);
                }
                else
                {
                    // horizontális vágás — alsó nagy csík + jobb oldali keskeny csík a használt rész magasságával
                    var bottom = new Rect(freeRect.X, freeRect.Y + usedRect.Height, freeRect.Width, freeRect.Height - usedRect.Height);
                    var right = new Rect(freeRect.X + usedRect.Width, freeRect.Y, freeRect.Width - usedRect.Width, usedRect.Height);

                    if (bottom.Width > EPS && bottom.Height > EPS) _freeRectangles.Add(bottom);
                    if (right.Width > EPS && right.Height > EPS) _freeRectangles.Add(right);
                }
            }
            private void PruneFreeList()
            {
                // remove contained rectangles
                for (int i = 0; i < _freeRectangles.Count; ++i)
                {
                    for (int j = i + 1; j < _freeRectangles.Count; ++j)
                    {
                        if (IsContainedIn(_freeRectangles[i], _freeRectangles[j]))
                        {
                            _freeRectangles.RemoveAt(i);
                            --i;
                            break;
                        }
                        if (IsContainedIn(_freeRectangles[j], _freeRectangles[i]))
                        {
                            _freeRectangles.RemoveAt(j);
                            --j;
                        }
                    }
                }
            }

            private void MergeFreeList()
            {
                // egyszerű egyesítési kísérlet: ha két rect szomszédos és összeilleszthetők, egyesítjük őket
                bool mergedAny;
                do
                {
                    mergedAny = false;
                    for (int i = 0; i < _freeRectangles.Count; ++i)
                    {
                        for (int j = i + 1; j < _freeRectangles.Count; ++j)
                        {
                            var a = _freeRectangles[i];
                            var b = _freeRectangles[j];

                            // függőleges egyesítés (azonos X és Width, Y-k egymás után)
                            if (Math.Abs(a.X - b.X) < EPS && Math.Abs(a.Width - b.Width) < EPS)
                            {
                                if (Math.Abs(a.Y + a.Height - b.Y) < EPS)
                                {
                                    // a felett b
                                    var merged = new Rect(a.X, a.Y, a.Width, a.Height + b.Height);
                                    _freeRectangles[i] = merged;
                                    _freeRectangles.RemoveAt(j);
                                    mergedAny = true;
                                    break;
                                }
                                if (Math.Abs(b.Y + b.Height - a.Y) < EPS)
                                {
                                    var merged = new Rect(b.X, b.Y, b.Width, b.Height + a.Height);
                                    _freeRectangles[i] = merged;
                                    _freeRectangles.RemoveAt(j);
                                    mergedAny = true;
                                    break;
                                }
                            }

                            // horizontális egyesítés (azonos Y és Height, X-k egymás után)
                            if (Math.Abs(a.Y - b.Y) < EPS && Math.Abs(a.Height - b.Height) < EPS)
                            {
                                if (Math.Abs(a.X + a.Width - b.X) < EPS)
                                {
                                    var merged = new Rect(a.X, a.Y, a.Width + b.Width, a.Height);
                                    _freeRectangles[i] = merged;
                                    _freeRectangles.RemoveAt(j);
                                    mergedAny = true;
                                    break;
                                }
                                if (Math.Abs(b.X + b.Width - a.X) < EPS)
                                {
                                    var merged = new Rect(b.X, b.Y, b.Width + a.Width, b.Height);
                                    _freeRectangles[i] = merged;
                                    _freeRectangles.RemoveAt(j);
                                    mergedAny = true;
                                    break;
                                }
                            }
                        }
                        if (mergedAny) break;
                    }
                } while (mergedAny);
            }

            private static bool IsContainedIn(Rect a, Rect b)
            {
                const double EPS = 1e-9;
                return a.X >= b.X - EPS && a.Y >= b.Y - EPS &&
                       a.X + a.Width <= b.X + b.Width + EPS &&
                       a.Y + a.Height <= b.Y + b.Height + EPS;
            }
        }

        public List<Piece> Heuristic(List<Piece> Pieces, double SheetX = 2800, double SheetY = 2070, double SheetPadding = 10, double BladeThickness = 3)
        {
           
            return Pieces;
        }
        
    }
}

