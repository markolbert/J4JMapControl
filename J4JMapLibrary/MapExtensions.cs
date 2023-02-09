using System.Text;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public static class MapExtensions
{
    private static readonly IJ4JLogger? Logger;
    private static readonly char[] ValidQuadKeyCharacters = new[] { '0', '1', '2', '3' };

    static MapExtensions()
    {
        Logger = J4JDeusEx.GetLogger();
        Logger?.SetLoggedType( typeof( MapExtensions ) );
    }

    public static string GetQuadKey( this FixedMapTile tile )
    {
        var retVal = new StringBuilder();

        for( var i = tile.Scope.Scale; i > tile.Scope.ScaleRange.Minimum - 1; i-- )
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

    public static string? GetQuadKey( this IFixedTileProjection projection, int xTile, int yTile )
    {
        var x = projection.TileXRange.ConformValueToRange( xTile, "X Tile" );
        var y = projection.TileYRange.ConformValueToRange( yTile, "Y Tile" );

        var scope = (FixedTileScope) projection.GetScope();

        if( x != xTile || y != yTile )
        {
            Logger?.Error( "Tile coordinates ({0}, {1}) are inconsistent with projection's scale {2}",
                           xTile,
                           yTile,
                           scope.Scale );

            return null;
        }

        var retVal = new StringBuilder();

        for( var i = scope.Scale; i > 0; i-- )
        {
            var digit = '0';
            var mask = 1 << ( i - 1 );

            if( ( xTile & mask ) != 0 )
                digit++;

            if( ( yTile & mask ) != 0 )
            {
                digit++;
                digit++;
            }

            retVal.Append( digit );
        }

        return retVal.ToString();
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

    public static LatLong CenterLatLong( this FixedMapTile tile ) =>
        tile.Scope.CartesianToLatLong( tile.CenterCartesian() );

    public static Cartesian CenterCartesian( this FixedMapTile tile )
    {
        var retVal = new Cartesian( tile.Scope );

        retVal.SetCartesian( tile.X * tile.HeightWidth + tile.HeightWidth / 2,
                             tile.Y * tile.HeightWidth + tile.HeightWidth / 2 );

        return retVal;
    }
}
