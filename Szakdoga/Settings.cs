namespace Szakdoga
{
    public class Settings
    {
        public int Language { get; set; } = 0; //0-Hungarian, 1-English
        public bool DarkMode { get; set; } = false; //true-dark, false-light
        public double? SheetHeight { get; set; } = 2070; //Deafult 2070
        public double? SheetWidth { get; set; } = 2800; //default 2800
        public double BladeThickness { get; set; } = 3.0; //default 3
        public double SheetPadding { get; set; } = 10.0; //default 10
        public string? SheetColor { get; set; }  //default "White"
        public string? SheetManufacturer { get; set; }
        public double? SheetPrice { get; set; } = 10000.0; //default 0
        public double? EdgeSealingPrice { get; set; } = 150.0; //default 0
        public string? Currency { get; set; } = "Huf";
    }
}
