using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.MapLibrary
{
    public class LatLongTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom( ITypeDescriptorContext? context, Type sourceType ) =>
            sourceType.IsAssignableTo( typeof( string ) );

        public override bool CanConvertTo( ITypeDescriptorContext? context, Type? destinationType ) =>
            destinationType != null && destinationType.IsAssignableTo( typeof( LatLong ) );

        public override object? ConvertFrom( ITypeDescriptorContext? context, CultureInfo? culture, object value )
        {
            if( !CanConvertFrom(value.GetType()))
                return null;

            var text = value as string;
            if( string.IsNullOrEmpty( text ) )
                return null;

            return LatLong.TryParse(text, out var result) ? result : null;
        }

        public override object? ConvertTo( ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType )
        {
            if( !CanConvertTo( destinationType ) )
                return null;

            return value is not LatLong latLong ? null : $"{latLong.Latitude}, {latLong.Longitude}";
        }
    }
}
