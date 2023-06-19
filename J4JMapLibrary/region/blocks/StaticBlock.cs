using System.Numerics;
using J4JSoftware.VisualUtilities;

namespace J4JSoftware.J4JMapLibrary;

public class StaticBlock : MapBlock
{
    public StaticBlock(
        StaticProjection projection,
        Region region
    )
        : base( projection, region.Scale )
    {
        var area = region.Area ?? 
            throw new ArgumentException( $"{typeof( Region )} does not contain a valid area" );

        Height = area.Height;
        Width = area.Width;
        Bounds = area;
        CenterPoint = region.CenterPoint!;

        FragmentId =
            $"{MapExtensions.LatitudeToText( CenterPoint.Latitude )}-{MapExtensions.LongitudeToText( CenterPoint.Longitude )}-{Scale}{GetStyleKey( Projection )}-{Projection.TileHeightWidth}-{Projection.TileHeightWidth}";
    }

    public StaticBlock(
        StaticProjection projection,
        int column,
        int row,
        int scale
    )
        : base( projection, scale )
    {
        Height = projection.TileHeightWidth;
        Width = Height;

        // determine the center point of the tile
        var upperLeftX = column * projection.TileHeightWidth;
        var upperLeftY = row * projection.TileHeightWidth;
        CenterPoint = new MapPoint( projection, scale );
        CenterPoint.SetCartesian( upperLeftX + projection.TileHeightWidth / 2,
                                  upperLeftY + projection.TileHeightWidth / 2 );

        Bounds = new Rectangle2D( Height,
                                  Width,
                                  0f,
                                  new Vector3( Width / 2, Height / 2, 0 ),
                                  CoordinateSystem2D.Display );

        FragmentId =
            $"{MapExtensions.LatitudeToText( CenterPoint.Latitude )}-{MapExtensions.LongitudeToText( CenterPoint.Longitude )}-{Scale}{GetStyleKey( Projection )}-{Projection.TileHeightWidth}-{Projection.TileHeightWidth}";
    }

    public MapPoint CenterPoint { get; init; }
}
