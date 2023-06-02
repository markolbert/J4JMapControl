using System.ComponentModel;

namespace J4JSoftware.J4JMapLibrary.MapRegion;

public record TileCoordinates( MapRegion Region, int X, int Y )
{
    public (float X, float Y) GetUpperLeftCartesian() =>
        Region.ProjectionType switch
        {
            ProjectionType.Static => ( Region.Center.X - Region.BoundingBox.Width / 2,
                                       Region.Center.Y - Region.BoundingBox.Height / 2 ),
            ProjectionType.Tiled => ( X * Region.Projection.TileHeightWidth, Y * Region.Projection.TileHeightWidth ),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof( ProjectionType )} value '{Region.ProjectionType}'" )
        };
}
