using System.Text;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public static class MapExtensions
{
    private static readonly IJ4JLogger? Logger;
    private static char[] ValidQuadKeyCharacters = new[] { '0', '1', '2', '3' };

    static MapExtensions()
    {
        Logger = J4JDeusEx.GetLogger();
        Logger?.SetLoggedType( typeof( MapExtensions ) );
    }

    public static string GetQuadKey( this MapTile tile )
    {
        var retVal = new StringBuilder();

        for( var i = tile.Metrics.Scale; i > 0; i-- )
        {
            var digit = '0';
            var mask = 1 << ( i - 1 );

            if( ( tile.X & mask ) != 0 )
                digit++;

            if( ( tile.Y & mask ) != 0 )
            {
                digit++;
                digit++;
            }

            retVal.Append( digit );
        }

        return retVal.ToString();
    }

    public static async Task<string> GetQuadKeyAsync(this ITiledProjection projection, int xTile, int yTile )
    {
        var tile = await MapTile.CreateAsync( projection, xTile, yTile );

        return tile.GetQuadKey();
    }

    public static bool TryParseQuadKey( string quadKey, out DeconstructedQuadKey? result )
    {
        result = null;

        if( quadKey.Length < 1 )
        {
            Logger?.Error( "Empty quadkey string" );
            return false;
        }

        result = new DeconstructedQuadKey { Scale = quadKey.Length };

        if( !quadKey.Any( x => ValidQuadKeyCharacters.Any( y => y == x ) ) )
        {
            Logger?.Error( "Invalid characters in quadkey string" );
            return false;
        }

        var levelOfDetail = quadKey.Length;

        for( var i = levelOfDetail; i > 0; i-- )
        {
            var mask = 1 << ( i - 1 );
            switch( quadKey[ levelOfDetail - i ] )
            {
                case '0':
                    break;

                case '1':
                    result.XTile |= mask;
                    break;

                case '2':
                    result.YTile |= mask;
                    break;

                case '3':
                    result.XTile |= mask;
                    result.YTile |= mask;
                    break;

                default:
                    Logger?.Error( "Unexpected quadkey character '{0}'", quadKey[ levelOfDetail - 1 ] );
                    break;
            }
        }

        return true;
    }
}
