using System.Collections.ObjectModel;
using System.Numerics;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class MapExtractNg
{
    public event EventHandler? Changed;

    private record Buffer( float Height, float Width )
    {
        public static readonly Buffer Default = new( 0, 0 );
    }

    private record Configuration(
        float Latitude,
        float Longitude,
        float Heading,
        int Scale,
        float Height,
        float Width,
        Buffer Buffer
    )
    {
        public static readonly Configuration Default = new( 0, 0, 0, 0, 0, 0, Buffer.Default );

        public bool IsValid( IProjection projection ) =>
            Height > 0 && Width > 0 && Scale >= projection.MapServer.MinScale && Scale <= projection.MapServer.MaxScale;
    }

    private readonly IProjection _projection;
    private readonly MinMax<float> _heightWidthRange = new( 0F, float.MaxValue );
    private readonly List<IMapFragment> _mapFragments = new();

    private Configuration _curConfig = Configuration.Default;

    public MapExtractNg(
        IProjection projection,
        IJ4JLogger logger
    )
    {
        _projection = projection;
        ProjectionType = _projection.GetProjectionType();

        Logger = logger;
        Logger.SetLoggedType( GetType() );
    }

    protected IJ4JLogger Logger { get; }

    public ProjectionType ProjectionType { get; }
    public ReadOnlyCollection<IMapFragment> MapFragments => _mapFragments.AsReadOnly();

    public float CenterLatitude => _curConfig.Latitude;
    public float CenterLongitude => _curConfig.Longitude;

    public void SetCenter( float latitude, float longitude )
    {
        var revisedConfig = _curConfig with
        {
            Latitude = _projection.MapServer.LatitudeRange.ConformValueToRange( latitude, "Latitude" ),
            Longitude = _projection.MapServer.LongitudeRange.ConformValueToRange( longitude, "Longitude" )
        };

        UpdateImages( revisedConfig );
    }

    public float Height => _curConfig.Height;
    public float Width => _curConfig.Width;

    public void SetHeightWidth( float height, float width )
    {
        var revisedConfig = _curConfig with
        {
            Height = _heightWidthRange.ConformValueToRange( height, "Height" ),
            Width = _heightWidthRange.ConformValueToRange( width, "Width" )
        };

        UpdateImages( revisedConfig );
    }

    public float HeightBuffer => _curConfig.Buffer.Height;
    public float WidthBuffer => _curConfig.Buffer.Width;

    public void SetHeightWidthBuffer( float heightBuffer, float widthBuffer )
    {
        var revisedConfig = _curConfig with
        {
            Buffer = new Buffer( Height: _heightWidthRange.ConformValueToRange( heightBuffer, "Height Buffer" ),
                                 Width: _heightWidthRange.ConformValueToRange( widthBuffer, "Width Buffer" ) )
        };

        UpdateImages( revisedConfig );
    }

    public int Scale
    {
        get => _curConfig.Scale;

        set
        {
            var revisedConfig = _curConfig with
            {
                Scale = _projection.MapServer.ScaleRange.ConformValueToRange( value, "Scale" )
            };

            UpdateImages( revisedConfig );
        }
    }

    // in degrees; north is 0/360; stored as mod 360
    public float Heading
    {
        get => _curConfig.Heading;

        set
        {
            var revisedConfig = _curConfig with { Heading = value % 360 };

            UpdateImages( revisedConfig );
        }
    }

    private void UpdateImages( Configuration revisedConfig )
    {
        if( !revisedConfig.IsValid( _projection ) )
        {
            _curConfig = revisedConfig;
            return;
        }

        var curRectangle = CreateRectangle( _curConfig );
        var revisedRectangle = CreateRectangle( revisedConfig );

        if( RectangleContains( curRectangle, revisedRectangle ) )
        {
            _curConfig = revisedConfig;
            return;
        }

        // update the image(s)
        if( !RetrieveImages( revisedConfig ) )
            return;

        _curConfig = revisedConfig;
        Changed?.Invoke( this, EventArgs.Empty );
    }

    private Vector3[] CreateRectangle( Configuration config )
    {
        var corners = new Vector3[]
        {
            new( 0, 0, 0 ),
            new( config.Width, 0, 0 ),
            new( config.Width, config.Height, 0 ),
            new(0, config.Height, 0)
        };

        var center = new Vector3(config.Width / 2F, config.Height / 2F, 0);

        corners = corners.ApplyTransform(
            Matrix4x4.CreateRotationZ((360 - config.Heading) * MapConstants.RadiansPerDegree, center));

        // apply buffer
        var bufferTransform = Matrix4x4.CreateScale( 1 + config.Buffer.Width / config.Width,
                                                     1 + config.Buffer.Height / config.Height,
                                                     0 );

        corners = corners.ApplyTransform( bufferTransform );

        return ProjectionType == ProjectionType.Tiled
            ? corners
            : NormalizeStaticRectangle(corners);
    }

    private Vector3[] NormalizeStaticRectangle(Vector3[] corners )
    {
        // find the range of tiles covering the mapped rectangle
        var minX = corners.Min( x => x.X );
        var maxX = corners.Max( x => x.X );

        if( maxX < minX )
            ( minX, maxX ) = ( maxX, minX );

        // figuring out the min/max of y coordinates is a royal pain in the ass...
        // because in display space, increasing y values take you >>down<< the screen,
        // not up the screen. So the first adjustment is to subject the raw Y values from
        // the height of the projection to reverse the direction. 
        var minY = corners.Min( y => Height - y.Y );
        var maxY = corners.Max( y => Height - y.Y );

        if( maxY < minY )
            ( minY, maxY ) = ( maxY, minY );

        return new Vector3[] { new( minX, minY, 0 ), new( maxX, minY, 0 ), new( maxX, maxY, 0 ), new( minX, maxY, 0 ) };
    }

    private bool RectangleContains( Vector3[] outer, Vector3[] inner )
    {
        return true;
    }

    private bool RetrieveImages( Configuration config )
    {
        return true;
    }
}
