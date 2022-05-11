using System.Runtime.InteropServices.ComTypes;

namespace J4JSoftware.MapLibrary;

public record BoundingBox
{
    public const double MinDelta = 1e-9;

    public BoundingBox(
        IMapProjection mapProjection,
        LatLong centerLatLong,
        double winUiWidth,
        double winUiHeight
    )
    {
        var ulTile = mapProjection.GetTileFromLatLong( centerLatLong, -winUiWidth / 2, -winUiHeight / 2 );
        var lrTile = mapProjection.GetTileFromLatLong( centerLatLong, winUiWidth / 2, winUiHeight / 2 );

        UpperLeft = new MultiCoordinates( ulTile, mapProjection, CoordinateOrigin.UpperLeft );
        LowerRight = new MultiCoordinates( lrTile, mapProjection, CoordinateOrigin.UpperLeft );

        HorizontalTiles = LowerRight.TilePoint.X - UpperLeft.TilePoint.X + 1;
        VerticalTiles = LowerRight.TilePoint.Y - UpperLeft.TilePoint.Y + 1;

        Width = HorizontalTiles * mapProjection.TileWidthHeight;
        Height = VerticalTiles * mapProjection.TileWidthHeight;
    }

    public MultiCoordinates UpperLeft { get; }
    public MultiCoordinates LowerRight { get; }

    public int HorizontalTiles { get; }
    public int VerticalTiles { get; }

    public double Width { get; }
    public double Height { get; }

    public IEnumerable<MultiCoordinates> GetTileCoordinates( IMapProjection mapProjection )
    {
        for( var yTile = UpperLeft.TilePoint.Y; yTile <= LowerRight.TilePoint.Y; yTile++ )
        {
            for( var xTile = UpperLeft.TilePoint.X; xTile <= LowerRight.TilePoint.X; xTile++ )
            {
                yield return mapProjection.GetTileCoordinates( xTile, yTile, CoordinateOrigin.UpperLeft );
            }
        }
    }
}