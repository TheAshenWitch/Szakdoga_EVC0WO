using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

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
        public int? Id { get; set; }
        public string? Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int? x { get; set; }
        public int? y { get; set; }
        public CutDirection CutDirection { get; set; }

        public override string ToString()
        {
            return $"{Id}. {Name} : {Height} x {Width}  |  {CutDirection}";
        }
    }

}
