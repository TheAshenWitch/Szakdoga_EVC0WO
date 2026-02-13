using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Szakdoga.Models
{
    public class OrderPiece
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public CutDirection CutDirection { get; set; }

        public bool IsAllocated { get; set; }  // kiosztva van-e
    }

}
