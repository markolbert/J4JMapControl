using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;
using J4JSoftware.Logging;
using J4JSoftware.VisualUtilities;

namespace J4JMapLibrary;

public partial class MapExtractNg : IAsyncEnumerable<IMapFragment>
{
    public event EventHandler? Changed;

    private readonly IProjection _projection;
    private readonly MinMax<float> _heightWidthRange = new( 0F, float.MaxValue );
    private readonly List<IMapFragment> _mapFragments = new();

    private Configuration _curConfig = Configuration.Default;
    private Configuration _revisedConfig = Configuration.Default;
    private bool _configChanged;

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
    private Configuration LatestConfiguration => _revisedConfig.Equals( _curConfig ) ? _curConfig : _revisedConfig;

    public ProjectionType ProjectionType { get; }
    public ReadOnlyCollection<IMapFragment> MapFragments => _mapFragments.AsReadOnly();

    public float CenterLatitude => LatestConfiguration.Latitude;
    public float CenterLongitude => LatestConfiguration.Longitude;

    public void SetCenter( float latitude, float longitude )
    {
        _revisedConfig = _revisedConfig with
        {
            Latitude = _projection.MapServer.LatitudeRange.ConformValueToRange( latitude, "Latitude" ),
            Longitude = _projection.MapServer.LongitudeRange.ConformValueToRange( longitude, "Longitude" )
        };

        OnConfigurationChanged();
    }

    public float Height => LatestConfiguration.Height;
    public float Width => LatestConfiguration.Width;

    public void SetHeightWidth( float height, float width )
    {
        _revisedConfig = _revisedConfig with
        {
            Height = _heightWidthRange.ConformValueToRange( height, "Height" ),
            Width = _heightWidthRange.ConformValueToRange( width, "Width" )
        };

        OnConfigurationChanged();
    }

    public float HeightBufferPercent => LatestConfiguration.Buffer.HeightPercent;
    public float WidthBufferPercent => LatestConfiguration.Buffer.WidthPercent;

    public void SetBuffer( float heightPercent, float widthPercent )
    {
        _revisedConfig = _revisedConfig with
        {
            Buffer = new Buffer(
                HeightPercent: _heightWidthRange.ConformValueToRange( heightPercent, "Height Buffer (percent)" ),
                WidthPercent: _heightWidthRange.ConformValueToRange( widthPercent, "Width Buffer (percent)" ) )
        };

        OnConfigurationChanged();
    }

    public int Scale
    {
        get => LatestConfiguration.Scale;

        set
        {
            _revisedConfig = _revisedConfig with
            {
                Scale = _projection.MapServer.ScaleRange.ConformValueToRange( value, "Scale" )
            };

            OnConfigurationChanged();
        }
    }

    // in degrees; north is 0/360; stored as mod 360
    public float Heading
    {
        get => LatestConfiguration.Heading;

        set
        {
            _revisedConfig = _revisedConfig with { Heading = value % 360 };
            OnConfigurationChanged();
        }
    }

    public float Rotation => 360 - Heading;

    public async IAsyncEnumerator<IMapFragment> GetAsyncEnumerator( CancellationToken ctx = default )
    {
        if( _configChanged )
            await UpdateImages( ctx );

        foreach( var fragment in _mapFragments )
        {
            yield return fragment;
        }
    }

    protected virtual void OnConfigurationChanged()
    {
        _configChanged = true;
        Changed?.Invoke( this, EventArgs.Empty );
    }

    private async Task UpdateImages( CancellationToken ctx )
    {
        if( !_revisedConfig.IsValid( _projection ) )
        {
            _curConfig = _revisedConfig;
            return;
        }

        var curRectangle = CreateRectangle( _curConfig );
        var revisedRectangle = CreateRectangle( _revisedConfig );

        if( curRectangle.Contains( revisedRectangle ) != RelativePosition2D.Outside )
        {
            _curConfig = _revisedConfig;
            return;
        }

        // update the image(s)
        if( !await RetrieveImages( _revisedConfig ) )
            return;

        _curConfig = _revisedConfig;
    }

    private Rectangle2D CreateRectangle( Configuration config )
    {
        var retVal = new Rectangle2D( config.Height, config.Width, config.Rotation );

        // apply buffer
        var bufferTransform = Matrix4x4.CreateScale( 1 + config.Buffer.WidthPercent,
                                                     1 + config.Buffer.HeightPercent,
                                                     1 );

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
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {nameof( ProjectionType )} value '{ProjectionType}'" )
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
        var viewportData = new Viewport( _projection )
        {
            Scale = Scale,
            CenterLatitude = CenterLatitude,
            CenterLongitude = CenterLongitude,
            Height = Height,
            Width = Width,
            Heading = Heading
        };

        var extract = await ( (ITiledProjection) _projection ).GetExtractAsync( viewportData );

        if( extract == null )
            return false;

        _mapFragments.Clear();
        _mapFragments.AddRange( extract );

        return true;
    }
}
