#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// MapTile.cs
//
// This file is part of JumpForJoy Software's J4JMapLibrary.
// 
// J4JMapLibrary is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JMapLibrary is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JMapLibrary. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Text;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary.MapRegion;

public partial class MapTile : Tile
{
    public MapTile(
        MapRegion region,
        int absoluteY
    )
        : base( region, -1, absoluteY )
    {
        SetSize();

        QuadKey = InProjection ? GetQuadKey() : string.Empty;
        FragmentId = GetFragmentId();
    }

    public bool InProjection
    {
        get
        {
            switch( Region.ProjectionType )
            {
                case ProjectionType.Static:
                    return X == 0 && Y == 0;

                case ProjectionType.Tiled:
                    var tileRange = Region.Projection.GetTileRange( Region.Scale );
                    return tileRange.InRange( X ) && tileRange.InRange( Y );

                default:
                    return false;
            }
        }
    }

    public float Height { get; set; }
    public float Width { get; set; }

    public string FragmentId { get; private set; }
    public string QuadKey { get; private set; }

    public int Row { get; private set; } = -1;
    public int Column { get; private set; } = -1;

    public byte[]? ImageData { get; set; }
    public long ImageBytes => ImageData?.Length <= 0 ? -1 : ImageData?.Length ?? -1;

    private string GetQuadKey()
    {
        // static projections only have a single quadkey, defaulting to "0"
        if( Region.Projection is not ITiledProjection )
            return "0";

        var retVal = new StringBuilder();

        for( var i = Region.Scale; i > Region.Projection.ScaleRange.Minimum - 1; i-- )
        {
            var digit = '0';
            var mask = 1 << ( i - 1 );

            if( ( X & mask ) != 0 )
                digit++;

            if( ( Y & mask ) != 0 )
            {
                digit++;
                digit++;
            }

            retVal.Append( digit );
        }

        return retVal.ToString();
    }

    private string GetFragmentId()
    {
        var styleKey = Region.Projection.MapStyle == null
            ? string.Empty
            : $"-{Region.Projection.MapStyle.ToLower()}";

        return Region.Projection is ITiledProjection
            ? $"{Region.Projection.Name}{styleKey}-{QuadKey}"
            : $"{MapExtensions.LatitudeToText(Region.CenterLatitude)}-{MapExtensions.LongitudeToText(Region.CenterLongitude)}-{Region.Scale}{styleKey}-{Region.Projection.TileHeightWidth}-{Region.Projection.TileHeightWidth}";
    }

    private void SetSize()
    {
        if( Region.Projection is ITiledProjection tiledProjection )
        {
            // for tiled projections, a MapTile's size equals
            // the underlying projection's tile size
            Height = tiledProjection.TileHeightWidth;
            Width = tiledProjection.TileHeightWidth;
        }
        else
        {
            // for static projections, there's only ever one
            // MapTile in the MapRegion, and its size
            // is that of the region's bounding box
            Height = Region.BoundingBox.Height;
            Width = Region.BoundingBox.Width;
        }
    }

    public MapTile SetXRelative( int relativeX ) => SetXAbsolute( Region.WrapXOffsetWithinProjection( relativeX ) );

    public MapTile SetXAbsolute( int absoluteX )
    {
        X = absoluteX;
        QuadKey = InProjection ? GetQuadKey() : string.Empty;
        FragmentId = GetFragmentId();

        return this;
    }

    public MapTile SetRowColumn( int row, int column )
    {
        Row = row < 0
            ? throw new ArgumentOutOfRangeException( $"Row row ({row}) less than 0" )
            : row < Region.TilesHigh
                ? row
                : throw new ArgumentOutOfRangeException(
                    $"Row row ({row}) equals or exceeds region height ({Region.TilesHigh})" );

        Column = column < 0
            ? throw new ArgumentOutOfRangeException( $"Column column ({column}) less than 0" )
            : column < Region.TilesWide
                ? column
                : throw new ArgumentOutOfRangeException(
                    $"Column column ({column}) equals or exceeds region width ({Region.TilesWide})" );

        return this;
    }
}
