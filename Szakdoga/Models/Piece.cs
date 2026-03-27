using Szakdoga.Models;

namespace Szakdoga
{
    public enum CutDirection
    {
        Szálirány,
        Keresztirány,
        Vegyes
    }
    public class Piece
    {
        public int? Id { get; set; } // unique identifier
        public int? SheetId { get; set; } //wich sheet it belongs to
        public string? Name { get; set; } // name of the piece
        public double Width { get; set; } // (height in the alogrithm) /\
        public double Height { get; set; } // (width in the algorithm) <->
        public double? x { get; set; } // position on the sheet
        public double? y { get; set; } // position on the sheet
        public CutDirection CutDirection { get; set; } // cut direction
        public CutDirection VirtualCutDirection { get; set; } // cut direction used in the algorithm
        public override string ToString()
        {
            return $"{Id}. {Name} : {Height} x {Width}  |  {CutDirection}";
        }
        public Piece()
        {
        }

        public Piece(Piece other)
        {
            Id = other.Id;
            SheetId = other.SheetId;
            Name = other.Name;
            Width = other.Width;
            Height = other.Height;
            x = other.x;
            y = other.y;
            CutDirection = other.CutDirection;
            VirtualCutDirection = other.VirtualCutDirection;
        }

        public Piece(int? id, int? sheetId, string? name, double width, double height, double? x, double? y, CutDirection cutDirection, CutDirection virtualCutDirection)
        {
            Id = id;
            SheetId = sheetId;
            Name = name;
            Width = width;
            Height = height;
            this.x = x;
            this.y = y;
            CutDirection = cutDirection;
            VirtualCutDirection = virtualCutDirection;
        }
       
        public static Piece OrderPieceToPiece(OrderPiece oPiece)
        {
            Piece piece = new Piece();
            piece.Id = oPiece.Id;
            piece.Width = oPiece.Width;
            piece.Height = oPiece.Height;
            piece.CutDirection = oPiece.CutDirection;
            if (oPiece.Name != null)
            {
                piece.Name = oPiece.Name;
            }
            if (oPiece.AllocatedSheetId != null)
            {
                piece.SheetId = oPiece.AllocatedSheetId;
            }
            if(oPiece.X  != null)
            {
                piece.x = oPiece.X;
            }
            if (oPiece.Y != null)
            {
                piece.y = oPiece.Y;
            }
            return piece;
        }
        public static List<Piece> OrderPiecesToPieces(List<OrderPiece> oPieces)
        {
            List<Piece> pieces = new List<Piece>();
            foreach (var oPiece in oPieces)
            {
                pieces.Add(OrderPieceToPiece(oPiece));
            }
            return pieces;
        }
        public static OrderPiece PieceToOrderPiece(Piece piece, int orderId)
        {
            OrderPiece oPiece = new OrderPiece();
            oPiece.OrderId = orderId;
            oPiece.Width = piece.Width;
            oPiece.Height = piece.Height;
            oPiece.CutDirection = piece.CutDirection;
            if (piece.Name != null)
            {
                oPiece.Name = piece.Name;
            }
            if (piece.SheetId != null)
            {
                oPiece.AllocatedSheetId = piece.SheetId;
            }
            if (piece.x != null)
            {
                oPiece.X = piece.x;
                oPiece.IsAllocated = true;
            }
            if (piece.y != null)
            {
                oPiece.Y = piece.y;
                oPiece.IsAllocated = true;
            }
            return oPiece;
        }
        public static List<OrderPiece> PiecesToOrderPieces(List<Piece> pieces, int orderId)
        {
            List<OrderPiece> oPieces = new List<OrderPiece>();
            foreach (var piece in pieces)
            {
                oPieces.Add(PieceToOrderPiece(piece, orderId));
            }
            return oPieces;
        }
    }
}
