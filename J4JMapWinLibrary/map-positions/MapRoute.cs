using System;
using J4JSoftware.WindowsUtilities;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace J4JSoftware.J4JMapWinLibrary;

public class MapRoute : DependencyObject
{
    private const double DefaultMarkerHeightWidth = 5;
    private const double DefaultStrokeWidth = 2;

    public event EventHandler? Changed;

    private readonly ThrottleDispatcher _throttleChanges = new();

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

    public static readonly DependencyProperty MarkerProperty = DependencyProperty.Register( nameof( Marker ),
        typeof( Shape ),
        typeof( J4JMapControl ),
        new PropertyMetadata( new Ellipse
        {
            Height = DefaultMarkerHeightWidth,
            Width = DefaultMarkerHeightWidth,
            Fill = new SolidColorBrush( Colors.Black )
        } ) );

    public Shape Marker
    {
        get => (Shape) GetValue( MarkerProperty );
        set => SetValue( MarkerProperty, value );
    }

    public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register( nameof( Stroke ),
        typeof( Brush ),
        typeof( J4JMapControl ),
        new PropertyMetadata( new SolidColorBrush( Colors.Black ) ) );

    public SolidColorBrush Stroke
    {
        get => (SolidColorBrush) GetValue( StrokeProperty );
        set => SetValue( StrokeProperty, value );
    }

    public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(nameof(Width),
        typeof(double),
        typeof(J4JMapControl),
        new PropertyMetadata(DefaultStrokeWidth));

    public double Width
    {
        get => (double)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    private void RouteSourceUpdated( object? sender, EventArgs e )
    {
        _throttleChanges.Throttle( UpdateEventInterval, _ => Changed?.Invoke( this, EventArgs.Empty ) );
    }
}
