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
    }
}
