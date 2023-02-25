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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using J4JSoftware.Logging;
using J4JSoftware.VisualUtilities;

namespace J4JSoftware.J4JMapLibrary;

public class MapFragments
{
    public event EventHandler? Changed;

    private readonly IProjection _projection;
    private readonly MinMax<float> _heightWidthRange = new( 0F, float.MaxValue );
    private readonly List<IMapFragment> _fragments = new();
    private readonly List<int> _xRange = new();
    private readonly List<int> _yRange = new();

    private MapFragmentsConfiguration? _curConfig;
    private MapFragmentsConfiguration? _lastConfig;

    public MapFragments(
        IProjection projection,
        IJ4JLogger logger
    )
    {
        _projection = projection;
        ProjectionType = _projection.GetProjectionType();

        CenterPoint = ProjectionType switch
        {
            ProjectionType.Static => new StaticPoint( _projection ),
            ProjectionType.Tiled => new TiledPoint( (ITiledProjection) _projection ),
            _ => throw new InvalidEnumArgumentException( $"Unsupported {typeof( ProjectionType )} '{ProjectionType}'" )
        };

        Logger = logger;
        Logger.SetLoggedType( GetType() );
    }

    protected IJ4JLogger Logger { get; }

    public ProjectionType ProjectionType { get; }

    public float CenterLatitude => _curConfig?.Latitude ?? 0;
    public float CenterLongitude => _curConfig?.Longitude ?? 0;

    public void SetCenter( float latitude, float longitude )
    {
        latitude = _projection.LatitudeRange.ConformValueToRange( latitude, "Latitude" );
        longitude = _projection.LongitudeRange.ConformValueToRange( longitude, "Longitude" );

        _curConfig = _curConfig == null
            ? new MapFragmentsConfiguration( latitude, longitude, 0, 0, 0, 0, MapFragmentsBuffer.Default )
            : _curConfig with { Latitude = latitude, Longitude = longitude };

        CenterPoint.SetLatLong( latitude, longitude );

        RaiseChangedEvent();
    }

    public StaticPoint CenterPoint { get; }

    public float RequestedHeight => _curConfig?.RequestedHeight ?? 0;
    public float RequestedWidth => _curConfig?.RequestedWidth ?? 0;

    public void SetRequestedHeightWidth( double height, double width ) =>
        SetRequestedHeightWidth( (float) height, (float) width );

    public void SetRequestedHeightWidth( float height, float width )
    {
        height = _heightWidthRange.ConformValueToRange( height, "RequestedHeight" );
        width = _heightWidthRange.ConformValueToRange( width, "RequestedWidth" );

        _curConfig = _curConfig == null
            ? new MapFragmentsConfiguration( 0, 0, 0, 0, height, width, MapFragmentsBuffer.Default )
            : _curConfig with { RequestedHeight = height, RequestedWidth = width };

        RaiseChangedEvent();
    }

    public float HeightBufferPercent => _curConfig?.FragmentBuffer.HeightPercent ?? 0;
    public float WidthBufferPercent => _curConfig?.FragmentBuffer.WidthPercent ?? 0;

    public void SetBuffer( float heightPercent, float widthPercent )
    {
        var buffer = new MapFragmentsBuffer( _heightWidthRange.ConformValueToRange( heightPercent, "RequestedHeight MapFragmentsBuffer (percent)" ),
                                 _heightWidthRange.ConformValueToRange( widthPercent, "RequestedWidth MapFragmentsBuffer (percent)" ) );

        _curConfig = _curConfig == null
            ? new MapFragmentsConfiguration( 0, 0, 0, 0, 0, 0, buffer )
            : _curConfig with { FragmentBuffer = buffer };

        RaiseChangedEvent();
    }

    public int Scale
    {
        get => _curConfig?.Scale ?? 0;

        set
        {
            value = _projection.ScaleRange.ConformValueToRange( value, "Scale" );

            _curConfig = _curConfig == null
                ? new MapFragmentsConfiguration( 0, 0, 0, value, 0, 0, MapFragmentsBuffer.Default )
                : _curConfig with { Scale = value };

            RaiseChangedEvent();
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
                ? new MapFragmentsConfiguration( 0, 0, value, 0, 0, 0, MapFragmentsBuffer.Default )
                : _curConfig with { Heading = value };

            RaiseChangedEvent();
        }
    }

    public float Rotation => 360 - Heading;

    public ReadOnlyCollection<int> XRange => _xRange.AsReadOnly();
    public ReadOnlyCollection<int> YRange => _yRange.AsReadOnly();

    public IMapFragment? this[ int xTile, int yTile ]
    {
        get
        {
            if( !XRange.Any() || !YRange.Any() )
                return null;

            if( xTile < XRange.First() || xTile > XRange.Last() || yTile < YRange.First() || yTile > YRange.Last() )
                return null;

            return _fragments.First( f => f.XTile == xTile && f.YTile == yTile );
        }
    }

    public ReadOnlyCollection<IMapFragment> Fragments => _fragments.AsReadOnly();

    public float ActualHeight
    {
        get
        {
            if( !_fragments.Any() )
                return 0;

            return ProjectionType == ProjectionType.Static
                ? _fragments[ 0 ].ImageHeight
                : _fragments.GroupBy( f => f.XTile ).First().Sum( g => g.ImageHeight );
        }
    }

    public float ActualWidth
    {
        get
        {
            if (!_fragments.Any())
                return 0;

            return ProjectionType == ProjectionType.Static
                ? _fragments[ 0 ].ImageWidth
                : _fragments.GroupBy( f => f.YTile ).First().Sum( g => g.ImageWidth );
        }
    }

    public bool UpdateNeeded
    {
        get
        {
            if( _curConfig == null )
            {
                Logger.Error( "MapFragments is not configured, map fragments cannot be retrieved" );
                return false;
            }

            if( !_curConfig.IsValid( _projection ) )
            {
                Logger.Error( "MapFragments is not validly configured, map fragments cannot be retrieved" );
                return false;
            }

            // force retrieval if we've never done a retrieval before
            if( _lastConfig == null )
                return true;

            // if we've already retrieved something, check to see if the new configuration
            // requires retrieving stuff from outside the previous configuration's scope
            var curRectangle = CreateRectangle( _curConfig );
            var lastRectangle = CreateRectangle( _lastConfig! );

            return lastRectangle.Contains( curRectangle ) == RelativePosition2D.Outside;
        }
    }

    public async Task UpdateAsync( CancellationToken ctx = default )
    {
        if( !UpdateNeeded )
            return;

        _fragments.Clear();

        var viewportRect = new Rectangle2D(_curConfig!.RequestedHeight, _curConfig.RequestedWidth, _curConfig.Rotation);

        await foreach ( var fragment in RetrieveImagesAsync( ctx ) )
        {
            // a static projection fragment is always in the viewport,
            // although it may extend beyond the viewport
            if( ProjectionType == ProjectionType.Static )
                fragment.InViewport = true;
            else
            {
                var tiledFragment = (ITiledFragment) fragment;

                // see if this fragment is totally inside the viewport or extends beyond it
                var tileCenter = new Vector3( tiledFragment.XTile * tiledFragment.HeightWidth - _curConfig.RequestedWidth / 2,
                                              tiledFragment.YTile * tiledFragment.HeightWidth - _curConfig.RequestedHeight / 2,
                                              0 );

                var fragmentRect = new Rectangle2D( tiledFragment.HeightWidth,
                                                    tiledFragment.HeightWidth,
                                                    0,
                                                    tileCenter,
                                                    CoordinateSystem2D.Display );

                fragment.InViewport = viewportRect.Contains( fragmentRect ) != RelativePosition2D.Outside;
            }

            _fragments.Add( fragment );
        }

        _lastConfig = _curConfig;

        UpdateRanges();
    }

    private void UpdateRanges()
    {
        var min = _fragments.Min( f => f.XTile );
        var max = _fragments.Max( f => f.XTile );

        _xRange.Clear();
        _xRange.AddRange( Enumerable.Range( min, max - min + 1 ) );

        min = _fragments.Min( f => f.YTile );
        max = _fragments.Max( f => f.YTile );

        _yRange.Clear();
        _yRange.AddRange( Enumerable.Range( min, max - min + 1 ) );
    }

    private void RaiseChangedEvent()
    {
        OnConfigurationChanged();

        Changed?.Invoke( this, EventArgs.Empty );
    }

    protected virtual void OnConfigurationChanged()
    {
    }

    private Rectangle2D CreateRectangle( MapFragmentsConfiguration config )
    {
        var retVal = new Rectangle2D( config.RequestedHeight, config.RequestedWidth, config.Rotation );

        // apply buffer
        var bufferTransform = Matrix4x4.CreateScale( 1 + config.FragmentBuffer.WidthPercent,
                                                     1 + config.FragmentBuffer.HeightPercent,
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
            RequestedHeight = (int) Math.Ceiling(RequestedHeight),
            RequestedWidth = (int) Math.Ceiling(RequestedWidth)
        };

        await foreach( var fragment in _projection.GetViewportAsync( viewportData, ctx: ctx ) )
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
            RequestedHeight = (int) Math.Ceiling( RequestedHeight ),
            RequestedWidth = (int) Math.Ceiling( RequestedWidth ),
            Heading = Heading
        };

        await foreach( var fragment in ( (ITiledProjection) _projection ).GetViewportAsync( viewportData, ctx: ctx ) )
        {
            yield return fragment;
        }
    }
}
