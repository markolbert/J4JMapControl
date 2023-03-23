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

using System.Collections;
using System.Collections.ObjectModel;
using System.Numerics;
using J4JSoftware.VisualUtilities;
using Serilog;

namespace J4JSoftware.J4JMapLibrary.MapRegion;

public class MapRegion : IEnumerable<MapTile>
{
    private readonly List<MapTile> _mapTiles = new();
    private readonly MinMax<float> _heightWidthRange = new( 0F, float.MaxValue );

    private float _latitude;
    private float _longitude;
    private float _requestedHeight;
    private float _requestedWidth;
    private int _scale;
    private float _heading;

    public MapRegion(
        IProjection projection,
        ILogger logger
    )
    {
        Logger = logger.ForContext<MapRegion>();

        Projection = projection;
        ProjectionType = Projection.GetProjectionType();

        UpperLeft = new MapTile( this, 0, 0 );
        LowerRight = new MapTile( this, 0, 0 );
    }

    protected internal ILogger Logger { get; }

    public IProjection Projection { get; }
    public ProjectionType ProjectionType { get; }

    public float CenterLatitude
    {
        get => _latitude;
        internal set => _latitude = Projection.LatitudeRange.ConformValueToRange( value, "MapRegion Latitude" );
    }

    public float CenterLongitude
    {
        get => _longitude;
        internal set => _longitude = Projection.LongitudeRange.ConformValueToRange( value, "MapRegion Longitude" );
    }

    public Vector3 Center { get; private set; }

    // The ViewpointOffset is the vector that describes how the origin
    // of the display elements -- which defaults to 0,0 -- needs to be
    // offset/translated so that the requested center point is, in fact,
    // in the center of the display elements. This doesn't happen
    // automatically for tiled projections because the images come
    // in fixed sizes, so the upper left image may well include data
    // that should be outside the display area (which is centered on the
    // requested center point)
    public Vector3 ViewpointOffset { get; private set; }

    public float RequestedHeight
    {
        get => _requestedHeight;
        internal set => _requestedHeight = _heightWidthRange.ConformValueToRange( value, "MapRegion Requested Height" );
    }

    public float RequestedWidth
    {
        get => _requestedWidth;
        internal set => _requestedWidth = _heightWidthRange.ConformValueToRange(value, "MapRegion Requested Width");
    }

    public int Scale
    {
        get => _scale;
        internal set => _scale = Projection.ScaleRange.ConformValueToRange(value, "MapRegion Scale");
    }

    // in degrees; north is 0/360; stored as mod 360
    public float Heading
    {
        get => _heading;
        internal set => _heading = value % 360;
    }

    public float Rotation => 360 - Heading;

    public Rectangle2D BoundingBox { get; private set; } = Rectangle2D.Empty;
    public MapTile UpperLeft { get; private set; }
    public MapTile LowerRight { get; private set; }

    public ReadOnlyCollection<MapTile> MapTiles => _mapTiles.AsReadOnly();
    public int NumColumns { get; private set; }
    public int NumRows { get; private set; }

    public MapTile? this[ int xTile, int yTile ]
    {
        get
        {
            if( !_mapTiles.Any()
            || xTile < UpperLeft.X
            || xTile > LowerRight.X
            || yTile < UpperLeft.Y
            || yTile > LowerRight.Y )
                return null;

            return _mapTiles.First( t => t.X == xTile && t.Y == yTile );
        }
    }

    public MapRegion Build()
    {
        _mapTiles.Clear();

        var centerPoint = new StaticPoint( Projection ) { Scale = Scale };
        centerPoint.SetLatLong( CenterLatitude, CenterLongitude );

        Center = new Vector3( centerPoint.X, centerPoint.Y, 0 );

        if( RequestedHeight <= 0 || RequestedWidth <= 0 )
            UpdateEmpty();
        else
        {
            var box = new Rectangle2D(RequestedHeight,
                                      RequestedWidth,
                                      Rotation,
                                      new Vector3(centerPoint.X, centerPoint.Y, 0));

            BoundingBox = box.BoundingBox;

            if (Projection is ITiledProjection)
                UpdateTiledDimensions();
            else UpdateStaticDimensions();

            NumColumns = LowerRight.X - UpperLeft.X + 1;
            NumRows = LowerRight.Y - UpperLeft.Y + 1;
        }

        return this;
    }

    private void UpdateEmpty()
    {
        UpperLeft = new MapTile(this, 0, 0);
        LowerRight = new MapTile(this, 0, 0);
        BoundingBox = Rectangle2D.Empty;
        NumColumns = 0;
        NumRows = 0;
    }

    private void UpdateTiledDimensions()
    {
        var minXTile = RoundTile( BoundingBox.Min( c => c.X ) );
        var maxXTile = RoundTile( BoundingBox.Max( c => c.X ) );
        var minYTile = RoundTile( BoundingBox.Min( c => c.Y ) );
        var maxYTile = RoundTile( BoundingBox.Max( c => c.Y ) );

        UpperLeft = new MapTile( this, minXTile, minYTile );
        LowerRight = new MapTile( this, maxXTile, maxYTile );

        var tileHeightWidth = ( (ITiledProjection) Projection ).TileHeightWidth;

        var tiledPoint = new TiledPoint( (ITiledProjection) Projection ) { Scale = Scale };
        tiledPoint.SetLatLong( CenterLatitude, CenterLongitude );

        // this may need to look for the first tile above and to the left
        // of the tile containing the center point
        var xOffset = tiledPoint.X - RequestedWidth / 2 - UpperLeft.X * tileHeightWidth;
        var yOffset = tiledPoint.Y - RequestedHeight / 2 - UpperLeft.Y * tileHeightWidth;

        ViewpointOffset = new Vector3( -xOffset, -yOffset, 0 );

        for( var xTile = UpperLeft.X; xTile <= LowerRight.X; xTile++ )
        {
            for( var yTile = UpperLeft.Y; yTile <= LowerRight.Y; yTile++ )
            {
                _mapTiles.Add( new MapTile( this, xTile, yTile ) );
            }
        }
    }

    private void UpdateStaticDimensions()
    {
        ViewpointOffset = new Vector3( -( BoundingBox.Width - RequestedWidth ) / 2,
                                       -( BoundingBox.Height - RequestedHeight ) / 2,
                                       0 );

        _mapTiles.Add( new MapTile( this, 0, 0) );
    }

    private int RoundTile( float value )
    {
        var tile = value / Projection.TileHeightWidth;
        return (int)Math.Floor(tile);
        //return tile < 0 ? (int) Math.Floor( tile ) : (int) Math.Ceiling( tile );
    }

    public IEnumerator<MapTile> GetEnumerator()
    {
        if( !_mapTiles.Any() )
            yield break;

        for (var xTile = UpperLeft.X; xTile <= LowerRight.X; xTile++)
        {
            for (var yTile = UpperLeft.Y; yTile <= LowerRight.Y; yTile++)
            {
                yield return this[ xTile, yTile ]!;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
