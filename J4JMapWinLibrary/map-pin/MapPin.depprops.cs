using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class MapPin
{
    public static DependencyProperty ArcRadiusProperty = DependencyProperty.Register( nameof( ArcRadius ),
        typeof( double ),
        typeof( MapPin ),
        new PropertyMetadata( 15D ) );

    public static DependencyProperty TailLengthProperty =
        DependencyProperty.Register( nameof( TailLength ),
                                     typeof( double ),
                                     typeof( MapPin ),
                                     new PropertyMetadata( 30D ) );

    public static DependencyProperty FillProperty = DependencyProperty.Register( nameof( Fill ),
        typeof( Brush ),
        typeof( MapPin ),
        new PropertyMetadata( new SolidColorBrush( Color.FromArgb( 128, 255, 0, 0 ) ) ) );

    public static DependencyProperty StrokeProperty = DependencyProperty.Register( nameof( Stroke ),
        typeof( Brush ),
        typeof( MapPin ),
        new PropertyMetadata( new SolidColorBrush( Color.FromArgb( 0, 0, 0, 0 ) ) ) );

    public static DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register( nameof( StrokeThickness ),
                                     typeof( double ),
                                     typeof( MapPin ),
                                     new PropertyMetadata( 0D ) );

    public double ArcRadius
    {
        get => (double) GetValue( ArcRadiusProperty );

        set
        {
            value = value <= 0 ? 15 : value;
            SetValue( ArcRadiusProperty, value );

            InitializePin();
        }
    }

    public double TailLength
    {
        get => (double) GetValue( TailLengthProperty );

        set
        {
            value = value <= 0 ? 30 : value;
            SetValue( TailLengthProperty, value );

            InitializePin();
        }
    }

    public Brush Fill
    {
        get => (Brush) GetValue( FillProperty );
        set => SetValue( FillProperty, value );
    }

    public Brush Stroke
    {
        get => (Brush) GetValue( StrokeProperty );
        set => SetValue( StrokeProperty, value );
    }

    public double StrokeThickness
    {
        get => (double) GetValue( StrokeThicknessProperty );

        set
        {
            value = value < 0 ? 0 : value;

            SetValue( StrokeThicknessProperty, value );
        }
    }
}
