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
        // A 'parameter' most már maga az enum érték, nem egy string
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 1. Ellenőrizzük, hogy a kapott értékek egyáltalán érvényesek-e
            if (value == null || parameter == null)
            {
                return false;
            }

            // 2. Biztosítjuk, hogy ugyanazt a típust hasonlítjuk össze
            // (Ez debuggoláshoz hasznos, ha a 'value' váratlanul nem enum)
            if (value.GetType() != parameter.GetType())
            {
                // Ide elvileg sosem szabadna eljutnia
                return false;
            }

            // 3. Ez a rész ugyanaz, mint a tiéd
            return parameter.Equals(value);
        }

        // A ConvertBack metódusod már tökéletes volt:
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? parameter : Binding.DoNothing;
        }
    }
}
