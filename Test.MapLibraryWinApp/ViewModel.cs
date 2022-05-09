using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
        return;
        TileImage = null;
        ErrorMessage = null;

        if( _selectedRetriever.Retriever.MapRetrieverInfo is not {} info )
            return;

        var tile = new TileCoordinates( new TilePoint( _xTile, _yTile ),
                                        new MercatorProjection
                                        {
                                            MapRetrieverInfo = _selectedRetriever.Retriever.MapRetrieverInfo
                                        } );

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