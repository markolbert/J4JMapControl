﻿// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
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
using Serilog;
using J4JSoftware.VisualUtilities;

namespace J4JSoftware.J4JMapLibrary;

public class MapFragments
{
    public event EventHandler? RetrievalComplete;

    private readonly IProjection _projection;
    private readonly MinMax<float> _heightWidthRange = new( 0F, float.MaxValue );
    private readonly List<IMapFragment> _fragments = new();
    private readonly List<int> _xRange = new();
    private readonly List<int> _yRange = new();

    private MapFragmentsConfiguration? _curConfig;
    private MapFragmentsConfiguration? _lastConfig;

    public MapFragments(
        IProjection projection,
        ILogger logger
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
        Logger.ForContext( GetType() );
    }

    protected ILogger Logger { get; }

    public ProjectionType ProjectionType { get; }

    public float CenterLatitude => _curConfig?.Latitude ?? 0;
    public float CenterLongitude => _curConfig?.Longitude ?? 0;

    public void SetViewport( Viewport viewport, MapFragmentsBuffer? buffer = null )
    {
        buffer ??= MapFragmentsBuffer.Default;

        var latitude = _projection.LatitudeRange.ConformValueToRange(viewport.CenterLatitude, "MapFragments Latitude");
        var longitude = _projection.LongitudeRange.ConformValueToRange(viewport.CenterLongitude, "MapFragments Longitude");
        var height = _heightWidthRange.ConformValueToRange(viewport.RequestedHeight, "MapFragments RequestedHeight");
        var width = _heightWidthRange.ConformValueToRange(viewport.RequestedWidth, "MapFragments RequestedWidth");
        var scale  = _projection.ScaleRange.ConformValueToRange(viewport.Scale, "MapFragments Scale");
        var heading = viewport.Heading % 360;

        _curConfig =new MapFragmentsConfiguration(latitude, longitude, heading, scale, height, width, buffer);

        CenterPoint.SetLatLong(latitude, longitude);
    }

    public StaticPoint CenterPoint { get; }
    public float RequestedHeight => _curConfig?.RequestedHeight ?? 0;
    public float RequestedWidth => _curConfig?.RequestedWidth ?? 0;
    public float HeightBufferPercent => _curConfig?.FragmentBuffer.HeightPercent ?? 0;
    public float WidthBufferPercent => _curConfig?.FragmentBuffer.WidthPercent ?? 0;
    public int Scale => _curConfig?.Scale ?? 0;

    // in degrees; north is 0/360; stored as mod 360
    public float Heading => _curConfig?.Heading ?? 0;
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

            return _fragments.FirstOrDefault( f => f.XTile == xTile && f.YTile == yTile );
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

    public void Update()
    {
        Task.Run( async () =>
        {
            await UpdateAsync();
            RetrievalComplete?.Invoke(this, EventArgs.Empty);
        } );
    }

    public async Task UpdateAsync( CancellationToken ctx = default )
    {
        if( !UpdateNeeded )
            return;

        _fragments.Clear();

        var viewportRect = CreateRectangle( _curConfig! );

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
                var tileCenter = new Vector3( tiledFragment.XTile * tiledFragment.HeightWidth - _curConfig!.RequestedWidth / 2,
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

    private Rectangle2D CreateRectangle( MapFragmentsConfiguration config )
    {
        var centerPoint = new StaticPoint( _projection ) { Scale = config.Scale };
        centerPoint.SetLatLong( config.Latitude, config.Longitude );

        var retVal = new Rectangle2D( config.RequestedHeight,
                                      config.RequestedWidth,
                                      config.Rotation,
                                      new Vector3( centerPoint.X, centerPoint.Y, 0 ) );

        // apply buffer
        var bufferTransform = Matrix4x4.CreateScale( 1 + config.FragmentBuffer.WidthPercent,
                                                     1 + config.FragmentBuffer.HeightPercent,
                                                     1 );

        retVal = retVal.ApplyTransform( bufferTransform );

        return ProjectionType == ProjectionType.Tiled
            ? retVal
            : retVal.BoundingBox;
    }

    private async IAsyncEnumerable<IMapFragment> RetrieveImagesAsync( [ EnumeratorCancellation ] CancellationToken ctx )
    {
        switch( ProjectionType )
        {
            case ProjectionType.Static:
                await foreach( var staticFrag in RetrieveStaticImagesAsync( ctx ) )
                {
                    yield return staticFrag;
                }

                break;

            case ProjectionType.Tiled:
                await foreach (var tiledFrag in RetrieveTiledImagesAsync(ctx))
                {
                    yield return tiledFrag;
                }

                break;

            default:
                throw new InvalidEnumArgumentException(
                    $"Unsupported {nameof( ProjectionType )} value '{ProjectionType}'" );

        }
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
