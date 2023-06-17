using System.ComponentModel;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using J4JSoftware.VisualUtilities;

namespace J4JSoftware.J4JMapLibrary;

public abstract class RegionView<TProj> : IRegionView
    where TProj : class, IProjection
{
    public event EventHandler<bool>? ImagesLoaded;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected RegionView(
        TProj projection,
        ProjectionType projectionType
        )
    {
        Projection = projection;
        ProjectionType = projectionType;
    }

    public IProjection Projection { get; }
    public ProjectionType ProjectionType { get; }
    public int TilesHighWide { get; private set; }

    public Region RequestedRegion { get; private set; } = new();
    public MapPoint? Center { get; private set; }

    public Region AdjustedRegion { get; private set; } = new();
    public bool RequestAdjusted => AdjustedRegion != RequestedRegion;
    public Rectangle2D LoadedArea { get; protected set; } = Rectangle2D.Empty;
    public Vector3 LoadedAreaOffset { get; protected set; }

    public (float X, float Y) GetUpperLeftCartesian()
    {
        var upperLeft = LoadedArea.OrderByDescending( c => c.X )
                                    .ThenByDescending( c => c.Y )
                                    .FirstOrDefault();

        return ( upperLeft.X, upperLeft.Y );
    }

    public float? GetZoom( Region requestedRegion )
    {
        var heightWidth = Projection.GetHeightWidth( RequestedRegion.Scale );
        var projRectangle = new Rectangle2D( heightWidth, heightWidth, coordinateSystem: CoordinateSystem2D.Display );

        requestedRegion = ConformRegion( requestedRegion, false );

        requestedRegion.Latitude = Projection.LatitudeRange
                                             .ConformValueToRange( requestedRegion.Latitude, "Latitude" );

        requestedRegion.Longitude = Projection.LongitudeRange
                                              .ConformValueToRange( requestedRegion.Longitude, "Longitude" );

        var center = new MapPoint( Projection, requestedRegion.Scale );
        center.SetLatLong( requestedRegion.Latitude, requestedRegion.Longitude );

        var requestedArea = new Rectangle2D( requestedRegion.Height,
                                             requestedRegion.Width,
                                             requestedRegion.Rotation,
                                             new Vector3( center.X, center.Y, 0 ),
                                             CoordinateSystem2D.Display );

        var shrinkResult = projRectangle.ShrinkToFit( requestedArea, RequestedRegion.ShrinkStyle );

        return shrinkResult.Zoom;
    }

    //public void Offset( float x, float y )
    //{
    //    if( Center == null )
    //        return;

    //    Center.OffsetCartesian( x, y );

    //    ConformProperty( r => r.Latitude, Projection.LatitudeRange );
    //    ConformProperty( r => r.Longitude, Projection.LongitudeRange );
    //}

    public async Task<ILoadedRegion> LoadRegionAsync( Region requestedRegion, CancellationToken ctx = default )
    {
        RequestedRegion = requestedRegion;

        AdjustedRegion = new Region
        {
            Latitude = RequestedRegion.Latitude,
            Longitude = RequestedRegion.Longitude,
            Scale = RequestedRegion.Scale,
            Heading = RequestedRegion.Heading,
            Height = RequestedRegion.Height,
            MapStyle = RequestedRegion.MapStyle,
            ShrinkStyle = RequestedRegion.ShrinkStyle,
            Width = RequestedRegion.Width
        };

        ConformRegion( AdjustedRegion, true );

        TilesHighWide = Projection.GetNumTiles( AdjustedRegion.Scale );

        Center = new MapPoint( Projection, AdjustedRegion.Scale );
        Center.SetLatLong( AdjustedRegion.Latitude, AdjustedRegion.Longitude );

        var requestedArea = new Rectangle2D( AdjustedRegion.Height,
                                             AdjustedRegion.Width,
                                             AdjustedRegion.Rotation,
                                             new Vector3( Center.X, Center.Y, 0 ),
                                             CoordinateSystem2D.Display );

        return await LoadRegionInternalAsync( requestedArea, ctx );
    }

    protected Region ConformRegion( Region region, bool raisePropertyChangeEvents )
    {
        var retVal = new Region
        {
            Latitude = region.Latitude,
            Longitude = region.Longitude,
            Scale = region.Scale,
            Heading = region.Heading,
            Height = region.Height,
            MapStyle = region.MapStyle,
            ShrinkStyle = region.ShrinkStyle,
            Width = region.Width
        };

        ConformProperty( retVal, r => r.Latitude, Projection.LatitudeRange, raisePropertyChangeEvents );
        ConformProperty( retVal, r => r.Longitude, Projection.LongitudeRange, raisePropertyChangeEvents );
        ConformProperty( retVal, r => r.Scale, Projection.ScaleRange, raisePropertyChangeEvents );

        return retVal;
    }

    protected void ConformProperty<TProp>( 
        Region region,
        Expression<Func<Region, TProp>> property,
        MinMax<TProp> range,
        bool raisePropertyChangeEvents
    )
        where TProp : struct, IComparable
    {
        var propInfo = property.GetPropertyInfo();
        var getter = propInfo.CreateGetter<Region, TProp>();
        var setter = propInfo.CreateSetter<Region, TProp>();

        var prop = getter( region );
        var adjProp = range.ConformValueToRange( prop, propInfo.Name );

        if( EqualityComparer<TProp>.Default.Equals( prop, adjProp ) )
            return;

        setter( region, adjProp );

        if( raisePropertyChangeEvents )
            OnPropertyChanged( propInfo.Name );
    }

    protected abstract Task<ILoadedRegion> LoadRegionInternalAsync( Rectangle2D requestedArea, CancellationToken ctx );

    protected virtual void OnImagesLoaded( bool loaded ) => ImagesLoaded?.Invoke( this, loaded );

    protected virtual void OnPropertyChanged( [ CallerMemberName ] string? propertyName = null )
    {
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    protected bool SetField<T>( ref T field, T value, [ CallerMemberName ] string? propertyName = null )
    {
        if( EqualityComparer<T>.Default.Equals( field, value ) )
            return false;

        field = value;
        OnPropertyChanged( propertyName );
        return true;
    }
}