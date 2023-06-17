#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// region.cs
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

using System.Threading;
using System.Threading.Tasks;
using ABI.System.Numerics;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private float _centerLatitude;
    private float _centerLongitude;
    private ILoadedRegion? _loadedRegion;

    public IRegionView? RegionView { get; private set; }

    public float? Zoom { get; private set; }

    public DependencyProperty CenterProperty = DependencyProperty.Register( nameof( Center ),
                                                                            typeof( string ),
                                                                            typeof( J4JMapControl ),
                                                                            new PropertyMetadata( "0N, 0W" ) );

    public string Center
    {
        get => (string) GetValue( CenterProperty );

        set
        {
            SetValue( CenterProperty, value );

            if( !MapExtensions.TryParseToLatLong( value, out _centerLatitude, out _centerLongitude ) )
                _logger?.LogError( "Could not parse center '{center}' to latitude/longitude, defaulting to (0,0)",
                                   value );
        }
    }

    public DependencyProperty HeadingProperty = DependencyProperty.Register( nameof( MapHeading ),
                                                                             typeof( double ),
                                                                             typeof( J4JMapControl ),
                                                                             new PropertyMetadata( 0D ) );

    public double MapHeading
    {
        get => (double) GetValue( HeadingProperty );

        set
        {
            SetValue(HeadingProperty, value);
            PositionCompassRose();
        }
    }

    public double MapRotation => ( 360 - MapHeading ) % 360;

    public DependencyProperty MapScaleProperty = DependencyProperty.Register( nameof( MapScale ),
                                                                              typeof( double ),
                                                                              typeof( J4JMapControl ),
                                                                              new PropertyMetadata( 0.0d ) );

    public double MapScale
    {
        get
        {
            var retVal = (double) GetValue( MapScaleProperty );

            return retVal < MinMapScale
                ? MinMapScale
                : retVal > MaxMapScale
                    ? MaxMapScale
                    : retVal;
        }
        set => SetValue( MapScaleProperty, value );
    }

    public DependencyProperty MapStyleProperty = DependencyProperty.Register( nameof( MapStyle ),
                                                                              typeof( string ),
                                                                              typeof( J4JMapControl ),
                                                                              new PropertyMetadata( null ) );

    public string? MapStyle
    {
        get => (string?) GetValue( MapStyleProperty );
        set => SetValue( MapStyleProperty, value );
    }

    private async Task LoadRegion( float height, float width, CancellationToken ctx = default )
    {
        if( RegionView == null )
            return;

        var region = new Region
        {
            Heading = (float) MapHeading,
            Height = height,
            Width = width,
            Latitude = _centerLatitude,
            Longitude = _centerLongitude,
            MapStyle = MapStyle,
            Scale = (int) MapScale,
            ShrinkStyle = ShrinkStyle
        };

        Zoom = RegionView.GetZoom( region );

        if( Zoom != null )
        {
            region.Height = Zoom.Value * region.Height;
            region.Width = Zoom.Value * region.Width;
        }

        _loadedRegion = await RegionView.LoadRegionAsync( region, ctx );

        if( _loadedRegion.Succeeded )
            UpdateDisplay();
        else
            _logger?.LogError( "Failed to load region from {regionView}", nameof( RegionView ) );
    }
}
