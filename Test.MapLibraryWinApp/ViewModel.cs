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
    private int _tileZoom = 1;
    private BitmapImage? _tileImageSource;
    private string? _errorMsg;
    private bool _suppressUpdate;

    private LatLong? _location;
    private int _mapZoom = 1;

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
            _suppressUpdate = false;

            GetTile();
        }
    }

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

            GetTile();
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

            GetTile();
        }
    }

    public int TileZoom
    {
        get => _tileZoom;

        set
        {
            if (_suppressUpdate)
                return;

            _suppressUpdate = true;
            SetProperty(ref _tileZoom, value);
            _suppressUpdate = false;

            GetTile();
        }
    }

    private void GetTile()
    {
        ErrorMessage = null;
        TileImageSource = null;

        if( Location == null
        || _selectedRetriever?.Retriever.MapRetrieverInfo is not {} info )
            return;

        var midPt = _selectedRetriever.Retriever.MapProjection.ProjectionWidthHeight / 2;

        var tile = new MultiCoordinates( new TilePoint( _xTile, _yTile, MapZoom ),
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

                                                TileImageSource = new BitmapImage();
                                                imageData.Stream.Seek(0);
                                                TileImageSource.SetSource(imageData.Stream);
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

    public int MapZoom
    {
        get=> _mapZoom;

        set
        {
            if (_suppressUpdate)
                return;

            _suppressUpdate = true;
            SetProperty(ref _mapZoom, value);
            _suppressUpdate = false;
        }
    }
}