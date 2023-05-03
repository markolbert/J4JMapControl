#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// controls.cs
//
// This file is part of JumpForJoy Software's J4JMapWinLibrary.
// 
// J4JMapWinLibrary is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JMapWinLibrary is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JMapWinLibrary. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using Windows.Foundation;
using Windows.UI;
using J4JSoftware.WindowsUtilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using WinRT;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private readonly ThrottleDispatcher _throttleSliderSizeChange = new();
    private Grid? _controlGrid;

    public DependencyProperty ControlVisibilityProperty = DependencyProperty.Register( nameof( ControlVisibility ),
        typeof( bool ),
        typeof( J4JMapControl ),
        new PropertyMetadata( true ) );

    public bool ControlVisibility
    {
        get => (bool)GetValue(ControlVisibilityProperty);
        set => SetValue(ControlVisibilityProperty, value);
    }

    public DependencyProperty MaxControlHeightProperty = DependencyProperty.Register(nameof(MaxControlHeight),
        typeof(double),
        typeof(J4JMapControl),
        new PropertyMetadata(double.MaxValue));

    public double MaxControlHeight
    {
        get => (double)GetValue(MaxControlHeightProperty);

        set
        {
            SetValue( MaxControlHeightProperty, value <= 0 ? DefaultControlHeight : value );
            SetControlGridSizes( new Size( ActualWidth, ActualHeight ) );
        }
    }

    public DependencyProperty HorizontalControlAlignmentProperty = DependencyProperty.Register(
        nameof( HorizontalControlAlignment ),
        typeof( HorizontalAlignment ),
        typeof( J4JMapControl ),
        new PropertyMetadata( HorizontalAlignment.Right ) );

    public HorizontalAlignment HorizontalControlAlignment
    {
        get => (HorizontalAlignment)GetValue(HorizontalControlAlignmentProperty);
        set => SetValue(HorizontalControlAlignmentProperty, value);
    }

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

    public static DependencyProperty CompassRoseImageProperty = DependencyProperty.Register(nameof(CompassRoseImage),
        typeof(BitmapImage),
        typeof(J4JMapControl),
        new PropertyMetadata(GetDefaultCompassRoseImage()));

    public BitmapImage CompassRoseImage
    {
        get => (BitmapImage) GetValue( CompassRoseImageProperty );
        set => SetValue( CompassRoseImageProperty, value );
    }

    public DependencyProperty CompassRoseHeightWidthProperty = DependencyProperty.Register(
        nameof(CompassRoseHeightWidth),
        typeof(double),
        typeof(J4JMapControl),
        new PropertyMetadata(100D));

    public double CompassRoseHeightWidth
    {
        get => (double) GetValue( CompassRoseHeightWidthProperty );
        set
        {
            SetValue( CompassRoseHeightWidthProperty, value );
            SetControlGridSizes( new Size( this.ActualWidth, this.ActualHeight ) );
        }
    }

    public DependencyProperty ControlBackgroundProperty = DependencyProperty.Register( nameof( ControlBackground ),
        typeof( Color ),
        typeof( J4JMapControl ),
        new PropertyMetadata( Color.FromArgb( 255, 128, 128, 128 ) ) );

    public Color ControlBackground
    {
        get => (Color) GetValue( ControlBackgroundProperty );
        set => SetValue( ControlBackgroundProperty, value );
    }

    public DependencyProperty ControlBackgroundOpacityProperty = DependencyProperty.Register(
        nameof(ControlBackgroundOpacity),
        typeof(double),
        typeof(J4JMapControl),
        new PropertyMetadata(0.6));

    public double ControlBackgroundOpacity
    {
        get => (double) GetValue( ControlBackgroundOpacityProperty );
        set => SetValue( ControlBackgroundOpacityProperty, value );
    }

    public DependencyProperty ControlVerticalMarginProperty = DependencyProperty.Register(
        nameof(ControlVerticalMargin),
        typeof(double),
        typeof(J4JMapControl),
        new PropertyMetadata(5D));

    public double ControlVerticalMargin
    {
        get => (double)GetValue(ControlVerticalMarginProperty);

        set
        {
            SetValue(ControlVerticalMarginProperty, value);
            SetMapControlMargins(value);
        }
    }

    public DependencyProperty LargeMapScaleChangeProperty = DependencyProperty.Register(nameof(LargeMapScaleChange),
        typeof(double),
        typeof(J4JMapControl),
        new PropertyMetadata(0.6));

    public double LargeMapScaleChange
    {
        get => (double) GetValue( LargeMapScaleChangeProperty );

        set
        {
            value = value <= 0 ? 1 : 0;
            SetValue( LargeMapScaleChangeProperty, value );
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

    private void SetControlGridSizes( Size mapSize )
    {
        if( _controlGrid != null )
            _controlGrid.Height = mapSize.Height;

        if( _scaleSlider == null )
            return;

        var sliderHeight = _compassRose != null
            ? mapSize.Height - _compassRose.Height + _compassRose.Margin.Top + _compassRose.Margin.Bottom
            : mapSize.Height - _scaleSlider.Margin.Top - _scaleSlider.Margin.Bottom;

        if( sliderHeight < 20 )
            sliderHeight = 20;

        _scaleSlider.Height = sliderHeight;
    }
}
