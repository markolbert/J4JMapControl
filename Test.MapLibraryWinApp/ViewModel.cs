using System.Collections.Generic;
using System.Linq;
using J4JSoftware.J4JMapControl;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Test.MapLibraryWinApp;

public class ViewModel : ObservableObject
{
    private readonly DispatcherQueue _dQueue;
    private readonly IJ4JLogger _logger;

    private RetrieverInfo? _selectedRetriever;
    private int _xTile;
    private int _yTile;
    private int _zoomLevel;
    private BitmapImage? _tileImageSource;
    private string? _errorMsg;
    private bool _suppressUpdate;

    private LatLong? _location;

    public ViewModel(
        IMapProjection mapProjection,
        IEnumerable<IMapImageRetriever> retrievers,
        IJ4JLogger logger
    )
    {
        _logger = logger;
        _logger.SetLoggedType( GetType() );

        _dQueue = DispatcherQueue.GetForCurrentThread();

        var temp = new List<RetrieverInfo>();

        foreach( var retriever in retrievers )
        {
            retriever.MapProjection = mapProjection;
            
            temp.Add( new RetrieverInfo( retriever.MapRetrieverInfo.Description, retriever ) );
        }

        Retrievers = temp;

        if( LatLong.TryParse( "37.5072, -122.2605",
                              Retrievers.First().Retriever.MapRetrieverInfo!,
                              out var initLocation ) )
            Location = initLocation;
    }

    public List<RetrieverInfo> Retrievers { get; }

    public RetrieverInfo? SelectedRetriever
    {
        get=> _selectedRetriever;

        set
        {
            if (_suppressUpdate)
                return;

            _suppressUpdate = true;

            SetProperty(ref _selectedRetriever, value);
            
            OnPropertyChanged(nameof(MinZoom));
            OnPropertyChanged(nameof(MaxZoom));
            OnPropertyChanged(nameof(MaxTile));

            _suppressUpdate = false;

            ZoomLevel = MinZoom;

            UpdateTileMap();
        }
    }

    public int MinZoom => _selectedRetriever?.Retriever.MapRetrieverInfo.MinimumZoom ?? 0;
    public int MaxZoom => _selectedRetriever?.Retriever.MapRetrieverInfo.MaximumZoom ?? 0;
    public int MaxTile => _selectedRetriever?.Retriever.MapProjection.ZoomFactor ?? 0;

    public int XTile
    {
        get => _xTile;

        set
        {
            if (_suppressUpdate)
                return;

            _suppressUpdate = true;
            SetProperty(ref _xTile, value);
            _suppressUpdate = false;

            UpdateTileMap();
        }
    }

    public int YTile
    {
        get => _yTile;

        set
        {
            if (_suppressUpdate)
                return;

            _suppressUpdate = true;
            SetProperty(ref _yTile, value);
            _suppressUpdate = false;

            UpdateTileMap();
        }
    }

    public int ZoomLevel
    {
        get => _zoomLevel;

        set
        {
            if (_suppressUpdate)
                return;

            _suppressUpdate = true;
            SetProperty(ref _zoomLevel, value);
            _suppressUpdate = false;

            if( _selectedRetriever != null )
                _selectedRetriever.Retriever.MapProjection.ZoomLevel = _zoomLevel;

            UpdateTileMap();
        }
    }

    private void UpdateTileMap()
    {
        ErrorMessage = null;
        TileImageSource = null;

        if( Location == null
        || _selectedRetriever?.Retriever.MapRetrieverInfo is not {} info )
            return;

        var midPt = _selectedRetriever.Retriever.MapProjection.ProjectionWidthHeight / 2;

        var tile = new MultiCoordinates( new TilePoint( _xTile, _yTile, ZoomLevel ),
                                         _selectedRetriever.Retriever.MapProjection,
                                         CoordinateOrigin.UpperLeft );

        var result = _selectedRetriever.Retriever.GetMapImageAsync( tile )
                                       .ContinueWith( t =>
                                        {
                                            _dQueue.TryEnqueue( () =>
                                            {
                                                if (t.Result is not { ReturnValue: MapImageData imageData })
                                                {
                                                    ErrorMessage = "Failed to retrieve image";
                                                    return;
                                                }

                                                var newImage = new BitmapImage();
                                                imageData.Stream.Seek(0);
                                                newImage.SetSource(imageData.Stream);

                                                TileImageSource = newImage;
                                            });
                                        });
    }

    public BitmapImage? TileImageSource
    {
        get => _tileImageSource;

        set
        {
            SetProperty( ref _tileImageSource, value );

            OnPropertyChanged( nameof( TileHeight ) );
            OnPropertyChanged( nameof( TileWidth ) );
        }
    }

    public string? ErrorMessage
    {
        get => _errorMsg;
        set => SetProperty( ref _errorMsg, value );
    }

    public int TileWidth=>TileImageSource == null ? 0 : ((BitmapSource) TileImageSource).PixelWidth;
    public int TileHeight => TileImageSource == null ? 0 : ((BitmapSource)TileImageSource).PixelHeight;

    public LatLong? Location
    {
        get => _location;

        set
        {
            if (_suppressUpdate)
                return;

            _suppressUpdate = true;
            SetProperty(ref _location, value);
            _suppressUpdate = false;
        }
    }
}