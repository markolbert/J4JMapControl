using System.Numerics;
using System.Text;
using J4JSoftware.J4JMapLibrary.MapRegion;
using J4JSoftware.VisualUtilities;

namespace J4JSoftware.J4JMapLibrary;

public class TileBlock : MapBlock
{
    public TileBlock(
        ITiledProjection projection,
        int scale,
        int column,
        int row
    )
        : base( projection, scale )
    {
        var maxTiles = projection.GetNumTiles( scale );
        if( row < 0 || row >= maxTiles )
            throw new ArgumentException( $"Row ({row}) exceeds the maximum allowed ({maxTiles - 1})" );

        if (column < 0 || column >= maxTiles)
            throw new ArgumentException($"Column ({column}) exceeds the maximum allowed ({maxTiles - 1})");

        Height = projection.TileHeightWidth;
        Width = Height;
        ProjectionCoordinates.Column = column;
        ProjectionCoordinates.Row = row;

        var heightWidth = projection.GetHeightWidth(scale);
        var halfHw = heightWidth / 2;

        Bounds = new Rectangle2D( heightWidth,
                                  heightWidth,
                                  center: new Vector3( column * heightWidth + halfHw,
                                                       row * heightWidth + halfHw,
                                                       0 ),
                                  coordinateSystem: CoordinateSystem2D.Display );

        QuadKey = GetQuadKey(projection, scale, column, row);
        FragmentId = $"{Projection.Name}{StyleKey}-{QuadKey}";
    }

    public TileCoordinates ProjectionCoordinates { get; } = new();

    public string QuadKey { get; }

    private static string GetQuadKey( ITiledProjection projection, int scale, int column, int row )
    {
        var maxTiles = projection.GetNumTiles( scale );

        if( column < 0 || column >= maxTiles || row < 0 || row >= maxTiles )
            return string.Empty;

        var retVal = new StringBuilder();

        for( var i = scale; i > projection.ScaleRange.Minimum - 1; i-- )
        {
            var digit = '0';
            var mask = 1 << ( i - 1 );

            if( ( column & mask ) != 0 )
                digit++;

            if( ( row & mask ) != 0 )
            {
                digit++;
                digit++;
            }

            retVal.Append( digit );
        }

        return retVal.ToString();
    }
}
