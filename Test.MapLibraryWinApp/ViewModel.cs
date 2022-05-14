using System;
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
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IJ4JLogger _logger;

    private SelectableItem<IMapImageRetriever>? _selectedRetriever;
    private int _xTile;
    private int _yTile;
    private int _zoomLevel;
    private HorizontalBinder _horizBinder;
    private VerticalBinder _vertBinder;

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

        var temp = new List<SelectableItem<IMapImageRetriever>>();

        foreach( var retriever in retrievers )
        {
            retriever.MapProjection = mapProjection;
            
            temp.Add( new Retriever( retriever.MapRetrieverInfo.Description, retriever ) );
        }

        Retrievers = temp;

        HorizontalBinders = Enum.GetValues<SmallMapHorizontalAlignment>()
                                .Select( x => new HorizontalBinder( x ) )
                                .ToList();

        HorizontalBinder = HorizontalBinders.First( x => x.Name.Equals( "Center", StringComparison.OrdinalIgnoreCase ) );

        VerticalBinders = Enum.GetValues<SmallMapVerticalAlignment>()
                              .Select( x => new VerticalBinder( x ) )
                              .ToList();

        VerticalBinder = VerticalBinders.First(x => x.Name.Equals("Middle", StringComparison.OrdinalIgnoreCase));
    }

    public List<SelectableItem<IMapImageRetriever>> Retrievers { get; }

    public SelectableItem<IMapImageRetriever>? SelectedRetriever
    {
        get=> _selectedRetriever;

        set
        {
            if (_suppressUpdate)
                return;

            _suppressUpdate = true;

            SetProperty(ref _selectedRetriever, value);

            if( _selectedRetriever != null )
            {
                if( LatLong.TryParse( "37.5072, -122.2605",
                                   _selectedRetriever.Item.MapRetrieverInfo,
                                   out var initLocation ) )
                    Location = initLocation;

            }

            OnPropertyChanged(nameof(MinZoom));
            OnPropertyChanged(nameof(MaxZoom));
            OnPropertyChanged(nameof(MaxTile));

            _suppressUpdate = false;

            ZoomLevel = MinZoom;

            UpdateTileMap();
        }
    }

    public int MinZoom => _selectedRetriever?.Item.MapRetrieverInfo.MinimumZoom ?? 0;
    public int MaxZoom => _selectedRetriever?.Item.MapRetrieverInfo.MaximumZoom ?? 0;
    public int MaxTile => _selectedRetriever?.Item.MapProjection.ZoomFactor ?? 0;

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
                _selectedRetriever.Item.MapProjection.ZoomLevel = _zoomLevel;

            UpdateTileMap();
        }
    }

    public List<HorizontalBinder> HorizontalBinders { get; }

    public HorizontalBinder HorizontalBinder
    {
        get => _horizBinder;

        set
        {
            if( _suppressUpdate )
                return;

            _suppressUpdate = true;
            SetProperty( ref _horizBinder, value );
            _suppressUpdate = false;
        }
    }

    public List<VerticalBinder> VerticalBinders { get; }

    public VerticalBinder VerticalBinder
    {
        get => _vertBinder;

        set
        {
            if( _suppressUpdate )
                return;

            _suppressUpdate = true;
            SetProperty( ref _vertBinder, value );
            _suppressUpdate = false;
        }
    }

    private void UpdateTileMap()
    {
        ErrorMessage = null;
        TileImageSource = null;

        if( Location == null
        || _selectedRetriever?.Item.MapRetrieverInfo is not {} _ )
            return;

        var tile = new MultiCoordinates( new TilePoint( _xTile, _yTile, ZoomLevel ),
                                         _selectedRetriever.Item.MapProjection,
                                         CoordinateOrigin.UpperLeft );

        _selectedRetriever.Item.GetMapImageAsync( tile )
                          .ContinueWith( t =>
                           {
                               _dQueue.TryEnqueue( () =>
                               {
                                   if( t.Result is not { ReturnValue: MapImageData imageData } )
                                   {
                                       ErrorMessage = "Failed to retrieve image";
                                       return;
                                   }

                                   var newImage = new BitmapImage();
                                   imageData.Stream.Seek( 0 );
                                   newImage.SetSource( imageData.Stream );

                                   TileImageSource = newImage;
                               } );
                           } );
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

    public int TileWidth=>TileImageSource == null ? 0 : TileImageSource.PixelWidth;
    public int TileHeight => TileImageSource == null ? 0 : TileImageSource.PixelHeight;

    public LatLong? Location
    {
        get => _location;
        set => SetProperty(ref _location, value);
    }
}