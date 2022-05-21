using System;
using Windows.Foundation;

namespace J4JSoftware.MapLibrary;

public class LatLongAnnotation : AnnotationBase
{
    public LatLongAnnotation()
        : base( AnnotationType.LatLong )
    {
    }

    public double Latitutde { get; set; } = double.NaN;
    public double Longitude { get; set; } = double.NaN;
    public string? Text { get; set; }

    public override bool Initialize( Size clipSize, IMapProjection mapProjection )
    {
        IsValid = false;

        if( double.IsNaN( Latitutde ) || double.IsNaN( Longitude ) )
            return IsValid;

        if (string.IsNullOrEmpty(Text) || !LatLong.TryParse(Text, mapProjection.MapRetrieverInfo, out var latLong))
            return IsValid;

        Latitutde = latLong!.Latitude;
        Longitude = latLong.Longitude;

        if ( Math.Abs( Latitutde ) > mapProjection.MapRetrieverInfo.MaximumLatitude
             || Math.Abs( Longitude ) > mapProjection.MapRetrieverInfo.MaximumLongitude )
            return IsValid;

        IsValid = true;

        var upperLeft = mapProjection.LatLongToScreen( latLong, CoordinateOrigin.UpperLeft );

        var (xPoint, yPoint) = upperLeft.GetValues( CoordinateOrigin.UpperLeft );
        Origin = new Point( xPoint, yPoint );

        return IsValid;
    }
}