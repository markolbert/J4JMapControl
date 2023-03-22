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
using Serilog;
using J4JSoftware.VisualUtilities;

namespace J4JSoftware.J4JMapLibrary;

public record ViewpointOffset(float X, float Y);

public class MapFragments
{
    public event EventHandler? RetrievalComplete;

    private readonly IProjection _projection;
    private readonly MinMax<float> _heightWidthRange = new( 0F, float.MaxValue );
    private readonly List<IMapFragment> _fragments = new();
    private readonly List<int> _xRange = new();
    private readonly List<int> _yRange = new();

    private MapFragmentsConfiguration? _curConfig;

    public MapFragments(
        IProjection projection,
        ILogger logger
    )
    {
        _projection = projection;
        ProjectionType = _projection.GetProjectionType();

        Logger = logger;
        Logger.ForContext( GetType() );
    }

    protected ILogger Logger { get; }

    public ProjectionType ProjectionType { get; }

    public float CenterLatitude => _curConfig?.Latitude ?? 0;
    public float CenterLongitude => _curConfig?.Longitude ?? 0;
    public Vector3 Center { get; private set; }

    public void SetViewport( Viewport viewport )
    {
        var latitude =
            _projection.LatitudeRange.ConformValueToRange( viewport.CenterLatitude, "MapFragments Latitude" );
        var longitude =
            _projection.LongitudeRange.ConformValueToRange( viewport.CenterLongitude, "MapFragments Longitude" );
        var height = _heightWidthRange.ConformValueToRange( viewport.RequestedHeight, "MapFragments RequestedHeight" );
        var width = _heightWidthRange.ConformValueToRange( viewport.RequestedWidth, "MapFragments RequestedWidth" );
        var scale = _projection.ScaleRange.ConformValueToRange( viewport.Scale, "MapFragments Scale" );
        var heading = viewport.Heading % 360;

        _curConfig = new MapFragmentsConfiguration( latitude, longitude, heading, scale, height, width );
    }

    // The ViewpointOffset is the vector that describes how the origin
    // of the display elements -- which defaults to 0,0 -- needs to be
    // offset/translated so that the requested center point is, in fact,
    // in the center of the display elements. This doesn't happen
    // automatically for tiled projections because the images come
    // in fixed sizes, so the upper left image may well include data
    // that should be outside the display area (which is centered on the
    // requested center point)
    public Vector3 ViewpointOffset { get; private set; }

    public float RequestedHeight => _curConfig?.RequestedHeight ?? 0;
    public float RequestedWidth => _curConfig?.RequestedWidth ?? 0;
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

    public float ActualHeight { get; private set; }
    public float ActualWidth { get; private set; }

    public void Update()
    {
        Task.Run( async () =>
        {
            await UpdateAsync();
            RetrievalComplete?.Invoke( this, EventArgs.Empty );
        } );
    }

    public async Task UpdateAsync( CancellationToken ctx = default )
    {
        _fragments.Clear();

        var viewportRect = _curConfig!.GetBoundingBox( _projection );

        await foreach( var fragment in RetrieveImagesAsync( ctx ) )
        {
            // a static projection fragment is always in the viewport,
            // although it may extend beyond the viewport
            if( ProjectionType == ProjectionType.Static )
                fragment.InViewport = true;
            else
            {
                var tiledFragment = (ITiledFragment) fragment;

                // see if this fragment is totally inside the viewport or extends beyond it
                var tileCenter = new Vector3(
                    tiledFragment.MapXTile * tiledFragment.HeightWidth - _curConfig!.RequestedWidth / 2,
                    tiledFragment.MapYTile * tiledFragment.HeightWidth - _curConfig.RequestedHeight / 2,
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

        OnSuccessfulRetrieval();
    }

    private void OnSuccessfulRetrieval()
    {
        var min = _fragments.Min( f => f.XTile );
        var max = _fragments.Max( f => f.XTile );

        _xRange.Clear();
        _xRange.AddRange( Enumerable.Range( min, max - min + 1 ) );

        min = _fragments.Min( f => f.YTile );
        max = _fragments.Max( f => f.YTile );

        _yRange.Clear();
        _yRange.AddRange( Enumerable.Range( min, max - min + 1 ) );

        ViewpointOffset = ProjectionType switch
        {
            ProjectionType.Static => GetStaticOffset(),
            ProjectionType.Tiled => GetTiledOffset(),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof( ProjectionType )} value '{ProjectionType}'" )
        };

        Center = _curConfig!.GetBoundingBox( _projection ).Center;

        ActualHeight = ProjectionType == ProjectionType.Static
            ? _fragments[ 0 ].ImageHeight
            : _fragments.GroupBy( f => f.MapXTile ).First().Sum( g => g.ImageHeight );

        ActualWidth = ProjectionType == ProjectionType.Static
            ? _fragments[ 0 ].ImageWidth
            : _fragments.GroupBy( f => f.MapYTile ).First().Sum( g => g.ImageWidth );
    }

    private Vector3 GetStaticOffset()
    {
        if( _curConfig == null || !_fragments.Any() )
            return new Vector3();

        var boundingBox = _curConfig.GetBoundingBox( _projection );

        return new Vector3( -( boundingBox.Width - _curConfig.RequestedWidth ) / 2,
                            -( boundingBox.Height - _curConfig.RequestedHeight ) / 2,
                            0 );
    }

    private Vector3 GetTiledOffset()
    {
        if( _curConfig == null || !_fragments.Any() )
            return new Vector3();

        var tileHeightWidth = ( (ITiledProjection) _projection ).TileHeightWidth;

        var upperLeftTile = _fragments[ 0 ];

        var tiledPoint = new TiledPoint( (ITiledProjection) _projection ) { Scale = _curConfig.Scale };
        tiledPoint.SetLatLong( _curConfig.Latitude, _curConfig.Longitude );

        var xOffset = tiledPoint.X - _curConfig.RequestedWidth / 2 - upperLeftTile.XTile * tileHeightWidth;
        var yOffset = tiledPoint.Y - _curConfig.RequestedHeight / 2 - upperLeftTile.YTile * tileHeightWidth;

        return new Vector3(-xOffset, -yOffset, 0);
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
                await foreach( var tiledFrag in RetrieveTiledImagesAsync( ctx ) )
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
        if( _curConfig == null )
            yield break;

        var boundingBox = _curConfig.GetBoundingBox( _projection );

        // the property values being pulled will always be from _revisedConfig
        var viewportData = new NormalizedViewport( _projection )
        {
            Scale = Scale,
            CenterLatitude = _curConfig.Latitude,
            CenterLongitude = _curConfig.Longitude,
            RequestedHeight = (int) Math.Ceiling( boundingBox.Height ),
            RequestedWidth = (int) Math.Ceiling( boundingBox.Width )
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
        if( _curConfig == null )
            yield break;

        var boundingBox = _curConfig.GetBoundingBox( _projection );

        // the property values being pulled will always be from _revisedConfig
        var viewportData = new Viewport( _projection )
        {
            Scale = Scale,
            CenterLatitude = _curConfig.Latitude,
            CenterLongitude = _curConfig.Longitude,
            RequestedHeight = (int) Math.Ceiling( boundingBox.Height ),
            RequestedWidth = (int) Math.Ceiling( boundingBox.Width ),
            Heading = Heading
        };

        await foreach( var fragment in ( (ITiledProjection) _projection ).GetViewportAsync( viewportData, ctx: ctx ) )
        {
            yield return fragment;
        }
    }
}
