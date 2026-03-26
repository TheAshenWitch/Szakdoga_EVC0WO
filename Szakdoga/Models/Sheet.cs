using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Szakdoga.Models
{
    public class Sheet
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string? Description { get; set; }

        public string? Color { get; set; }

        public double Width { get; set; }
        public double Height { get; set; }

        public double? Price { get; set; }

        public override string ToString()
        {
            return $"{Name} - {Width}x{Height}";
        }
    }

}
