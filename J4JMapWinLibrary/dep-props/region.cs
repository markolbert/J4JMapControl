using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapLibrary.MapRegion;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public DependencyProperty MapScaleProperty = DependencyProperty.Register(nameof(MapScale),
                                                                             typeof(double),
                                                                             typeof(J4JMapControl),
                                                                             new PropertyMetadata( 0.0d));

    public MapRegion? MapRegion { get; private set; }

    public string Center
    {
        get => (string) GetValue( CenterProperty );
        set => SetValue( CenterProperty, value );
    }

    public double MapScale
    {
        get => (double) GetValue( MapScaleProperty );

        set
        {
            value = value < MinMapScale
                ? MinMapScale
                : value > MaxMapScale
                    ? MaxMapScale
                    : value;

            SetValue( MapScaleProperty, value );

            MapRegion?.Scale( (int) value );
        }
    }

    public double Heading
    {
        get => (double) GetValue( HeadingProperty );
        set => SetValue( HeadingProperty, value );
    }

    public DependencyProperty MapRotationProperty = DependencyProperty.Register(
        nameof(MapRotation),
        typeof(double),
        typeof(J4JMapControl),
        new PropertyMetadata(0));

    public double MapRotation
    {
        get => (double)GetValue(MapRotationProperty);
        set => SetValue(MapRotationProperty, value);
    }

}
