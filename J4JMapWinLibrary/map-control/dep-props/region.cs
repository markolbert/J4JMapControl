using System;
using System.ComponentModel;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapLibrary.MapRegion;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public DependencyProperty CenterProperty = DependencyProperty.Register( nameof( Center ),
                                                                            typeof( string ),
                                                                            typeof( J4JMapControl ),
                                                                            new PropertyMetadata( "0N, 0W" ) );

    public DependencyProperty HeadingProperty = DependencyProperty.Register( nameof( Heading ),
                                                                             typeof( double ),
                                                                             typeof( J4JMapControl ),
                                                                             new PropertyMetadata( 0D ) );

    public DependencyProperty MapScaleProperty = DependencyProperty.Register( nameof( MapScale ),
                                                                              typeof( double ),
                                                                              typeof( J4JMapControl ),
                                                                              new PropertyMetadata( 0.0d ) );

    public MapRegion? MapRegion { get; private set; }

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

        set
        {
            MapRegion?.Heading( (float) value );

            // we call SetValue after updating MapRegion so that
            // modulus 360 logic can be applied
            SetValue( HeadingProperty, MapRegion?.Heading ?? value );

            PositionCompassRose();
        }
    }

    public string Center
    {
        get => (string) GetValue( CenterProperty );

        set
        {
            SetValue( CenterProperty, value );

            if( !Extensions.TryParseToLatLong( value, out var latitude, out var longitude ) )
            {
                Logger?.Error( "Could not parse center '{0}' to latitude/longitude, defaulting to (0,0)",
                               value );
            }

            MapRegion?.Center( latitude, longitude );
        }
    }

    private void MapGridOnPointerWheelChanged( object sender, PointerRoutedEventArgs e )
    {
        e.Handled = true;
        var point = e.GetCurrentPoint( this );
        MapScale += point.Properties.MouseWheelDelta < 0 ? -1 : 1;
    }

    private void MapRegionBuildUpdated( object? sender, RegionBuildResults e )
    {
        switch( e.Change )
        {
            case MapRegionChange.Empty:
            case MapRegionChange.NoChange:
                break;

            case MapRegionChange.OffsetChanged:
                SetImagePanelTransforms( e );
                IncludeAnnotations();
                break;

            case MapRegionChange.LoadRequired:
                _projection!.LoadRegionAsync( MapRegion! );
                SetImagePanelTransforms( e );
                IncludeAnnotations();
                break;

            default:
                throw new InvalidEnumArgumentException( $"Unsupported {typeof( MapRegionChange )} value '{e.Change}'" );
        }
    }

    private void MapRegionConfigurationChanged( object? sender, EventArgs e )
    {
        _throttleRegionChanges.Throttle( UpdateEventInterval, _ => MapRegion!.Update() );
    }
}
