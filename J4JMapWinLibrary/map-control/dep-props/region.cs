using System;
using System.ComponentModel;
using System.Threading.Tasks;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapLibrary.MapRegion;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public MapRegion? MapRegion { get; private set; }

    public DependencyProperty CenterProperty = DependencyProperty.Register( nameof( Center ),
                                                                            typeof( string ),
                                                                            typeof( J4JMapControl ),
                                                                            new PropertyMetadata( "0N, 0W" ) );

    public string Center
    {
        get => (string)GetValue(CenterProperty);

        set
        {
            SetValue(CenterProperty, value);

            if (!Extensions.TryParseToLatLong(value, out var latitude, out var longitude))
            {
                _logger?.LogError("Could not parse center '{0}' to latitude/longitude, defaulting to (0,0)",
                                  value);
            }

            MapRegion?.Center(latitude, longitude);
        }
    }

    public DependencyProperty HeadingProperty = DependencyProperty.Register( nameof( Heading ),
                                                                             typeof( double ),
                                                                             typeof( J4JMapControl ),
                                                                             new PropertyMetadata( 0D ) );

    public double Heading
    {
        get => (double)GetValue(HeadingProperty);

        set
        {
            MapRegion?.Heading((float)value);

            // we call SetValue after updating MapRegion so that
            // modulus 360 logic can be applied
            SetValue(HeadingProperty, MapRegion?.Heading ?? value);

            PositionCompassRose();
        }
    }

    public DependencyProperty MapScaleProperty = DependencyProperty.Register( nameof( MapScale ),
                                                                              typeof( double ),
                                                                              typeof( J4JMapControl ),
                                                                              new PropertyMetadata( 0.0d ) );

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

    public DependencyProperty MapStyleProperty = DependencyProperty.Register(nameof(MapStyle),
                                                                             typeof(string),
                                                                             typeof(J4JMapControl),
                                                                             new PropertyMetadata(null));

    public string? MapStyle
    {
        get => (string?) GetValue( MapStyleProperty );

        set
        {
            SetValue( MapStyleProperty, value );
            MapRegion?.MapStyle( value );
        }
    }

    private async void MapRegionBuildUpdated( object? sender, MapRegionChange change )
    {
        switch( change )
        {
            case MapRegionChange.Empty:
            case MapRegionChange.NoChange:
                break;

            case MapRegionChange.OffsetChanged:
                SetImagePanelTransforms();
                IncludeAnnotations();
                break;

            case MapRegionChange.LoadRequired:
                await _projection!.LoadRegionAsync( MapRegion! );
                SetImagePanelTransforms();
                IncludeAnnotations();
                break;

            default:
                throw new InvalidEnumArgumentException( $"Unsupported {typeof( MapRegionChange )} value '{change}'" );
        }
    }

    private void MapRegionConfigurationChanged( object? sender, EventArgs e )
    {
        _throttleRegionChanges.Throttle( UpdateEventInterval, _ => MapRegion!.Update() );
    }
}
