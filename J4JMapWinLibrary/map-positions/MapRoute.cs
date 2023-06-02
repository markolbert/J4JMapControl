using System;
using System.Reflection;
using Windows.UI;
using J4JSoftware.WindowsUtilities;
using Microsoft.UI;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public class MapRoute : DependencyObject
{
    private const double DefaultStrokeWidth = 5;
    private const double DefaultStrokeTransparency = 0.3;

    public event EventHandler? Changed;

    private readonly ThrottleDispatcher _throttleRouteSourceChanges = new();
    private readonly ThrottleDispatcher _throttleRouteVisualChanges = new();

    private int _updateInterval = J4JMapControl.DefaultUpdateEventInterval;

    public RoutePositions? RoutePositions { get; private set; }

    public int UpdateEventInterval
    {
        get => _updateInterval;
        set => _updateInterval = value <= 0 ? J4JMapControl.DefaultUpdateEventInterval : value;
    }

    public static readonly DependencyProperty RouteNameProperty =
        DependencyProperty.Register( nameof( RouteName ),
                                     typeof( string ),
                                     typeof( MapRoute ),
                                     new PropertyMetadata( null ) );

    public string RouteName
    {
        get => (string) GetValue( RouteNameProperty );
        set => SetValue( RouteNameProperty, value );
    }

    public static readonly DependencyProperty LatitudeFieldProperty =
        DependencyProperty.Register( nameof( LatitudeField ),
                                     typeof( string ),
                                     typeof( J4JMapControl ),
                                     new PropertyMetadata( null ) );

    public string? LatitudeField
    {
        get => (string?) GetValue( LatitudeFieldProperty );

        set
        {
            SetValue( LatitudeFieldProperty, value );

            if( RoutePositions == null )
                InitializeRoutePositions();
            else RoutePositions.LatitudeProperty = value;
        }
    }

    private void InitializeRoutePositions()
    {
        if( RoutePositions != null || string.IsNullOrEmpty( RouteName ) )
            return;

        RoutePositions = new RoutePositions( RouteName, this, x => x.PointTemplate )
        {
            LatitudeProperty = LatitudeField,
            LongitudeProperty = LongitudeField,
            LatLongProperty = LatLongField,
            Source = DataSource
        };

        RoutePositions.SourceUpdated += RouteSourceUpdated;
    }

    public static readonly DependencyProperty LongitudeFieldProperty =
        DependencyProperty.Register( nameof( LongitudeField ),
                                     typeof( string ),
                                     typeof( J4JMapControl ),
                                     new PropertyMetadata( null ) );

    public string? LongitudeField
    {
        get => (string?) GetValue( LongitudeFieldProperty );

        set
        {
            SetValue( LongitudeFieldProperty, value );

            if( RoutePositions == null )
                InitializeRoutePositions();
            else RoutePositions.LongitudeProperty = value;
        }
    }

    public static readonly DependencyProperty LatLongFieldProperty =
        DependencyProperty.Register( nameof( LatLongField ),
                                     typeof( string ),
                                     typeof( J4JMapControl ),
                                     new PropertyMetadata( null ) );

    public string? LatLongField
    {
        get => (string?) GetValue( LatLongFieldProperty );

        set
        {
            SetValue( LatLongFieldProperty, value );

            if( RoutePositions == null )
                InitializeRoutePositions();
            else RoutePositions.LatLongProperty = value;
        }
    }

    public static readonly DependencyProperty PointVisibilityFieldProperty =
        DependencyProperty.Register( nameof( PointVisibilityField ),
                                     typeof( string ),
                                     typeof( J4JMapControl ),
                                     new PropertyMetadata( null ) );

    public string? PointVisibilityField
    {
        get => (string?) GetValue( PointVisibilityFieldProperty );

        set
        {
            SetValue( PointVisibilityFieldProperty, value );

            if( RoutePositions == null )
                InitializeRoutePositions();
            else RoutePositions.PositionVisibilityProperty = value;
        }
    }

    internal PropertyInfo? PointVisibilityPropertyInfo { get; set; }

    public static readonly DependencyProperty DataSourceProperty =
        DependencyProperty.Register( nameof( DataSource ),
                                     typeof( object ),
                                     typeof( J4JMapControl ),
                                     new PropertyMetadata( null ) );

    public object? DataSource
    {
        get => GetValue( DataSourceProperty );

        set
        {
            SetValue( DataSourceProperty, value );

            if( RoutePositions == null )
                InitializeRoutePositions();
            else RoutePositions.Source = value;
        }
    }

    public DataTemplate? PointTemplate { get; set; }

    public static readonly DependencyProperty StrokeColorProperty = DependencyProperty.Register( nameof( StrokeColor ),
        typeof( Color ),
        typeof( J4JMapControl ),
        new PropertyMetadata( Colors.Black ) );

    public Color StrokeColor
    {
        get => (Color) GetValue( StrokeColorProperty );

        set
        {
            SetValue( StrokeColorProperty, value );
            _throttleRouteVisualChanges.Throttle( UpdateEventInterval, _ => Changed?.Invoke( this, EventArgs.Empty ) );
        }
    }

    public static readonly DependencyProperty StrokeWidthProperty = DependencyProperty.Register( nameof( StrokeWidth ),
        typeof( double ),
        typeof( J4JMapControl ),
        new PropertyMetadata( DefaultStrokeWidth ) );

    public double StrokeWidth
    {
        get => (double) GetValue( StrokeWidthProperty );

        set
        {
            SetValue( StrokeWidthProperty, value );
            _throttleRouteVisualChanges.Throttle( UpdateEventInterval, _ => Changed?.Invoke( this, EventArgs.Empty ) );
        }
    }

    public static readonly DependencyProperty ShowPointsProperty = DependencyProperty.Register( nameof( ShowPoints ),
        typeof( bool ),
        typeof( J4JMapControl ),
        new PropertyMetadata( false ) );

    public bool ShowPoints
    {
        get => (bool) GetValue( ShowPointsProperty );

        set
        {
            SetValue( ShowPointsProperty, value );
            _throttleRouteVisualChanges.Throttle( UpdateEventInterval, _ => Changed?.Invoke( this, EventArgs.Empty ) );
        }
    }

    public static readonly DependencyProperty StrokeOpacityProperty = DependencyProperty.Register(
        nameof( StrokeOpacity ),
        typeof( double ),
        typeof( J4JMapControl ),
        new PropertyMetadata( DefaultStrokeTransparency ) );

    public double StrokeOpacity
    {
        get => (double) GetValue( StrokeOpacityProperty );

        set
        {
            SetValue( StrokeOpacityProperty, value < 0 ? DefaultStrokeTransparency : value > 1 ? 1 : value );
            _throttleRouteVisualChanges.Throttle( UpdateEventInterval, _ => Changed?.Invoke( this, EventArgs.Empty ) );
        }
    }

    private void RouteSourceUpdated( object? sender, EventArgs e )
    {
        _throttleRouteSourceChanges.Throttle( UpdateEventInterval, _ => Changed?.Invoke( this, EventArgs.Empty ) );
    }
}
