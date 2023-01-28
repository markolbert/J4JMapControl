using System.Text;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;

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

    public static string ToQuadKey( this MapTile tile )
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

    public static (int Scale, int XTile, int YTile) ToTileCoordinates( string quadKey )
    {
        (int scale, int x, int y ) retVal = ( 0, 0, 0 );

        if( quadKey.Length < 1 )
        {
            Logger?.Error( "Empty quadkey string" );
            return retVal;
        }

        retVal.scale = quadKey.Length;

        if( !quadKey.Any( x => ValidQuadKeyCharacters.Any( y => y == x ) ) )
        {
            Logger?.Error( "Invalid characters in quadkey string" );
            return retVal;
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
                    retVal.x |= mask;
                    break;

                case '2':
                    retVal.y |= mask;
                    break;

                case '3':
                    retVal.x |= mask;
                    retVal.y |= mask;
                    break;

                default:
                    Logger?.Error( "Unexpected quadkey character '{0}'", quadKey[ levelOfDetail - 1 ] );
                    break;
            }
        }

        return retVal;
    }
}
