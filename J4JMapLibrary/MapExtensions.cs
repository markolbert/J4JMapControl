using System.Text;

namespace J4JMapLibrary;

public static class MapExtensions
{
    public static string GetQuadKey(this MapTile tile)
    {
        var retVal = new StringBuilder();

        for (var i = tile.Metrics.Scale; i > 0; i--)
        {
            var digit = '0';
            var mask = 1 << (i - 1);

            if ((tile.X & mask) != 0)
                digit++;

            if( ( tile.Y & mask ) != 0 )
            {
                digit++;
                digit++;
            }

            retVal.Append(digit);
        }

        return retVal.ToString();
    }
}
