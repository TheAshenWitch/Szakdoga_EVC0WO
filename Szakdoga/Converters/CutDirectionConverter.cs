using System.Globalization;
using System.Windows.Data;
using Szakdoga.Resources;

namespace Szakdoga.Converters
{
    public class CutDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CutDirection direction)
            {
                return GetLocalizedName(direction);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string name)
            {
                return GetDirectionFromLocalizedName(name);
            }
            return CutDirection.Szálirány;
        }

        public static string GetLocalizedName(CutDirection direction)
        {
            return direction switch
            {
                CutDirection.Szálirány => Strings.RadioGrainDir,
                CutDirection.Keresztirány => Strings.RadioCrossDir,
                CutDirection.Vegyes => Strings.RadioVariableDir,
                _ => direction.ToString()
            };
        }

        public static CutDirection GetDirectionFromLocalizedName(string name)
        {
            if (name == Strings.RadioGrainDir) return CutDirection.Szálirány;
            if (name == Strings.RadioCrossDir) return CutDirection.Keresztirány;
            if (name == Strings.RadioVariableDir) return CutDirection.Vegyes;
            return CutDirection.Szálirány;
        }
    }
}
