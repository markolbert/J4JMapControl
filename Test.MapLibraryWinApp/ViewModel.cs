using System;
using System.Collections.Generic;
using System.ComponentModel;
using J4JSoftware.J4JMapControl;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Serilog.Core;

namespace Test.MapLibraryWinApp;

public class ViewModel : ObservableObject
{
    private readonly DispatcherQueue _dQueue;
    private readonly IJ4JLogger _logger;

    private RetrieverInfo? _selectedRetriever;
    private int _xTile;
    private int _yTile;
    private int _tileZoom = 1;
    private ImageSource? _tileImage;
    private string? _errorMsg;

    private LatLong? _location;
    private int _mapZoom = 1;

    public ViewModel(
        IEnumerable<IMapImageRetriever> retrievers,
        IJ4JLogger logger
    )
    {
        _logger = logger;
        _logger.SetLoggedType(GetType());

        _dQueue = DispatcherQueue.GetForCurrentThread();

        var temp = new List<RetrieverInfo>();

        foreach( var retriever in retrievers )
        {
            if( retriever.MapRetrieverInfo != null )
                temp.Add( new RetrieverInfo( retriever.MapRetrieverInfo.Description, retriever ) );
            else _logger?.Error( "Could not initialize {0}", retriever.GetType() );
        }

        Retrievers = temp;

        LoadMapTileCommand = new RelayCommand( LoadMapTileHandler );

        if( LatLong.TryParse( "37.5072, -122.2605", out var initLocation ) )
            Location = initLocation;
    }

    public RelayCommand LoadMapTileCommand { get; }

    private void LoadMapTileHandler() => GetTile();

    public List<RetrieverInfo> Retrievers { get; }

    public RetrieverInfo? SelectedRetriever
    {
        get=> _selectedRetriever;

        set
        {
            SetProperty( ref _selectedRetriever, value );

            GetTile();
        }
    }

    public int XTile
    {
        get => _xTile;

        set
        {
            SetProperty( ref _xTile, value );
            GetTile();
        }
    }

    public int YTile
    {
        get => _yTile;

        set
        {
            SetProperty(ref _yTile, value);
            GetTile();
        }
    }

    public int TileZoom
    {
        get => _tileZoom;

        set
        {
            SetProperty(ref _tileZoom, value);
            GetTile();
        }
    }

    private void GetTile()
    {
        TileImage = null;
        ErrorMessage = null;

        if( _selectedRetriever == null || _selectedRetriever.Retriever.MapRetrieverInfo is not {} info )
            return;

        var tile = new ScreenTileGlobalCoordinates(ScreenPoint.Empty.ToDoublePoint(),
                                            new IntPoint(XTile, YTile),
                                            LatLong.GetEmpty(info),
                                            new Zoom(TileZoom, info));

        _selectedRetriever.Retriever.GetMapImageAsync(tile)
                          .ContinueWith((t) => _dQueue.TryEnqueue(() =>
                           {
                               TileImage = (t.Result.ReturnValue as Image)?.Source;
                               ErrorMessage = t.Result.Message;
                           }));
    }

    public ImageSource? TileImage
    {
        get => _tileImage;

        set
        {
            SetProperty( ref _tileImage, value );

            OnPropertyChanged( nameof( TileHeight ) );
            OnPropertyChanged( nameof( TileWidth ) );
        }
    }

    public string? ErrorMessage
    {
        get => _errorMsg;
        set => SetProperty( ref _errorMsg, value );
    }

    public int TileWidth=>TileImage == null ? 0 : ((BitmapSource) TileImage).PixelWidth;
    public int TileHeight => TileImage == null ? 0 : ((BitmapSource)TileImage).PixelHeight;

    [TypeConverter(typeof(LatLongTypeConverter))]
    public LatLong? Location
    {
        get => _location;

        set
        {
            SetProperty( ref _location, value );
        }
    }

    public int MapZoom
    {
        get=> _mapZoom;

        set
        {
            SetProperty( ref _mapZoom, value );
        }
    }
}