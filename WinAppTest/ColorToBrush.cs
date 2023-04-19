using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace WinAppTest;

internal class ColorToBrush : IValueConverter
{
    public object Convert( object value, Type targetType, object parameter, string language )
    {
        if( value is not Color color || targetType != typeof( Brush ) )
            throw new ArgumentException(
                $"Expected to convert {typeof( Color )} to {typeof( Brush )}, but got {value.GetType()}, {targetType} instead" );

        return new SolidColorBrush( color );
    }

    public object ConvertBack( object value, Type targetType, object parameter, string language )
    {
        throw new NotImplementedException();
    }
}
