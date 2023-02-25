// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
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

public sealed class StaticFragment : MapFragment, IStaticFragment
{
    public StaticFragment(
        IProjection projection,
        INormalizedViewport viewport
    )
        : this( projection,
                viewport.CenterLatitude,
                viewport.CenterLongitude,
                viewport.Scale,
                viewport.RequestedHeight,
                viewport.RequestedWidth )
    {
    }

    public StaticFragment(
        IProjection projection,
        float centerLatitude,
        float centerLongitude,
        int scale,
        float height,
        float width
    )
        : base( projection )
    {
        CenterLatitude = Projection.LatitudeRange.ConformValueToRange( centerLatitude, "StaticFragment Latitude" );
        CenterLongitude = Projection.LongitudeRange.ConformValueToRange( centerLongitude, "StaticFragment Latitude" );
        Scale = scale;
        RequestedHeight = height;
        RequestedWidth = width;

        ImageHeight = (int) Math.Ceiling( height );
        ImageWidth = (int) Math.Ceiling( width );

        FragmentId =
            $"{MapExtensions.LatitudeToText( CenterLatitude )}-{MapExtensions.LongitudeToText( CenterLongitude )}-{Scale}-{ImageHeight}-{ImageWidth}";
    }

    public float RequestedHeight { get; }
    public float RequestedWidth { get; }

    public float CenterLatitude { get; }
    public float CenterLongitude { get; }
}
