namespace J4JSoftware.MapLibrary;

public static class PublicExtensions
{
    // thanx to 3dGrabber for this
    // https://stackoverflow.com/questions/383587/how-do-you-do-integer-exponentiation-in-c
    public static int Pow( this int bas, int exp ) =>
        Enumerable
           .Repeat( bas, Math.Abs(exp) )
           .Aggregate( 1, ( a, b ) => exp < 0 ? a / b : a * b );

    public static LatLong Capped( this LatLong toCheck, MapRetrieverInfo retrieverInfo )
    {
        var absLat = Math.Abs( toCheck.Latitude );
        if( absLat > retrieverInfo.MaximumLatitude )
            toCheck.Latitude = Math.Sign( toCheck.Latitude ) * absLat;

        var absLng = Math.Abs( toCheck.Longitude );
        if( absLng > retrieverInfo.MaximumLongitude )
            toCheck.Longitude = Math.Sign( toCheck.Longitude ) * absLng;

        return toCheck;
    }
}