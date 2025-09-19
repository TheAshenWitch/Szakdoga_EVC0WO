using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Szakdoga
{
    public class Settings
    {
        public int Language { get; set; } //0-Hungarian, 1-English
        public bool DarkMode { get; set; } //true-dark, false-light
        public int? SheetHeight { get; set; }   //Deafult 2070
        public int? SheetWidth { get; set; }     //default 2800
        public double BladeThickness { get; set; } //default 3
        public double SheetPadding { get; set; } //default 10
        public string? SheetColor { get; set; }  //default "White"
        public string? SheetManufacturer { get; set; }
    }
}
