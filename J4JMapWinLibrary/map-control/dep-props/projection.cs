using System;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapLibrary.MapRegion;
using J4JSoftware.WindowsAppUtilities;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly ThrottleDispatcher _throttleRegionChanges = new();

    private IProjection? _projection;
    private ProjectionFactory? _projFactory;

    public ProjectionFactory? ProjectionFactory
    {
        get => _projFactory;

        set
        {
            _projFactory = value;
            _projFactory?.ScanAssemblies();
        }
    }

    public DependencyProperty MapProjectionProperty = DependencyProperty.Register( nameof( MapProjection ),
        typeof( string ),
        typeof( J4JMapControl ),
        new PropertyMetadata( null, OnMapProjectionChanged ) );

    public DependencyProperty MapStyleProperty = DependencyProperty.Register( nameof( MapStyle ),
                                                                              typeof( string ),
                                                                              typeof( J4JMapControl ),
                                                                              new PropertyMetadata( null,
                                                                                  OnMapProjectionChanged ) );

    public string MapProjection
    {
        get => (string) GetValue( MapProjectionProperty );
        set => SetValue( MapProjectionProperty, value );
    }

    public string MapStyle
    {
        get => (string) GetValue( MapStyleProperty );
        set => SetValue( MapStyleProperty, value );
    }

    private static void OnMapProjectionChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if( d is not J4JMapControl mapControl )
            return;

        mapControl.UpdateProjection();
    }

    private void UpdateProjection()
    {
        if( ProjectionFactory == null )
        {
            _logger?.Error("ProjectionFactory is not defined");
            return;
        }

        var cache = _tileMemCache ?? _tileFileCache;

        var projResult = ProjectionFactory.CreateProjection( MapProjection, cache );
        if( !projResult.ProjectionTypeFound )
        {
            Logger?.Fatal("Could not create projection '{0}'", MapProjection);
            throw new InvalidOperationException( $"Could not create projection '{MapProjection}'" );
        }

        if( !projResult.Authenticated )
        {
            Logger?.Fatal( $"Could not authenticate projection '{0}'", MapProjection );
            throw new InvalidOperationException( $"Could not authenticate projection '{MapProjection}'" );
        }

        _projection = projResult.Projection!;
        _projection.LoadComplete += ( _, _ ) => _dispatcherQueue.TryEnqueue( LoadMapImages );

        if( !Extensions.TryParseToLatLong( Center, out var latitude, out var longitude ) )
            Logger?.Error("Could not parse Center ('{0}') to latitude/longitude, defaulting to 0/0", Center);

        if( MapRegion != null )
        {
            MapRegion.ConfigurationChanged -= MapRegionConfigurationChanged;
            MapRegion.BuildUpdated -= MapRegionBuildUpdated;
        }

        MapRegion = new MapRegion( _projection, Logger )
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
}
