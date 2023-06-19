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

using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.VisualUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private IMapRegion? _mapRegion;

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
            if( !MapExtensions.TryParseToLatLong( value, out var latitude, out var longitude ) )
            {
                _logger?.LogError( "Could not parse center '{center}' to latitude/longitude, ignoring", value );
                return;
            }

            SetValue( CenterProperty, value );

            SetMapCenterPoint( latitude, longitude );
            SetMapRectangle();
        }
    }

    private void SetMapCenterPoint( float latitude, float longitude )
    {
        if( _projection == null )
        {
            MapCenterPoint = null;
            return;
        }

        MapCenterPoint = new MapPoint( _projection, (int) MapScale );
        MapCenterPoint.SetLatLong( latitude, longitude );
    }

    public MapPoint? MapCenterPoint { get; private set; }

    public Rectangle2D? MapRectangle { get; private set; }
    public Vector3 MapUpperLeft { get; private set; }

    private void SetMapRectangle()
    {
        if( MapCenterPoint == null )
        {
            MapRectangle = null;
            MapUpperLeft = Vector3.Zero;
        }
        else
        {
            MapRectangle = new Rectangle2D( ActualSize.Y,
                                            ActualSize.X,
                                            MapRotation,
                                            new Vector3( MapCenterPoint.X, MapCenterPoint.Y, 0 ),
                                            CoordinateSystem2D.Display );

            MapUpperLeft = MapRectangle.OrderByDescending(c => c.X).ThenBy(c => c.Y).First();
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

    public float MapRotation => ( 360 - (float) MapHeading ) % 360;

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

        set
        {
            var castValue = (int) value;

            if( _projection != null )
                castValue = _projection.ScaleRange.ConformValueToRange( castValue, "MapScale" );

            SetValue( MapScaleProperty, castValue );

            if( MapCenterPoint == null )
                return;

            SetMapCenterPoint( MapCenterPoint.Latitude, MapCenterPoint.Longitude );
            SetMapRectangle();
        }
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
        if( _projection == null || MapCenterPoint == null )
            return;

        var region = new Region
        {
            Heading = (float) MapHeading,
            Height = height,
            Width = width,
            CenterPoint = MapCenterPoint,
            MapStyle = MapStyle,
            Scale = (int) MapScale,
            ShrinkStyle = ShrinkStyle
        };

        Zoom = _projection.GetRegionZoom( region );

        if( Zoom != null )
        {
            region.Height = Zoom.Value * region.Height;
            region.Width = Zoom.Value * region.Width;
        }

        _mapRegion = await _projection.LoadRegionAsync( region, ctx );

        if( _mapRegion?.ImagesLoaded ?? false )
            UpdateDisplay();
        else _logger?.LogError( "Failed to load region" );
    }
}
