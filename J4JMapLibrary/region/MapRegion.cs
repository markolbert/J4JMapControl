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
using System.ComponentModel;
using System.Numerics;
using J4JSoftware.VisualUtilities;
using Serilog;

namespace J4JSoftware.J4JMapLibrary.MapRegion;

public class MapRegion : IEnumerable<MapTile>
{
    private readonly MinMax<float> _heightWidthRange = new( 0F, float.MaxValue );

    private float _latitude;
    private float _longitude;
    private float _requestedHeight;
    private float _requestedWidth;
    private int _scale;
    private float _heading;

#pragma warning disable CS8618
    public MapRegion(
#pragma warning restore CS8618
        IProjection projection,
        ILogger logger
    )
    {
        Logger = logger.ForContext<MapRegion>();

        Projection = projection;
        ProjectionType = Projection.GetProjectionType();

        Scale = projection.MinScale;
        MaximumTiles = projection.GetNumTiles( Scale );

        UpdateEmpty();
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

    public Rectangle2D BoundingBox { get; private set; }
    public int MaximumTiles { get; private set; }
    public Tile UpperLeft { get; private set; }
    public int TilesWide { get; private set; }
    public int TilesHigh { get; private set; }

    public MapTile[,] MapTiles { get; private set; } = new MapTile[0, 0];

    public MapTile? this[ int xRelative, int yRelative ]
    {
        get
        {
            if( xRelative < 0 || yRelative < 0 || xRelative >= TilesWide || yRelative >= TilesHigh )
                return null;

            return MapTiles[ xRelative, yRelative ];
        }
    }

    public int ConvertRelativeXToAbsolute( int relativeX )
    {
        var retVal = UpperLeft.X + relativeX;

        if (retVal < 0)
            retVal += MaximumTiles;

        if (retVal < 0)
            retVal = -1;

        if (retVal >= MaximumTiles)
            retVal = -1;

        return retVal;
    }

    public bool IsDefined { get; private set; }
    public bool ChangedOnBuild { get; private set; }

    public MapRegion Build()
    {
        ChangedOnBuild = false;
        IsDefined = false;

        MaximumTiles = Projection.GetNumTiles(Scale);

        var oldBoundingBox = BoundingBox.Copy();
        var oldUpperLeft = UpperLeft;
        var oldWide = TilesWide;
        var oldHigh = TilesHigh;

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

            IsDefined = true;
        }

        ChangedOnBuild = ProjectionType switch
        {
            ProjectionType.Static => !BoundingBox.Equals( oldBoundingBox ),
            ProjectionType.Tiled => UpperLeft != oldUpperLeft || TilesWide != oldWide || TilesHigh != oldHigh,
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof( ProjectionType )} value '{ProjectionType}'" )
        };

        return this;
    }

    private void UpdateEmpty()
    {
        UpperLeft = new Tile(this, int.MinValue, int.MinValue);
        TilesWide = 0;
        TilesHigh = 0;
        BoundingBox = Rectangle2D.Empty;
    }

    private void UpdateTiledDimensions()
    {
        var minXTile = RoundTile( BoundingBox.Min( c => c.X ) );
        var maxXTile = RoundTile( BoundingBox.Max( c => c.X ) - 1 );
        var minYTile = RoundTile( BoundingBox.Min( c => c.Y ) );
        var maxYTile = RoundTile( BoundingBox.Max( c => c.Y )-1 );

        UpperLeft = new Tile( this, minXTile, minYTile );
        TilesWide = maxXTile - minXTile + 1;
        TilesHigh = maxYTile - minYTile + 1;

        var tileHeightWidth = ( (ITiledProjection) Projection ).TileHeightWidth;

        var tiledPoint = new TiledPoint( (ITiledProjection) Projection ) { Scale = Scale };
        tiledPoint.SetLatLong( CenterLatitude, CenterLongitude );

        // this may need to look for the first tile above and to the left
        // of the tile containing the center point
        var xOffset = tiledPoint.X - RequestedWidth / 2 - UpperLeft.X * tileHeightWidth;
        var yOffset = tiledPoint.Y - RequestedHeight / 2 - UpperLeft.Y * tileHeightWidth;

        ViewpointOffset = new Vector3( -xOffset, -yOffset, 0 );

        MapTiles = new MapTile[ TilesWide, TilesHigh ];

        for( var yTileOffset = 0; yTileOffset < TilesHigh; yTileOffset++ )
        {
            for( var xTileOffset = 0; xTileOffset < TilesWide; xTileOffset++ )
            {
                MapTiles[ xTileOffset, yTileOffset ] = new MapTile( this, UpperLeft.Y + yTileOffset )
                   .SetXRelative( xTileOffset );
            }
        }
    }

    private void UpdateStaticDimensions()
    {
        UpperLeft = new Tile(this, 0,0);
        TilesWide = 1;
        TilesHigh = 1;

        ViewpointOffset = new Vector3( -( BoundingBox.Width - RequestedWidth ) / 2,
                                       -( BoundingBox.Height - RequestedHeight ) / 2,
                                       0 );

        MapTiles = new MapTile[ 1, 1 ];
        MapTiles[ 0, 0 ] = new MapTile( this, 0 ).SetXAbsolute( 0 );
    }

    private int RoundTile( float value )
    {
        var tile = Math.Round(value) / Projection.TileHeightWidth;
        return (int)Math.Floor(tile);
        //return tile < 0 ? (int) Math.Floor( tile ) : (int) Math.Ceiling( tile );
    }

    public IEnumerator<MapTile> GetEnumerator()
    {
        if( !IsDefined )
            yield break;

        for( var y = 0; y < TilesHigh; y++ )
        {
            for( var x = 0; x < TilesWide; x++ )
            {
                yield return MapTiles[ x, y ];
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
