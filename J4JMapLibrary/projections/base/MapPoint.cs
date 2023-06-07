#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// MapPoint.cs
//
// This file is part of JumpForJoy Software's J4JMapLibrary.
// 
// J4JMapLibrary is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JMapLibrary is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JMapLibrary. If not, see <https://www.gnu.org/licenses/>.

#endregion

namespace J4JSoftware.J4JMapLibrary;

public class MapPoint
{
    private IProjection _projection;
    private int _scale;
    private bool _suppressUpdate;
    public EventHandler? Changed;

    public MapPoint(
        MapRegion.MapRegion region
    )
        : this( region.Projection, region.Scale )
    {
    }

    public MapPoint(
        IProjection projection,
        int scale
    )
    {
        _projection = projection;
        _scale = scale;
    }

    public float X { get; private set; }
    public float Y { get; private set; }

    public float Latitude { get; private set; }
    public float Longitude { get; private set; }

    public MapPoint Copy()
    {
        var retVal = (MapPoint) MemberwiseClone();
        retVal.Changed = null;
        retVal._projection = _projection;
        retVal._scale = _scale;

        return retVal;
    }

    public void SetCartesian( float? x, float? y )
    {
        if( x == null && y == null )
            return;

        if( x.HasValue )
        {
            // handle potential wrap-around
            var maxX = _projection.GetHeightWidth( _scale );

            X = x.Value % maxX;
            X += X < 0 ? maxX : 0;
        }

        if( y.HasValue )
        {
            var yRange = _projection.GetXYRange( _scale );
            Y = yRange.ConformValueToRange( y.Value, $"{GetType().Name} Y" );
        }

        UpdateLatLong();
    }

    public void OffsetCartesian( float? xOffset, float? yOffset )
    {
        if( xOffset == null && yOffset == null )
            return;

        var x = X + xOffset;
        var y = Y + yOffset;

        SetCartesian( x, y );
    }

    private void UpdateLatLong()
    {
        if( _suppressUpdate )
            return;

        _suppressUpdate = true;

        var heightWidth = (double) _projection.GetHeightWidth( _scale );

        var scaledX = X / heightWidth - 0.5;
        var scaledY = 0.5 - Y / heightWidth;
        var latitude = (float) ( 90 - 360 * Math.Atan( Math.Exp( -scaledY * MapConstants.TwoPi ) ) / Math.PI );
        var longitude = (float) ( 360 * scaledX );

        SetLatLong( latitude, longitude );

        _suppressUpdate = false;
        Changed?.Invoke( this, EventArgs.Empty );
    }

    public void SetLatLong( float? latitude, float? longitude )
    {
        if( latitude == null && longitude == null )
            return;

        if( latitude.HasValue )
            Latitude = _projection.LatitudeRange.ConformValueToRange( latitude.Value, "Latitude" );

        if( longitude.HasValue )
            Longitude = _projection.LongitudeRange.ConformValueToRange( longitude.Value, "Longitude" );

        UpdateCartesian();
    }

    private void UpdateCartesian()
    {
        if( _suppressUpdate )
            return;

        _suppressUpdate = true;

        var heightWidth = _projection.GetHeightWidth( _scale );

        // x == 0 is the left hand edge of the projection (the x/y origin is in
        // the upper left corner)
        var x = (int) Math.Round( heightWidth * ( Longitude / 360 + 0.5 ) );

        // another way of calculating Y...leave as comment for testing
        //var latRadians = Latitude * MapConstants.RadiansPerDegree;
        //var sinRatio = ( 1 + Math.Sin( latRadians ) ) / ( 1 - Math.Sin( latRadians ) );
        //var lnLat = Math.Log( sinRatio );
        //var junk = ( 0.5F - lnLat / MapConstants.FourPi ) * Projection.Height;

        // this weird "subtract the calculation from half the height" is due to the
        // fact y values increase going >>down<< the display, so the top is y = 0
        // while the bottom is y = height
        var y = (int) Math.Round( heightWidth / 2F
                                - heightWidth
                                * Math.Log( Math.Tan( MapConstants.QuarterPi
                                                    + Latitude * MapConstants.RadiansPerDegree / 2 ) )
                                / MapConstants.TwoPi );

        SetCartesian( x, y );

        _suppressUpdate = false;
        Changed?.Invoke( this, EventArgs.Empty );
    }
}
