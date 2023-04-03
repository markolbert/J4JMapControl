using System;
using System.ComponentModel;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapLibrary.MapRegion;
using J4JSoftware.WindowsAppUtilities;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private readonly ProjectionFactory _projFactory;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly ThrottleDispatcher _throttleRegionChanges = new();

    private IProjection? _projection;

    public DependencyProperty MapProjectionProperty = DependencyProperty.Register( nameof( MapProjection ),
        typeof( string ),
        typeof( J4JMapControl ),
        new PropertyMetadata( null, OnMapProjectionChanged ) );

    private static void OnMapProjectionChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if( d is not J4JMapControl mapControl )
            return;

        mapControl.UpdateProjection();
    }

    private void UpdateProjection()
    {
        if( !_cacheIsValid )
            UpdateCaching();

        var cache = _tileMemCache ?? _tileFileCache;

        var projResult = _projFactory.CreateProjection( MapProjection, cache );
        if( !projResult.ProjectionTypeFound )
        {
            J4JDeusEx.OutputFatalMessage( $"Could not create projection '{MapProjection}'", _logger );
            throw new InvalidOperationException( $"Could not create projection '{MapProjection}'" );
        }

        if( !projResult.Authenticated )
        {
            J4JDeusEx.OutputFatalMessage( $"Could not authenticate projection '{MapProjection}'", _logger );
            throw new InvalidOperationException( $"Could not authenticate projection '{MapProjection}'" );
        }

        _projection = projResult.Projection!;
        _projection.LoadComplete += ( _, _ ) => _dispatcherQueue.TryEnqueue( LoadMapImages );

        if( !Extensions.TryParseToLatLong( Center, out var latitude, out var longitude ) )
            _logger.Error( "Could not parse Center ('{0}') to latitude/longitude, defaulting to 0/0", Center );

        if( MapRegion != null )
        {
            MapRegion.ConfigurationChanged -= MapRegionConfigurationChanged;
            MapRegion.BuildUpdated -= MapRegionBuildUpdated;
        }

        MapRegion = new MapRegion( _projection, _logger )
                   .Center( latitude, longitude )
                   .Scale( (int) MapScale )
                   .Heading( (float) Heading )
                   .Size( (float) Height, (float) Width );

        MapRegion.ConfigurationChanged += MapRegionConfigurationChanged;
        MapRegion.BuildUpdated += MapRegionBuildUpdated;

        MinMapScale = _projection.MinScale;
        MaxMapScale = _projection.MaxScale;

        MapRegion.Update();
    }

    public string MapProjection
    {
        get => (string) GetValue( MapProjectionProperty );
        set => SetValue( MapProjectionProperty, value );
    }

    public DependencyProperty MapStyleProperty = DependencyProperty.Register( nameof( MapStyle ),
                                                                              typeof( string ),
                                                                              typeof( J4JMapControl ),
                                                                              new PropertyMetadata( null,
                                                                                  OnMapProjectionChanged ) );

    public string MapStyle
    {
        get => (string) GetValue( MapStyleProperty );
        set => SetValue( MapStyleProperty, value );
    }
}
