using System.Windows;

namespace Szakdoga.Services
{
    /// <summary>
    /// Optimizer class implements various bin packing algorithms for arranging pieces on sheets.
    /// Supports three optimization modes: Test (simple row-based), Guillotine (heuristic splitting), and Heuristic (placeholder).
    /// </summary>
    internal class Optimizer
    {
        // ============================================================================
        // TEST ALGORITHM - Simple row-based packing
        // ============================================================================

        /// <summary>
        /// Test algorithm: Prepares pieces (handles cut direction swaps) and arranges them row-by-row on sheets.
        /// </summary>
        /// <param name="Pieces">List of pieces to arrange</param>
        /// <param name="SheetX">Sheet width in mm</param>
        /// <param name="SheetY">Sheet height in mm</param>
        /// <param name="SheetPadding">Padding from sheet edges in mm</param>
        /// <param name="BladeThickness">Thickness of cut blade in mm</param>
        /// <returns>List of pieces with assigned positions and sheet IDs</returns>
        public List<Piece> Test(List<Piece> Pieces, double SheetX = 2800, double SheetY = 2070, double SheetPadding = 10, double BladeThickness = 3)
        {
            // Create deep copies of pieces to avoid modifying input
            List<Piece> PiecesRemaining = Pieces.Select(p => new Piece(p)).ToList();
            foreach (var piece in PiecesRemaining)
            {
                // If both cut directions are 'Keresztirány' (cross direction), swap width/height and set direction to 'Szálirány' (grain direction)
                // This optimizes the cutting pattern
                if (piece.CutDirection == CutDirection.Keresztirány && piece.VirtualCutDirection == CutDirection.Keresztirány)
                {
                    (piece.Height, piece.Width) = (piece.Width, piece.Height);
                    piece.VirtualCutDirection = CutDirection.Szálirány;
                }
            }
            // Sort pieces by width (descending), then by height - larger pieces are placed first for better packing
            PiecesRemaining.Sort((a, b) =>
            {
                int widthComparison = b.Width.CompareTo(a.Width);
                return widthComparison != 0 ? widthComparison : b.Height.CompareTo(a.Height);
            });

            // Arrange pieces on sheets using row-based algorithm
            return ArragePieces(PiecesRemaining, SheetX, SheetY, SheetPadding, BladeThickness);
        }

        /// <summary>
        /// Places pieces row by row on sheets, trying to minimize waste.
        /// Algorithm: Places pieces horizontally until row is full, then moves to next row.
        /// When no more space available on current sheet, starts a new sheet.
        /// </summary>
        /// <param name="Pieces">List of pieces to arrange</param>
        /// <param name="SheetX">Sheet width in mm</param>
        /// <param name="SheetY">Sheet height in mm</param>
        /// <param name="SheetPadding">Padding from sheet edges in mm</param>
        /// <param name="BladeThickness">Blade thickness in mm (space required between cuts)</param>
        /// <returns>List of pieces with assigned coordinates and sheet IDs</returns>
        public List<Piece> ArragePieces(List<Piece> Pieces, double SheetX = 2800, double SheetY = 2070, double SheetPadding = 10, double BladeThickness = 3)
        {
            List<Piece> PiecesRemaining = [.. Pieces];
            int _SheetId = 1;
            double CurrentX = SheetPadding;      // Current X position in the current row
            double CurrentY = SheetPadding;      // Current Y position (which row we're on)
            double RowHeight = 0;                // Height of the current row (max piece width in row)

            foreach (var piece in Pieces)
            {
                bool placed;
                do
                {
                    placed = false;
                    if (piece.x == null && piece.y == null && piece.SheetId == null)
                    {
                        // Try to place piece in current position
                        if (CanFitInCurrentPosition(piece, CurrentX, CurrentY, RowHeight, SheetX, SheetY, BladeThickness))
                        {
                            PlacePieceAtCurrentPosition(piece, CurrentX, CurrentY, _SheetId);
                            CurrentX += piece.Height + BladeThickness;
                            RowHeight = Math.Max(RowHeight, piece.Width);
                            PiecesRemaining.Remove(piece);
                        }
                        // Try to place piece in next row if space available
                        else if (CanFitInNextRow(CurrentY, RowHeight, piece.Width, SheetY, BladeThickness, SheetPadding))
                        {
                            placed = TryFitInGapOrNextRow(piece, Pieces, PiecesRemaining, ref CurrentX, ref CurrentY, ref RowHeight, SheetX, SheetY, SheetPadding, BladeThickness, _SheetId);
                        }
                        // Try to fill remaining space or start new sheet
                        else
                        {
                            placed = TryFitInLastGapOrNewSheet(piece, Pieces, PiecesRemaining, ref CurrentX, ref CurrentY, ref RowHeight, ref _SheetId, SheetX, SheetY, SheetPadding, BladeThickness);
                        }
                    }
                } while (placed);
            }

            ValidateAndReportPlacement(Pieces, SheetX, SheetY);
            return Pieces;
        }

        /// <summary>
        /// Checks if a piece can fit in the current position without exceeding sheet boundaries.
        /// </summary>
        /// <returns>True if piece fits horizontally and vertically at current position</returns>
        private bool CanFitInCurrentPosition(Piece piece, double CurrentX, double CurrentY, double RowHeight, double SheetX, double SheetY, double BladeThickness)
        {
            // Check if piece fits within sheet boundaries (accounting for padding on right/bottom)
            return CurrentX + piece.Height + BladeThickness <= SheetX && 
                   CurrentY + Math.Max(RowHeight, piece.Width) + BladeThickness <= SheetY;
        }

        /// <summary>
        /// Places a piece at the specified coordinates and assigns sheet ID.
        /// Reusable method to eliminate code duplication.
        /// </summary>
        private void PlacePieceAtCurrentPosition(Piece piece, double x, double y, int sheetId)
        {
            piece.x = x;
            piece.y = y;
            piece.SheetId = sheetId;
        }

        /// <summary>
        /// Determines if there's enough space for another row without exceeding sheet height.
        /// Accounts for padding and blade thickness.
        /// </summary>
        /// <returns>True if a new row would fit on the current sheet</returns>
        private bool CanFitInNextRow(double CurrentY, double RowHeight, double PieceWidth, double SheetY, double BladeThickness, double SheetPadding)
        {
            return CurrentY + RowHeight + BladeThickness + PieceWidth + BladeThickness <= SheetY - SheetPadding;
        }

        /// <summary>
        /// Attempts to fill the gap at the end of the current row with a smaller piece,
        /// or moves to the next row if no suitable piece is found.
        /// </summary>
        /// <returns>True if another placement was made (triggers retry); False if piece was placed in new row</returns>
        private bool TryFitInGapOrNextRow(Piece piece, List<Piece> Pieces, List<Piece> PiecesRemaining, ref double CurrentX, ref double CurrentY, ref double RowHeight, double SheetX, double SheetY, double SheetPadding, double BladeThickness, int _SheetId)
        {
            // Try to find a smaller piece that fits in the remaining gap
            Piece? StillFittingPiece = TryFitMorePiece(PiecesRemaining, (SheetX - SheetPadding - CurrentX), RowHeight, (int)piece.Id!);
            if (StillFittingPiece != null)
            {
                // Found a piece that fits in the gap - place it and continue
                Piece FittedPiece = Pieces.First(p => p.Id == StillFittingPiece.Id);
                PlacePieceAtCurrentPosition(FittedPiece, CurrentX, CurrentY, _SheetId);
                CurrentX += FittedPiece.Height + BladeThickness;
                RowHeight = Math.Max(RowHeight, FittedPiece.Width);
                PiecesRemaining.Remove(StillFittingPiece);
                return true;
            }
            else
            {
                // No piece fits in gap - move to new row
                CurrentX = SheetPadding;
                CurrentY += RowHeight + BladeThickness;
                RowHeight = 0;  // Reset row height for new row
                PlacePieceAtCurrentPosition(piece, CurrentX, CurrentY, _SheetId);
                CurrentX += piece.Height + BladeThickness;
                RowHeight = piece.Width;
                PiecesRemaining.Remove(piece);
                return false;
            }
        }

        /// <summary>
        /// Attempts to fill remaining space on current sheet or starts a new sheet.
        /// First tries to find a single piece for the gap, then tries multiple pieces for last row,
        /// finally allocates a new sheet if necessary.
        /// </summary>
        /// <returns>True if another placement was made; False if new sheet was started</returns>
        private bool TryFitInLastGapOrNewSheet(Piece piece, List<Piece> Pieces, List<Piece> PiecesRemaining, ref double CurrentX, ref double CurrentY, ref double RowHeight, ref int _SheetId, double SheetX, double SheetY, double SheetPadding, double BladeThickness)
        {
            // Try to fit a single smaller piece in remaining gap
            Piece? StillFittingPiece = TryFitMorePiece(PiecesRemaining, (SheetX - SheetPadding - CurrentX), RowHeight, (int)piece.Id!);
            if (StillFittingPiece != null)
            {
                Piece FittedPiece = Pieces.First(p => p.Id == StillFittingPiece.Id);
                PlacePieceAtCurrentPosition(FittedPiece, CurrentX, CurrentY, _SheetId);
                CurrentX += FittedPiece.Height + BladeThickness;
                RowHeight = Math.Max(RowHeight, FittedPiece.Width);
                PiecesRemaining.Remove(StillFittingPiece);
                return true;
            }
            else
            {
                // Try to fit multiple pieces in the last available row
                List<Piece> fittedRow = TryFitMorePieces(PiecesRemaining, _SheetId, SheetX - 2 * SheetPadding, (SheetY - (CurrentY + RowHeight + BladeThickness + SheetPadding)), BladeThickness);
                if (fittedRow.Count > 0)
                {
                    // Multiple pieces fit - place them and continue on current sheet
                    CurrentX = SheetPadding;
                    CurrentY += RowHeight + BladeThickness;
                    RowHeight = 0;  // Reset row height for new row
                    foreach (var item in fittedRow)
                    {
                        Piece pieceToPlace = Pieces.First(p => p.Id == item.Id);
                        PlacePieceAtCurrentPosition(pieceToPlace, CurrentX, CurrentY, _SheetId);
                        CurrentX += pieceToPlace.Height + BladeThickness;
                        RowHeight = Math.Max(RowHeight, pieceToPlace.Width);
                        PiecesRemaining.Remove(pieceToPlace);
                    }
                    return true;
                }
                else
                {
                    // No more space on current sheet - start a new sheet
                    _SheetId++;
                    CurrentX = SheetPadding;
                    CurrentY = SheetPadding;
                    RowHeight = 0;  // Reset for new sheet
                    PlacePieceAtCurrentPosition(piece, CurrentX, CurrentY, _SheetId);
                    CurrentX += piece.Height + BladeThickness;
                    RowHeight = piece.Width;
                    PiecesRemaining.Remove(piece);
                    return false;
                }
            }
        }

        /// <summary>
        /// Validates piece placements and reports any issues (unplaced pieces or overlaps).
        /// </summary>
        private void ValidateAndReportPlacement(List<Piece> Pieces, double SheetX, double SheetY)
        {
            (int unplaced, int overlap) = ValidatePlacements(Pieces, SheetX, SheetY);
            if (unplaced > 0 || overlap > 0)
            {
                MessageBox.Show($"Placement validation failed:\n{unplaced} unplaced\n{overlap} overlapping", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Validates all piece placements for completeness and overlaps.
        /// Checks that all pieces are placed and no two pieces on the same sheet overlap.
        /// </summary>
        /// <returns>Tuple of (unplaced count, overlap count)</returns>
        public static (int unplaced, int overlap) ValidatePlacements(IEnumerable<Piece> pieces, double sheetWidth, double sheetHeight)
        {
            var problemsList = new List<string>();
            var placed = pieces.Where(p => p.SheetId != null && p.x != null && p.y != null).ToList();
            var unplaced = pieces.Where(p => p.SheetId == null || p.x == null || p.y == null).ToList();
            
            int overlapCount = 0;

            // Check for overlaps within each sheet
            foreach (var g in placed.GroupBy(p => p.SheetId))
            {
                var list = g.ToList();
                for (int i = 0; i < list.Count; ++i)
                    for (int j = i + 1; j < list.Count; ++j)
                    {
                        var a = list[i];
                        var b = list[j];

                        // Both pieces MUST have coordinates at this point (filtered above)
                        double ax0 = a.x ?? 0;
                        double ay0 = a.y ?? 0;
                        double ax1 = ax0 + a.Height;
                        double ay1 = ay0 + a.Width;

                        double bx0 = b.x ?? 0;
                        double by0 = b.y ?? 0;
                        double bx1 = bx0 + b.Height;
                        double by1 = by0 + b.Width;

                        // Check for overlap using AABB (Axis-Aligned Bounding Box) collision
                        // Two rectangles overlap if they intersect on both axes
                        bool overlap = ax0 < bx1 && ax1 > bx0 && ay0 < by1 && ay1 > by0;
                        if (overlap)
                        {
                            problemsList.Add($"Overlap on sheet {g.Key}: Piece {a.Id} ({a.Name}) <-> Piece {b.Id} ({b.Name})");
                            overlapCount++;
                        }
                    }
            }

            return (unplaced.Count, overlapCount);
        }

        /// <summary>
        /// Finds a single piece from remaining pieces that fits in the available space.
        /// Prioritizes larger pieces (by area) to minimize waste.
        /// </summary>
        /// <param name="PiecesRemaining">List of unplaced pieces to search</param>
        /// <param name="RemainingX">Available width in mm</param>
        /// <param name="RemainingY">Available height in mm</param>
        /// <param name="PieceId">ID of the piece we're trying to fit - excluded from search</param>
        /// <returns>Best fitting piece, or null if none found</returns>
        public Piece? TryFitMorePiece(List<Piece> PiecesRemaining, double RemainingX, double RemainingY, int PieceId)
        {
            return PiecesRemaining
                .Where(piece => piece.Height <= RemainingX && 
                               piece.Width <= RemainingY && 
                               piece.Id != PieceId)
                .OrderByDescending(piece => piece.Width * piece.Height)
                .FirstOrDefault();
        }

        /// <summary>
        /// Attempts to fit multiple pieces into available space in a row-by-row manner.
        /// Used to fill remaining space on a sheet or before moving to a new sheet.
        /// </summary>
        /// <param name="pieces">List of unplaced pieces</param>
        /// <param name="sheetId">ID of the sheet to place pieces on</param>
        /// <param name="availWidth">Available width in mm</param>
        /// <param name="availHeight">Available height in mm</param>
        /// <param name="BladeThickness">Blade thickness in mm</param>
        /// <param name="offsetX">X offset for placement (default 0)</param>
        /// <param name="offsetY">Y offset for placement (default 0)</param>
        /// <returns>List of pieces that were successfully placed</returns>
        public List<Piece> TryFitMorePieces(List<Piece> pieces, int sheetId, double availWidth, double availHeight, double BladeThickness, double offsetX = 0, double offsetY = 0)
        {
            var fitted = new List<Piece>();
            var candidates = new List<Piece>(pieces);

            double curX = 0;
            double curY = 0;
            double rowMaxHeight = 0; // Track the maximum width (height in placement) of pieces in current row

            foreach (var piece in candidates)
            {
                if (piece.x != null && piece.y != null) continue; // Skip already placed pieces

                // Try to fit piece in current row
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

                // Try to move to next row if there's space
                if (curY + rowMaxHeight + piece.Width <= availHeight)
                {
                    curX = 0;
                    curY += rowMaxHeight;
                    rowMaxHeight = 0;

                    // Attempt to place piece in the new row
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
                        // Piece doesn't fit even in the new row
                        continue;
                    }
                }
            }

            return fitted;
        }

        // ============================================================================
        // GUILLOTINE ALGORITHM - Heuristic-based rectangle packing with free space splitting
        // ============================================================================

        double _binWidth;
        double _binHeight;
        double _bladeThickness;
        List<Rect> _freeRectangles = new List<Rect>();
        const double EPS = 1e-9;

        /// <summary>
        /// Guillotine algorithm: Advanced bin packing using MaxRects approach with guillotine heuristic.
        /// Splits free space into rectangles after each placement for better space utilization.
        /// Supports piece rotation if cut direction is "Vegyes" (variable).
        /// </summary>
        /// <param name="Pieces">List of pieces to arrange</param>
        /// <param name="SheetX">Sheet width in mm</param>
        /// <param name="SheetY">Sheet height in mm</param>
        /// <param name="SheetPadding">Padding from edges in mm</param>
        /// <param name="BladeThickness">Blade thickness in mm</param>
        /// <returns>List of pieces with assigned positions and sheet IDs</returns>
        public List<Piece> Guillotine(List<Piece> Pieces, double SheetX = 2800, double SheetY = 2070, double SheetPadding = 10, double BladeThickness = 3)
        {
            _binWidth = SheetX;
            _binHeight = SheetY;
            _bladeThickness = Math.Max(0, BladeThickness);
            _freeRectangles = new List<Rect> { new Rect(0, 0, _binWidth, _binHeight) };
            
            if (Pieces == null) throw new ArgumentNullException(nameof(Pieces));
            if (SheetX <= 0 || SheetY <= 0) throw new ArgumentException("Sheet méretek pozitívak legyenek.");

            var PiecesRemaining = new List<Piece>(Pieces);

            // Sort by largest dimension (descending), then by area - prioritizes larger pieces
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
            double innerWidth = SheetX - 2 * SheetPadding;   // Usable horizontal space
            double innerHeight = SheetY - 2 * SheetPadding;  // Usable vertical space

            if (innerWidth <= 0 || innerHeight <= 0) throw new ArgumentException("SheetPadding túl nagy a lapmérethez képest.");

            // Place pieces on sheets until all are placed
            while (PiecesRemaining.Count > 0)
            {
                var packer = new MaxRectsBinPack(innerWidth, innerHeight, BladeThickness);
                bool placedAny = false;

                // Attempt to place all remaining pieces on current sheet
                foreach (var piece in PiecesRemaining.ToList())
                {
                    // Map piece dimensions to packer coordinates
                    // Packer width = piece.Height (horizontal extent)
                    // Packer height = piece.Width (vertical extent)
                    double placementWidth = piece.Height;
                    double placementHeight = piece.Width;

                    bool allowRotate = piece.VirtualCutDirection == CutDirection.Vegyes;

                    var placed = packer.Insert(placementWidth, placementHeight, allowRotate);
                    if (placed != null)
                    {
                        // Convert packer coordinates to sheet coordinates (add padding offset)
                        piece.x = placed.Value.X + SheetPadding;
                        piece.y = placed.Value.Y + SheetPadding;
                        piece.SheetId = sheetId;
                        PiecesRemaining.Remove(piece);
                        placedAny = true;
                    }
                }

                if (!placedAny)
                {
                    // No pieces could be placed on current sheet - try special handling
                    bool advancedPlaced = false;

                    foreach (var piece in PiecesRemaining.ToList())
                    {
                        double placementWidth = piece.Height;
                        double placementHeight = piece.Width;
                        bool allowRotate = piece.VirtualCutDirection == CutDirection.Vegyes;

                        // Check if piece can physically fit (with or without rotation)
                        bool fitsNoRotate = placementWidth <= innerWidth + 1e-9 && placementHeight <= innerHeight + 1e-9;
                        bool fitsRotate = allowRotate && (placementHeight <= innerWidth + 1e-9 && placementWidth <= innerHeight + 1e-9);

                        if (!fitsNoRotate && !fitsRotate)
                        {
                            // Piece is too large - place on separate sheet
                            piece.x = SheetPadding;
                            piece.y = SheetPadding;
                            piece.SheetId = sheetId;
                            PiecesRemaining.Remove(piece);
                            sheetId++;
                            advancedPlaced = true;
                        }
                        else
                        {
                            // Try packing again for this piece
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
                        // Prevent infinite loop - move to next sheet
                        sheetId++;
                    }
                    else
                    {
                        sheetId++;
                    }
                }
                else
                {
                    // At least one piece was placed - move to next sheet
                    sheetId++;
                }
            }

            return Pieces;
        }

        /// <summary>
        /// Internal rectangle structure for the packing algorithm.
        /// </summary>
        private struct Rect
        {
            public double X, Y, Width, Height;
            public Rect(double x, double y, double w, double h) { X = x; Y = y; Width = w; Height = h; }
        }

        /// <summary>
        /// MaxRects bin packer: Advanced rectangle packing algorithm with guillotine-based free space splitting.
        /// Minimizes waste by choosing best placement location based on short side fit and area waste.
        /// </summary>
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

            /// <summary>
            /// Inserts a rectangle into the bin, attempting both normal and rotated orientations if allowed.
            /// Uses best-fit heuristic (minimizes leftover space).
            /// Splits the used space from free rectangles using guillotine algorithm.
            /// </summary>
            /// <param name="width">Rectangle width</param>
            /// <param name="height">Rectangle height</param>
            /// <param name="allowRotate">If true, can try 90° rotation (w <-> h)</param>
            /// <returns>Placed rectangle coordinates, or null if no space found</returns>
            public Rect? Insert(double width, double height, bool allowRotate)
            {
                double reqW = width + _bladeThickness;
                double reqH = height + _bladeThickness;

                int bestIndex = -1;
                Rect bestFree = default;
                double bestShortSide = double.MaxValue;
                double bestAreaWaste = double.MaxValue;
                bool bestRotated = false;

                // Find the best free rectangle to place this piece
                for (int i = 0; i < _freeRectangles.Count; i++)
                {
                    var fr = _freeRectangles[i];

                    // Try without rotation
                    if (reqW <= fr.Width + EPS && reqH <= fr.Height + EPS)
                    {
                        double leftoverW = fr.Width - reqW;
                        double leftoverH = fr.Height - reqH;
                        double shortSide = Math.Min(leftoverW, leftoverH);
                        double areaWaste = fr.Width * fr.Height - reqW * reqH;

                        // Update best if this placement is better
                        if (shortSide < bestShortSide - EPS || (Math.Abs(shortSide - bestShortSide) < EPS && areaWaste < bestAreaWaste - EPS))
                        {
                            bestIndex = i;
                            bestFree = fr;
                            bestShortSide = shortSide;
                            bestAreaWaste = areaWaste;
                            bestRotated = false;
                        }
                    }

                    // Try rotated if allowed
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

                // Determine final rectangle dimensions (with or without rotation)
                Rect placed;
                Rect usedRect;
                if (!bestRotated)
                {
                    placed = new Rect(bestFree.X, bestFree.Y, width, height);
                    usedRect = new Rect(bestFree.X, bestFree.Y, width + _bladeThickness, height + _bladeThickness);
                }
                else
                {
                    // Swapped dimensions for rotated placement
                    placed = new Rect(bestFree.X, bestFree.Y, height, width);
                    usedRect = new Rect(bestFree.X, bestFree.Y, height + _bladeThickness, width + _bladeThickness);
                }

                // Remove the used rectangle and split remaining free space
                _freeRectangles.RemoveAt(bestIndex);
                SplitFreeRectByHeuristic(bestFree, usedRect);

                // Clean up degenerate rectangles and merge adjacent ones
                PruneFreeList();
                MergeFreeList();

                return placed;
            }

            /// <summary>
            /// Splits a free rectangle using guillotine heuristic.
            /// Cuts along the direction with more leftover space for better utilization.
            /// </summary>
            private void SplitFreeRectByHeuristic(Rect freeRect, Rect usedRect)
            {
                double leftoverRight = freeRect.Width - usedRect.Width;
                double leftoverBottom = freeRect.Height - usedRect.Height;

                if (leftoverRight > leftoverBottom)
                {
                    // Vertical cut - split right side and bottom
                    var right = new Rect(freeRect.X + usedRect.Width, freeRect.Y, freeRect.Width - usedRect.Width, freeRect.Height);
                    var bottom = new Rect(freeRect.X, freeRect.Y + usedRect.Height, usedRect.Width, freeRect.Height - usedRect.Height);

                    if (right.Width > EPS && right.Height > EPS) _freeRectangles.Add(right);
                    if (bottom.Width > EPS && bottom.Height > EPS) _freeRectangles.Add(bottom);
                }
                else
                {
                    // Horizontal cut - split bottom and right
                    var bottom = new Rect(freeRect.X, freeRect.Y + usedRect.Height, freeRect.Width, freeRect.Height - usedRect.Height);
                    var right = new Rect(freeRect.X + usedRect.Width, freeRect.Y, freeRect.Width - usedRect.Width, usedRect.Height);

                    if (bottom.Width > EPS && bottom.Height > EPS) _freeRectangles.Add(bottom);
                    if (right.Width > EPS && right.Height > EPS) _freeRectangles.Add(right);
                }
            }

            /// <summary>
            /// Removes degenerate (too small) and contained rectangles from the free list.
            /// Simplifies the data structure for better performance.
            /// </summary>
            private void PruneFreeList()
            {
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

            /// <summary>
            /// Merges adjacent rectangles that can be combined into a larger rectangle.
            /// Reduces fragmentation for better packing efficiency.
            /// </summary>
            private void MergeFreeList()
            {
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

                            // Vertical merge (same X and Width, adjacent Y positions)
                            if (Math.Abs(a.X - b.X) < EPS && Math.Abs(a.Width - b.Width) < EPS)
                            {
                                if (Math.Abs(a.Y + a.Height - b.Y) < EPS)
                                {
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

                            // Horizontal merge (same Y and Height, adjacent X positions)
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

            /// <summary>
            /// Checks if rectangle 'a' is completely contained within rectangle 'b'.
            /// </summary>
            /// <returns>True if 'a' is fully inside 'b'</returns>
            private static bool IsContainedIn(Rect a, Rect b)
            {
                const double EPS = 1e-9;
                return a.X >= b.X - EPS && a.Y >= b.Y - EPS &&
                       a.X + a.Width <= b.X + b.Width + EPS &&
                       a.Y + a.Height <= b.Y + b.Height + EPS;
            }
        }

        // ============================================================================
        // HEURISTIC ALGORITHM - Placeholder
        // ============================================================================

        /// <summary>
        /// Heuristic algorithm: Currently a placeholder - returns pieces unchanged.
        /// Can be implemented with custom optimization logic in the future.
        /// </summary>
        public List<Piece> Heuristic(List<Piece> Pieces, double SheetX = 2800, double SheetY = 2070, double SheetPadding = 10, double BladeThickness = 3)
        {
            return Pieces;
        }
    }
}

