// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// 
// This file is part of ConsoleUtilities.
//
// ConsoleUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// ConsoleUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with ConsoleUtilities. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using J4JSoftware.Logging;
using J4JSoftware.VisualUtilities;

namespace J4JSoftware.J4JMapLibrary;

public partial class MapFragments<TFrag> : IAsyncEnumerable<TFrag>
    where TFrag : class
{
    public event EventHandler? Changed;

    private readonly IProjection _projection;
    private readonly Func<IMapFragment, Task<TFrag?>> _fragmentFactory;
    private readonly MinMax<float> _heightWidthRange = new( 0F, float.MaxValue );
    private readonly List<TFrag> _fragments = new();

    private Configuration? _curConfig;
    private Configuration? _lastConfig;

    protected MapFragments(
        IProjection projection,
        Func<IMapFragment, Task<TFrag?>> fragmentFactory,
        IJ4JLogger logger
    )
    {
        _projection = projection;
        ProjectionType = _projection.GetProjectionType();

        _fragmentFactory = fragmentFactory;

        Logger = logger;
        Logger.SetLoggedType( GetType() );
    }

    protected IJ4JLogger Logger { get; }

    public ProjectionType ProjectionType { get; }

    public float CenterLatitude => _curConfig?.Latitude ?? 0;

    public float CenterLongitude => _curConfig?.Longitude ?? 0;

    public void SetCenter( float latitude, float longitude )
    {
        latitude = _projection.MapServer.LatitudeRange.ConformValueToRange( latitude, "Latitude" );
        longitude = _projection.MapServer.LongitudeRange.ConformValueToRange( longitude, "Longitude" );

        _curConfig = _curConfig == null
            ? new Configuration( latitude, longitude, 0, 0, 0, 0, Buffer.Default )
            : _curConfig with { Latitude = latitude, Longitude = longitude };

        OnConfigurationChanged();
    }

    public float Height => _curConfig?.Height ?? 0;
    public float Width => _curConfig?.Width ?? 0;

    public void SetHeightWidth( float height, float width )
    {
        height = _heightWidthRange.ConformValueToRange( height, "Height" );
        width = _heightWidthRange.ConformValueToRange( width, "Width" );

        _curConfig = _curConfig == null
            ? new Configuration( 0, 0, 0, 0, height, width, Buffer.Default )
            : _curConfig with { Height = height, Width = width };

        OnConfigurationChanged();
    }

    public float HeightBufferPercent => _curConfig?.Buffer.HeightPercent ?? 0;
    public float WidthBufferPercent => _curConfig?.Buffer.WidthPercent ?? 0;

    public void SetBuffer( float heightPercent, float widthPercent )
    {
        var buffer = new Buffer( _heightWidthRange.ConformValueToRange( heightPercent, "Height Buffer (percent)" ),
                                 _heightWidthRange.ConformValueToRange( widthPercent, "Width Buffer (percent)" ) );

        _curConfig = _curConfig == null
            ? new Configuration( 0, 0, 0, 0, 0, 0, buffer )
            : _curConfig with { Buffer = buffer };

        OnConfigurationChanged();
    }

    public int Scale
    {
        get => _curConfig?.Scale ?? 0;

        set
        {
            value = _projection.MapServer.ScaleRange.ConformValueToRange( value, "Scale" );

            _curConfig = _curConfig == null
                ? new Configuration( 0, 0, 0, value, 0, 0, Buffer.Default )
                : _curConfig with { Scale = value };

            OnConfigurationChanged();
        }
    }

    // in degrees; north is 0/360; stored as mod 360
    public float Heading
    {
        get => _curConfig?.Heading ?? 0;

        set
        {
            value = value % 360;

            _curConfig = _curConfig == null
                ? new Configuration( 0, 0, value, 0, 0, 0, Buffer.Default )
                : _curConfig with { Heading = value };

            OnConfigurationChanged();
        }
    }

    public float Rotation => 360 - Heading;

    public async IAsyncEnumerator<TFrag> GetAsyncEnumerator( CancellationToken ctx = default )
    {
        if( _curConfig == null )
        {
            Logger.Error( "MapExtractNG is not configured, map fragments cannot be retrieved" );
            yield break;
        }

        if( !_curConfig.IsValid( _projection ) )
        {
            Logger.Error( "MapExtractNG is not validly configured, map fragments cannot be retrieved" );
            yield break;
        }

        var needToRetrieve = _lastConfig == null;

        // if we've already retrieved something, check to see if the new configuration
        // requires retrieving stuff from outside the previous configuration's scope
        if( !needToRetrieve )
        {
            var curRectangle = CreateRectangle( _curConfig );
            var lastRectangle = CreateRectangle( _lastConfig! );

            needToRetrieve = lastRectangle.Contains( curRectangle ) == RelativePosition2D.Outside;
        }

        if( needToRetrieve )
        {
            _fragments.Clear();

            await foreach( var imgData in RetrieveImagesAsync( ctx ) )
            {
                var fragment = await _fragmentFactory( imgData );

                if( fragment == null )
                    continue;

                _fragments.Add( fragment );
                yield return fragment;
            }

            _lastConfig = _curConfig;
        }
        else
        {
            foreach( var fragment in _fragments )
            {
                yield return fragment;
            }
        }
    }

    protected virtual void OnConfigurationChanged() => Changed?.Invoke( this, EventArgs.Empty );

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

    private IAsyncEnumerable<IMapFragment> RetrieveImagesAsync( CancellationToken ctx )
    {
        return ProjectionType switch
        {
            ProjectionType.Static => RetrieveStaticImagesAsync( ctx ),
            ProjectionType.Tiled => RetrieveTiledImagesAsync( ctx ),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {nameof( ProjectionType )} value '{ProjectionType}'" )
        };
    }

    private async IAsyncEnumerable<IMapFragment> RetrieveStaticImagesAsync(
        [ EnumeratorCancellation ] CancellationToken ctx
    )
    {
        // the property values being pulled will always be from _revisedConfig
        var viewportData = new NormalizedViewport( _projection )
        {
            Scale = Scale,
            CenterLatitude = CenterLatitude,
            CenterLongitude = CenterLongitude,
            Height = Height,
            Width = Width
        };

        await foreach( var fragment in _projection.GetExtractAsync( viewportData, ctx: ctx ) )
        {
            yield return fragment;
        }
    }

    private async IAsyncEnumerable<IMapFragment> RetrieveTiledImagesAsync(
        [ EnumeratorCancellation ] CancellationToken ctx
    )
    {
        // the property values being pulled will always be from _revisedConfig
        var viewportData = new Viewport( _projection )
        {
            Scale = Scale,
            CenterLatitude = CenterLatitude,
            CenterLongitude = CenterLongitude,
            Height = Height,
            Width = Width,
            Heading = Heading
        };

        await foreach( var fragment in ( (ITiledProjection) _projection ).GetExtractAsync( viewportData, ctx: ctx ) )
        {
            yield return fragment;
        }
    }
}
