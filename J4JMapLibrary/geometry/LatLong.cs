// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of ConsoleUtilities.
//
// ConsoleUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// ConsoleUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with ConsoleUtilities. If not, see <https://www.gnu.org/licenses/>.

namespace J4JSoftware.J4JMapLibrary;

public class LatLong
{
    private readonly MinMax<float> _latitudeRange;
    private readonly MinMax<float> _longitudeRange;
    public EventHandler? Changed;

    public LatLong(
        IMapServer server
    )
    {
        _latitudeRange = server.LatitudeRange;
        _longitudeRange = server.LongitudeRange;
    }

    public float Latitude { get; private set; }
    public float Longitude { get; private set; }

    public void SetLatLong( float? latitude, float? longitude )
    {
        if( latitude == null && longitude == null )
            return;

        if( latitude.HasValue )
            Latitude = _latitudeRange.ConformValueToRange( latitude.Value, "Latitude" );

        if( longitude.HasValue )
            Longitude = _longitudeRange.ConformValueToRange( longitude.Value, "Longitude" );

        Changed?.Invoke( this, EventArgs.Empty );
    }

    public void SetLatLong( LatLong latLong )
    {
        Latitude = _latitudeRange.ConformValueToRange( latLong.Latitude, "Latitude" );
        Longitude = _longitudeRange.ConformValueToRange( latLong.Longitude, "Longitude" );

        Changed?.Invoke( this, EventArgs.Empty );
    }
}
