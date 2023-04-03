using System;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public static DependencyProperty CompassRoseImageProperty = DependencyProperty.Register( nameof( CompassRoseImage ),
        typeof( BitmapImage ),
        typeof( J4JMapControl ),
        new PropertyMetadata( GetDefaultCompassRoseImage() ) );

    public DependencyProperty CompassRoseHeightWidthProperty = DependencyProperty.Register(
        nameof( CompassRoseHeightWidth ),
        typeof( double ),
        typeof( J4JMapControl ),
        new PropertyMetadata( 100D ) );

    public DependencyProperty ControlBackgroundOpacityProperty = DependencyProperty.Register(
        nameof( ControlBackgroundOpacity ),
        typeof( double ),
        typeof( J4JMapControl ),
        new PropertyMetadata( 0.6 ) );

    public DependencyProperty ControlBackgroundProperty = DependencyProperty.Register( nameof( ControlBackground ),
        typeof( Color ),
        typeof( J4JMapControl ),
        new PropertyMetadata( 0 ) );

    public DependencyProperty ControlVerticalMarginProperty = DependencyProperty.Register(
        nameof( ControlVerticalMargin ),
        typeof( double ),
        typeof( J4JMapControl ),
        new PropertyMetadata( 5D ) );

    public DependencyProperty ControlVisibilityProperty = DependencyProperty.Register( nameof( ControlVisibility ),
        typeof( bool ),
        typeof( J4JMapControl ),
        new PropertyMetadata( true ) );

    public DependencyProperty HorizontalControlAlignmentProperty = DependencyProperty.Register(
        nameof( HorizontalControlAlignment ),
        typeof( HorizontalAlignment ),
        typeof( J4JMapControl ),
        new PropertyMetadata( HorizontalAlignment.Right ) );

    public DependencyProperty LargeMapScaleChangeProperty = DependencyProperty.Register( nameof( LargeMapScaleChange ),
        typeof( double ),
        typeof( J4JMapControl ),
        new PropertyMetadata( 0.6 ) );

    public DependencyProperty VerticalControlAlignmentProperty = DependencyProperty.Register(
        nameof( VerticalControlAlignment ),
        typeof( VerticalAlignment ),
        typeof( J4JMapControl ),
        new PropertyMetadata( VerticalAlignment.Top ) );

    public VerticalAlignment VerticalControlAlignment
    {
        get => (VerticalAlignment) GetValue( VerticalControlAlignmentProperty );
        set => SetValue( VerticalControlAlignmentProperty, value );
    }

    public HorizontalAlignment HorizontalControlAlignment
    {
        get => (HorizontalAlignment) GetValue( HorizontalControlAlignmentProperty );
        set => SetValue( HorizontalControlAlignmentProperty, value );
    }

    public bool ControlVisibility
    {
        get => (bool) GetValue( ControlVisibilityProperty );
        set => SetValue( ControlVisibilityProperty, value );
    }

    public BitmapImage CompassRoseImage
    {
        get => (BitmapImage) GetValue( CompassRoseImageProperty );
        set => SetValue( CompassRoseImageProperty, value );
    }

    public double CompassRoseHeightWidth
    {
        get => (double) GetValue( CompassRoseHeightWidthProperty );
        set => SetValue( CompassRoseHeightWidthProperty, value );
    }

    public Color ControlBackground
    {
        get => (Color) GetValue( ControlBackgroundProperty );
        set => SetValue( ControlBackgroundProperty, value );
    }

    public double ControlBackgroundOpacity
    {
        get => (double) GetValue( ControlBackgroundOpacityProperty );
        set => SetValue( ControlBackgroundOpacityProperty, value );
    }

    public double LargeMapScaleChange
    {
        get => (double) GetValue( LargeMapScaleChangeProperty );

        set
        {
            value = value <= 0 ? 1 : 0;
            SetValue( LargeMapScaleChangeProperty, value );
        }
    }

    public double ControlVerticalMargin
    {
        get => (double) GetValue( ControlVerticalMarginProperty );

        set
        {
            SetValue( ControlVerticalMarginProperty, value );
            SetMapControlMargins( value );
        }
    }

    private static BitmapImage GetDefaultCompassRoseImage()
    {
        var uri = new Uri( "ms-appx:///media/rose.png" );
        return new BitmapImage( uri );
    }

    private void SetMapControlMargins( double value )
    {
        if( _compassRose != null )
            _compassRose.Margin = new Thickness( 0, 2 * value, 0, value );

        if( _scaleSlider != null )
            _scaleSlider.Margin = new Thickness( 0, value, 0, 2 * value );
    }
}
