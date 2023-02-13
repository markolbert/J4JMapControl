using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;
using J4JSoftware.Logging;
using J4JSoftware.VisualUtilities;

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

        public float Rotation => 360 - Heading;
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

    public float Rotation => 360 - Heading;

    private async Task UpdateImages( Configuration revisedConfig )
    {
        if( !revisedConfig.IsValid( _projection ) )
        {
            _curConfig = revisedConfig;
            return;
        }

        var curRectangle = CreateRectangle( _curConfig );
        var revisedRectangle = CreateRectangle( revisedConfig );

        if( curRectangle.Contains( revisedRectangle ) != RelativePosition2D.Outside )
        {
            _curConfig = revisedConfig;
            return;
        }

        // update the image(s)
        if( ! await RetrieveImages( revisedConfig ) )
            return;

        _curConfig = revisedConfig;
        Changed?.Invoke( this, EventArgs.Empty );
    }

    private Rectangle2D CreateRectangle( Configuration config )
    {
        var retVal = new Rectangle2D( config.Height, config.Width, config.Rotation );

        // apply buffer
        var bufferTransform = Matrix4x4.CreateScale( 1 + config.Buffer.Width / config.Width,
                                                     1 + config.Buffer.Height / config.Height,
                                                     0 );

        retVal = retVal.ApplyTransform( bufferTransform );

        return ProjectionType == ProjectionType.Tiled
            ? retVal
            : retVal.BoundingBox;
    }

    private async Task<bool> RetrieveImages( Configuration config )
    {
        return ProjectionType switch
        {
            ProjectionType.Static => await RetrieveStaticImages( config ),
            ProjectionType.Tiled => await RetrieveTiledImages( config ),
            _ => throw new InvalidEnumArgumentException($"Unsupported {nameof(ProjectionType)} value '{ProjectionType}'")
        };
    }

    private async Task<bool> RetrieveStaticImages( Configuration config )
    {
        var viewportData = new NormalizedViewport( _projection )
        {
            Scale = Scale,
            CenterLatitude = CenterLatitude,
            CenterLongitude = CenterLongitude,
            Height = Height,
            Width = Width
        };

        var extract = await ( (IStaticProjection) _projection ).GetExtractAsync( viewportData );

        if( extract == null )
            return false;

        _mapFragments.Clear();
        _mapFragments.AddRange( extract );

        return true;
    }

    private async Task<bool> RetrieveTiledImages( Configuration config )
    {
        var viewportData = new Viewport(_projection)
        {
            Scale = Scale,
            CenterLatitude = CenterLatitude,
            CenterLongitude = CenterLongitude,
            Height = Height,
            Width = Width,
            Heading = Heading
        };

        var extract = await ((ITiledProjection)_projection).GetExtractAsync(viewportData);

        if (extract == null)
            return false;

        _mapFragments.Clear();
        _mapFragments.AddRange(extract);

        return true;
    }
}
