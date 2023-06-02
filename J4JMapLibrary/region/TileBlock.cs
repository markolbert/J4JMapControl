using System.Text;
#pragma warning disable CS8618

namespace J4JSoftware.J4JMapLibrary.MapRegion;

public class TileBlock : MapBlock
{
    public static TileBlock? CreateBlock( MapRegion region, int x, int y )
    {
        if( region.ProjectionType != ProjectionType.Tiled )
            return null;

        if( y < 0 || y >= region.MaximumTiles )
            return null;

        x = x < 0 ? x + region.MaximumTiles : x;
        x = x >= region.MaximumTiles ? x - region.MaximumTiles : x;
        if( x < 0 || x >= region.MaximumTiles ) 
            return null;

        var quadKey = GetQuadKey( region, x, y );

        return new TileBlock
        {
            Projection = region.Projection,
            Scale = region.Scale,
            Height=region.TileHeight,
            Width=region.TileWidth,
            X = x,
            Y = y,
            QuadKey = quadKey,
            FragmentId = $"{region.Projection.Name}{GetStyleKey(region.Projection)}-{quadKey}",
        };
    }

    private TileBlock()
    {
    }

    public string QuadKey { get; init; }

    private static string GetQuadKey( MapRegion region, int x, int y )
    {
        if( region.ProjectionType != ProjectionType.Tiled )
            return string.Empty;

        if( x<0 || x >= region.MaximumTiles )
            return string.Empty;

        if( y <0 || y >= region.MaximumTiles )
            return string.Empty;

        var retVal = new StringBuilder();

        for (var i = region.Scale; i > region.Projection.ScaleRange.Minimum - 1; i--)
        {
            var digit = '0';
            var mask = 1 << (i - 1);

            if ((x & mask) != 0)
                digit++;

            if ((y & mask) != 0)
            {
                digit++;
                digit++;
            }

            retVal.Append(digit);
        }

        return retVal.ToString();
    }

}
