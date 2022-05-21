using System.Collections.Generic;
using Windows.Foundation;
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
    private int _zoomLevel;
    private Size _mapSize;
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
            if( !retriever.IsInitialized)
                continue;
            
            temp.Add( new Retriever( retriever.MapRetrieverInfo!.Description, retriever ) );
        }

        Retrievers = temp;
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
                                   _selectedRetriever.Item.MapRetrieverInfo!,
                                   out var initLocation ) )
                    Location = initLocation;

            }

            OnPropertyChanged(nameof(MinZoom));
            OnPropertyChanged(nameof(MaxZoom));

            _suppressUpdate = false;

            ZoomLevel = MinZoom;
        }
    }

    public int MinZoom => _selectedRetriever?.Item.MapRetrieverInfo!.MinimumZoom ?? 0;
    public int MaxZoom => _selectedRetriever?.Item.MapRetrieverInfo!.MaximumZoom ?? 0;

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
        }
    }

    public Size MapSize
    {
        get => _mapSize;
        set => SetProperty(ref _mapSize, value);
    }

    public string? ErrorMessage
    {
        get => _errorMsg;
        set => SetProperty( ref _errorMsg, value );
    }

    public LatLong? Location
    {
        get => _location;
        set => SetProperty(ref _location, value);
    }
}