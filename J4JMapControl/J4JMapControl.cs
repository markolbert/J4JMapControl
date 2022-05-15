using System.ComponentModel;
using Windows.Foundation;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.J4JMapControl;

public sealed partial class J4JMapControl : Panel, IMapContext
{
    private static double Tolerance = 0.1;

    private readonly IJ4JLogger? _logger;

    private IMapProjection? _mapProjection;
    private BoundingBox? _boundingBox;

    public J4JMapControl()
    {
        _logger = J4JDeusEx.ServiceProvider.GetRequiredService<IJ4JLogger>();
        _logger?.SetLoggedType( GetType() );

        SizeChanged += ( _, args ) => OnSizeChangedAsync( args );

        SmallMapHorizontalBinding = SmallMapHorizontalAlignment.Center;
        SmallMapVerticalBinding = SmallMapVerticalAlignment.Middle;
    }

    private async Task OnMapImageRetrieverChanged(IMapImageRetriever retriever)
    {
        _mapProjection = new MercatorProjection { MapRetrieverInfo = retriever.MapRetrieverInfo };
        retriever.MapProjection = _mapProjection;

        if( Center == null )
            return;

        await UpdateMap();
    }

    private async Task OnZoomLevelChanged( int zoom )
    {
        if( MapRetriever == null )
            return;

        MapRetriever.MapProjection.ZoomLevel = zoom;

        await UpdateMap();
    }

    private async Task OnMapCenterChanged( LatLong? center )
    {
        if( center == null && Visibility != Visibility.Collapsed )
            Visibility = Visibility.Collapsed;
        else await UpdateMap();
    }

    private async void OnSizeChangedAsync(SizeChangedEventArgs args)
    {
        if( _mapProjection == null || Center == null || args.NewSize.Width > MaxWidth ||
            args.NewSize.Height > MaxHeight )
            return;

        _mapProjection.ViewportWidth = args.NewSize.Width;

        await UpdateMap();
    }

    public async Task UpdateMap()
    {
        if( Center == null || MapRetriever == null || _mapProjection == null )
            return;

        _boundingBox = new BoundingBox( _mapProjection, Center, ActualWidth, ActualHeight );

        var retrievalResult = await MapRetriever
           .GetMapImagesAsync( _boundingBox, Children.MapImages().ExtractCoordinates() );

        if( retrievalResult.ReturnValue == null )
            return;

        Children.SetMapTileState( MapTileState.NotInBoundingBox );

        foreach( var imgData in retrievalResult.ReturnValue!.Cast<MapImageData>() )
        {
            // only add tiles that aren't already in the child collection
            var curImage = Children.GetMapTile( imgData.Coordinates );

            if( curImage != null )
            {
                AttachedProperties.SetMapTileState(curImage, MapTileState.InUse);
                continue;
            }

            curImage = new Image();

            AttachedProperties.SetCoordinates( curImage, imgData.Coordinates );
            AttachedProperties.SetIsMapTile( curImage, true );
            AttachedProperties.SetIsFixedImageSize( curImage, MapRetriever.FixedSizeImages );
            AttachedProperties.SetMapTileState(curImage, MapTileState.InUse);

            imgData.Stream.Seek(0);
            var imgSource = new BitmapImage();
            imgSource.SetSource(imgData.Stream);
            curImage.Source = imgSource;

            Children.Add( curImage );
        }

        Children.RemoveMapImages( MapTileState.NotInBoundingBox, MapTileState.NotSet );
    }

    protected override Size MeasureOverride( Size availableSize )
    {
        // not sure why I have to do this guard action, but returning anything
        // with an infinity, or an empty Size, blows up the WinUI library
        if( double.IsPositiveInfinity( availableSize.Width ) )
            availableSize.Width = 100;
        
        if( double.IsPositiveInfinity( availableSize.Height ) )
            availableSize.Height = 100;

        if ( Center == null || !Children.OfType<Image>().Where( AttachedProperties.GetIsMapTile ).Any() )
            return availableSize;

        return MeasureMapLayer( availableSize );
    }

    private Size MeasureMapLayer( Size retVal )
    {
        if( _mapProjection == null )
            return retVal;

        double desiredWidth;
        double desiredHeight;

        var varSizedImage = Children.VariableSizedMapImages().FirstOrDefault();

        if( varSizedImage != null )
        {
            // there should only ever by a single variable-sized map image
            var varSize = new Size( ( (BitmapSource)varSizedImage.Source ).PixelWidth,
                ( (BitmapSource)varSizedImage.Source ).PixelHeight );

            desiredWidth = varSize.Width;
            desiredHeight = varSize.Height;

            // map images don't resize -- they stay the same size as when they're created
            varSizedImage.Measure( varSize );
        }
        else
        {
            // process fixed-size map images
            var minXTile = int.MaxValue;
            var minYTile = int.MaxValue;
            var maxXTile = 0;
            var maxYTile = 0;

            foreach( var image in Children.FixedSizedMapImages() )
            {
                // map images don't resize -- they stay the same size as when they're created
                image.Measure( new Size( ( (BitmapSource)image.Source ).PixelWidth,
                    ( (BitmapSource)image.Source ).PixelHeight ) );

                var coords = AttachedProperties.GetCoordinates( image );
                if( coords == null )
                    continue;

                minXTile = coords.TilePoint.X < minXTile ? coords.TilePoint.X : minXTile;
                minYTile = coords.TilePoint.Y < minYTile ? coords.TilePoint.Y : minYTile;
                maxXTile = coords.TilePoint.X > maxXTile ? coords.TilePoint.X : maxXTile;
                maxYTile = coords.TilePoint.Y > maxYTile ? coords.TilePoint.Y : maxYTile;
            }

            desiredWidth = _mapProjection.TileWidthHeight * ( maxXTile - minXTile + 1 );
            desiredHeight = _mapProjection.TileWidthHeight * ( maxYTile - minYTile + 1 );
        }

        if( desiredWidth < retVal.Width )
            retVal.Width = desiredWidth;

        if( desiredHeight < retVal.Height )
            retVal.Height = desiredHeight;

        MaxWidth = _mapProjection.TileWidthHeight * _boundingBox?.HorizontalTiles ?? 1;
        MaxHeight = _mapProjection.TileWidthHeight * _boundingBox?.VerticalTiles ?? 1;

        return retVal;
    }

    private double OffsetX()
    {
        if( Math.Abs( _boundingBox?.GetDesiredCenterOffset( CoordinateAxis.XAxis) ?? 0) < Tolerance )
            return (ActualWidth - _boundingBox?.Width ?? 0) / 2;

        return 0.0;
    }

    private double OffsetY()
    {
        if (Math.Abs(_boundingBox?.GetDesiredCenterOffset(CoordinateAxis.YAxis) ?? 0) < Tolerance)
            return (ActualHeight - _boundingBox?.Height ?? 0) / 2;

        return 0.0;
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
        if( Center == null
        || MapRetriever == null
        || !Children.MapImages().Any() )
            return finalSize;

        Clip = new RectangleGeometry { Rect = new Rect( new Point( 0, 0 ), finalSize ) };

        // we do this in layers by starting with arranging the map tiles, and
        // then arranging anything else on top of it
        ArrangeMapTiles();

        return finalSize;
    }

    private void ArrangeMapTiles()
    {
        if( _mapProjection == null || _boundingBox == null || MapRetriever == null )
            return;

        var xOffset = SmallMapHorizontalBinding switch
        {
            SmallMapHorizontalAlignment.Center => OffsetX(),
            SmallMapHorizontalAlignment.Left => 0.0,
            SmallMapHorizontalAlignment.Right => ActualWidth - _boundingBox.Width,
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof( SmallMapHorizontalAlignment )} value '{SmallMapHorizontalBinding}'" )
        };

        var yOffset = SmallMapVerticalBinding switch
        {
            SmallMapVerticalAlignment.Middle => OffsetY(),
            SmallMapVerticalAlignment.Top => 0,
            SmallMapVerticalAlignment.Bottom => ActualHeight - _boundingBox.Height,
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof(SmallMapVerticalAlignment)} value '{SmallMapVerticalBinding}'")
        };

        foreach ( var image in Children.MapImages() )
        {
            var coords = AttachedProperties.GetCoordinates( image );
            if( coords == null )
            {
                _logger?.Error<string>( "Map Image lacks {0}", nameof( AttachedProperties.CoordinatesProperty ) );
                continue;
            }

            var finalRect = AttachedProperties.GetIsFixedImageSize( image ) 
                ? GetFixedTileRect( coords, xOffset, yOffset ) 
                : null;

            if( finalRect != null )
                image.Arrange( finalRect.Value );
        }
    }

    private Rect? GetFixedTileRect( MultiCoordinates coordinates, double xOffset, double yOffset )
    {
        var upperLeftX = coordinates.ScreenPoint.GetX( CoordinateOrigin.UpperLeft )
          - _boundingBox!.UpperLeft.ScreenPoint.GetX( CoordinateOrigin.UpperLeft );

        var upperLeftY = coordinates.ScreenPoint.GetY( CoordinateOrigin.UpperLeft )
          - _boundingBox.UpperLeft.ScreenPoint.GetY( CoordinateOrigin.UpperLeft );

        var xPosition = upperLeftX + xOffset;
        var yPosition = upperLeftY + yOffset;

        return new( xPosition,
                    yPosition,
                    _mapProjection!.TileWidthHeight,
                    _mapProjection.TileWidthHeight );
    }
}