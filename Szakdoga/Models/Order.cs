using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Szakdoga.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int? CustomerId { get; set; }   // NULL ha saját projekt
        public Customer? Customer { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<OrderPiece> Pieces { get; set; }
    }

}
