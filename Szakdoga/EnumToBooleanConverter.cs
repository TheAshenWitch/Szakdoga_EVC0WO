using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Szakdoga
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string enumString && value != null)
            {
                var enumValue = Enum.Parse(value.GetType(), enumString);
                return enumValue.Equals(value);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string enumString && (bool)value)
            {
                return Enum.Parse(targetType, enumString);
            }
            return Binding.DoNothing;
        }
    }
}
