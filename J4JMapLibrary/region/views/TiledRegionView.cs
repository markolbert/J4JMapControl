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

    public List<PositionedMapBlock> PositionedBlocks { get; } = new();

    protected override async Task<ILoadedRegion> LoadRegionInternalAsync(
        Rectangle2D requestedArea,
        CancellationToken ctx
    )
    {
        if( Center == null )
            return LoadedTiledRegion.Empty;

        var zoom = DefineBlocksAndOffset( requestedArea );

        var loaded = await Projection.LoadBlocksAsync( PositionedBlocks.Select( b => b.MapBlock ), ctx );
        OnImagesLoaded( loaded );

        return new LoadedTiledRegion( zoom, loaded ? PositionedBlocks : null );
    }

    private float? DefineBlocksAndOffset( Rectangle2D requestedArea )
    {
        var heightWidth = Projection.GetHeightWidth( RequestedRegion.Scale );
        var projRectangle = new Rectangle2D( heightWidth, heightWidth, coordinateSystem: CoordinateSystem2D.Display );

        var shrinkResult = projRectangle.ShrinkToFit( requestedArea, RequestedRegion.ShrinkStyle );
        LoadedArea = shrinkResult.Rectangle;

        var tilesHighWide = Projection.GetNumTiles( RequestedRegion.Scale );

        var top = LoadedArea.Min( c => c.Y );
        var firstRow = (int) Math.Floor( top / Projection.TileHeightWidth );
        var lastRow = (int) Math.Ceiling( LoadedArea.Max( c => c.Y ) / Projection.TileHeightWidth );

        var left = LoadedArea.Min( c => c.X );
        var firstCol = (int) Math.Floor( left / Projection.TileHeightWidth );
        var lastCol = (int) Math.Ceiling( LoadedArea.Max( c => c.X ) / Projection.TileHeightWidth );

        var centerCol = (int) Math.Ceiling( Center!.X / Projection.TileHeightWidth );

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

                PositionedBlocks.Add( new PositionedMapBlock( row,
                                                              srcCol,
                                                              new TileBlock( (ITiledProjection) Projection,
                                                                             RequestedRegion.Scale,
                                                                             col,
                                                                             row ) ) );
            }
        }

        if( PositionedBlocks.Any() )
        {
            var topFirstIncludedRow = PositionedBlocks.MinBy( b => b.Row )!.Row * Projection.TileHeightWidth;
            var leftFirstIncludedCol = PositionedBlocks.MinBy( b => b.Column )!.Column * Projection.TileHeightWidth;

            LoadedAreaOffset = new Vector3( topFirstIncludedRow - top, leftFirstIncludedCol - left, 0 );
        }
        else LoadedAreaOffset = new Vector3();

        return shrinkResult.Zoom;
    }
}
