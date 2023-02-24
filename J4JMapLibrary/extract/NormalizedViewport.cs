﻿// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
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

public class NormalizedViewport : INormalizedViewport
{
    private readonly IProjection _projection;

    private float _latitude;
    private float _longitude;
    private int _scale;

    public NormalizedViewport(
        IProjection projection
    )
    {
        _projection = projection;
    }

    public float CenterLatitude
    {
        get => _latitude;
        set => _latitude = _projection.LatitudeRange.ConformValueToRange( value, "Latitude" );
    }

    public float CenterLongitude
    {
        get => _longitude;
        set => _longitude = _projection.LongitudeRange.ConformValueToRange( value, "Longitude" );
    }

    public float RequestedHeight { get; set; }
    public float RequestedWidth { get; set; }

    public int Scale
    {
        get => _scale;
        set => _scale = _projection.ScaleRange.ConformValueToRange( value, "Scale" );
    }
}
