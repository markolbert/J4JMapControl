using System.Numerics;
using J4JSoftware.VisualUtilities;

namespace J4JSoftware.J4JMapLibrary;

public class StaticBlock : MapBlock
{
    public StaticBlock(
        IRegionView regionView
    )
        : base( regionView.Projection, regionView.RequestedRegion.Scale )
    {
        if( regionView.ProjectionType != ProjectionType.Static )
            throw new ArgumentException(
                $"Expected an {typeof( IRegionView )} based on a {typeof( StaticProjection )} but received a {regionView.Projection.GetType()} instead" );

        Height = regionView.LoadedArea.Height;
        Width = regionView.LoadedArea.Width;
        Bounds = regionView.LoadedArea;

        CenterPoint = regionView.Center 
         ?? throw new ArgumentException( $"{typeof(IRegionView)} is not defined");

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
        Height = projection.GetHeightWidth( scale );
        Width = projection.GetHeightWidth( scale );

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
