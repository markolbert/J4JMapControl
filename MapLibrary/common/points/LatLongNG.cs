using J4JSoftware.DeusEx;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.MapLibrary;

public class LatLong
{
    public static LatLong Empty { get; }= new();

    public event EventHandler? ValueChanged;

    private readonly LimitedPoint<double> _latitude;
    private readonly LimitedPoint<double> _longitude;

    public LatLong()
    {
        var mapContext = J4JDeusEx.ServiceProvider.GetRequiredService<IMapContext>();

        _latitude = new LimitedPoint<double>( new DoubleLimits(
                                                  -mapContext.MapImageRetriever.MapRetrieverInfo.MaximumLatitude,
                                                  mapContext.MapImageRetriever.MapRetrieverInfo.MaximumLatitude ) );

        _longitude = new LimitedPoint<double>( new DoubleLimits(
                                                   -mapContext.MapImageRetriever.MapRetrieverInfo.MaximumLongitude,
                                                   mapContext.MapImageRetriever.MapRetrieverInfo.MaximumLongitude ) );
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
