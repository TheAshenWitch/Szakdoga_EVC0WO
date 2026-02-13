using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Szakdoga.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }

        public int SheetId { get; set; }
        public Sheet Sheet { get; set; }

        public int TotalQuantity { get; set; }
        public int ReservedQuantity { get; set; }

        public int AvailableQuantity => TotalQuantity - ReservedQuantity;
    }

}
