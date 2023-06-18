using J4JSoftware.VisualUtilities;
using System.Numerics;

namespace J4JSoftware.J4JMapLibrary;

public class TiledRegionView : RegionView<ITiledProjection>
{
    public TiledRegionView(
        ITiledProjection projection
    )
        : base(projection, ProjectionType.Tiled)
    {
    }

    public override async Task<ILoadedRegion?> LoadRegionAsync(
        Region region,
        CancellationToken ctx = default( CancellationToken )
    )
    {
        var retVal = DefineBlocksAndOffset( region );
        if( retVal == null )
            return null;

        retVal.ImagesLoaded = await Projection.LoadBlocksAsync( retVal.Blocks
                                                                   .Select( b => b.MapBlock ), ctx );
        
        OnImagesLoaded( retVal.ImagesLoaded );

        return retVal;
    }

    private LoadedTiledRegion? DefineBlocksAndOffset( Region region )
    {
        var area = region.Area;
        if( area == null )
            return null;

        var heightWidth = Projection.GetHeightWidth( region.Scale );
        var projRectangle = new Rectangle2D( heightWidth, heightWidth, coordinateSystem: CoordinateSystem2D.Display );

        var shrinkResult = projRectangle.ShrinkToFit( area, region.ShrinkStyle );

        var tilesHighWide = Projection.GetNumTiles( region.Scale );

        var top = shrinkResult.Rectangle.Min( c => c.Y );
        var firstRow = (int) Math.Floor( top / Projection.TileHeightWidth );
        var lastRow = (int) Math.Ceiling( shrinkResult.Rectangle.Max( c => c.Y ) / Projection.TileHeightWidth );

        var left = shrinkResult.Rectangle.Min( c => c.X );
        var firstCol = (int) Math.Floor( left / Projection.TileHeightWidth );
        var lastCol = (int) Math.Ceiling( shrinkResult.Rectangle.Max( c => c.X ) / Projection.TileHeightWidth );

        var centerCol = (int) Math.Ceiling( region.CenterPoint!.X / Projection.TileHeightWidth );

        var retVal = new LoadedTiledRegion { Zoom = shrinkResult.Zoom };

        for( var row = firstRow; row <= lastRow; row++ )
        {
            if( row < 0 || row >= tilesHighWide )
                continue;

            for( var col = firstCol; col <= lastCol; col++ )
            {
                var srcCol = col;

                if( col < 0 )
                {
                    srcCol = col + tilesHighWide;
                    if( srcCol < 0 || srcCol >= centerCol )
                        continue;
                }
                else
                {
                    if( col >= tilesHighWide )
                    {
                        srcCol = col - tilesHighWide;
                        if( srcCol >= tilesHighWide || srcCol <= centerCol )
                            continue;
                    }
                }

                retVal.Blocks.Add( new PositionedMapBlock( row,
                                                           srcCol,
                                                           new TileBlock( (ITiledProjection) Projection,
                                                                          region.Scale,
                                                                          col,
                                                                          row ) ) );
            }
        }

        var topFirstIncludedRow = retVal.Blocks.MinBy( b => b.Row )!.Row * Projection.TileHeightWidth;
        var leftFirstIncludedCol = retVal.Blocks.MinBy( b => b.Column )!.Column * Projection.TileHeightWidth;

        retVal.Offset = new Vector3( topFirstIncludedRow - top, leftFirstIncludedCol - left, 0 );

        return retVal;
    }
}
