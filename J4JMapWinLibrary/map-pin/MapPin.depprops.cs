#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// MapPin.depprops.cs
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

using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class MapPin
{
    public static readonly DependencyProperty ArcRadiusProperty = DependencyProperty.Register( nameof( ArcRadius ),
        typeof( double ),
        typeof( MapPin ),
        new PropertyMetadata( 15D ) );

    public double ArcRadius
    {
        get => (double)GetValue(ArcRadiusProperty);

        set
        {
            value = value <= 0 ? 15 : value;
            SetValue(ArcRadiusProperty, value);

            InitializePin();
        }
    }

    public static readonly DependencyProperty TailLengthProperty =
        DependencyProperty.Register( nameof( TailLength ),
                                     typeof( double ),
                                     typeof( MapPin ),
                                     new PropertyMetadata( 30D ) );

    public double TailLength
    {
        get => (double)GetValue(TailLengthProperty);

        set
        {
            value = value <= 0 ? 30 : value;
            SetValue(TailLengthProperty, value);

            InitializePin();
        }
    }

    public static readonly DependencyProperty FillProperty = DependencyProperty.Register( nameof( Fill ),
        typeof( Brush ),
        typeof( MapPin ),
        new PropertyMetadata( new SolidColorBrush( Color.FromArgb( 128, 255, 0, 0 ) ) ) );

    public Brush Fill
    {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register( nameof( Stroke ),
        typeof( Brush ),
        typeof( MapPin ),
        new PropertyMetadata( new SolidColorBrush( Color.FromArgb( 0, 0, 0, 0 ) ) ) );

    public Brush Stroke
    {
        get => (Brush) GetValue( StrokeProperty );
        set => SetValue( StrokeProperty, value );
    }

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(nameof(StrokeThickness),
                                    typeof(double),
                                    typeof(MapPin),
                                    new PropertyMetadata(0D));

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
