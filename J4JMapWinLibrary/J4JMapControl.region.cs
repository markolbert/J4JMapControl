using J4JSoftware.J4JMapLibrary.MapRegion;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public MapRegion? MapRegion { get; private set; }

    public string Center
    {
        get => (string) GetValue( CenterProperty );
        set => SetValue( CenterProperty, value );
    }

    public double MapScale
    {
        get => (double) GetValue( MapScaleProperty );
        set => SetValue( MapScaleProperty, value );
    }

    public double MinScale
    {
        get => (double) GetValue( MinScaleProperty );
        private set => SetValue( MinScaleProperty, value );
    }

    public double MaxScale
    {
        get => (double) GetValue( MaxScaleProperty );
        private set => SetValue( MaxScaleProperty, value );
    }

    public double Heading
    {
        get => (double) GetValue( HeadingProperty );
        set => SetValue( HeadingProperty, value );
    }
}
