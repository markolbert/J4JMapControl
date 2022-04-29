using J4JSoftware.DeusEx;

namespace J4JSoftware.MapLibrary;

public class LatLong
{
    public static LatLong GetEmpty( MapRetrieverInfo info ) => new( info );

    public event EventHandler? ValueChanged;

    private readonly LimitedPoint<double> _latitude;
    private readonly LimitedPoint<double> _longitude;

    public LatLong( MapRetrieverInfo info )
    {
        //var info = mapContext.MapRetriever.MapRetrieverInfo;
        //if( info == null )
        //{
        //    var msg =
        //        $"Attempting to create instance of {nameof( LatLong )} from an undefined {nameof( MapRetrieverInfo )}";

        //    J4JDeusEx.Logger?.Fatal( msg );

        //    throw new J4JDeusExException( msg );
        //}

        _latitude = new LimitedPoint<double>( new DoubleLimits( -info.MaximumLatitude, info.MaximumLatitude ) );
        _longitude = new LimitedPoint<double>( new DoubleLimits( -info.MaximumLongitude, info.MaximumLongitude ) );
    }

    public double Latitude => _latitude.Value;
    public double Longitude => _longitude.Value;

    public void Set( DoublePoint point )
    {
        _latitude.Value = point.Latitude;
        _longitude.Value = point.Longitude;

        OnValueChanged();
    }

    public void Set( LatLong point )
    {
        _latitude.Value = point.Latitude;
        _longitude.Value = point.Longitude;

        OnValueChanged();
    }

    protected virtual void OnValueChanged() => ValueChanged?.Invoke( this, EventArgs.Empty );
}
