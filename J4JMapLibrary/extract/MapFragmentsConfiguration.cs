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

using System.Numerics;
using J4JSoftware.VisualUtilities;

namespace J4JSoftware.J4JMapLibrary;

internal record MapFragmentsConfiguration(
    float Latitude,
    float Longitude,
    float Heading,
    int Scale,
    float RequestedHeight,
    float RequestedWidth
)
{
    public virtual bool Equals( MapFragmentsConfiguration? other )
    {
        if( ReferenceEquals( null, other ) )
            return false;
        if( ReferenceEquals( this, other ) )
            return true;

        return Latitude.Equals( other.Latitude )
         && Longitude.Equals( other.Longitude )
         && Heading.Equals( other.Heading )
         && Scale == other.Scale
         && RequestedHeight.Equals( other.RequestedHeight )
         && RequestedWidth.Equals( other.RequestedWidth );
    }

    public override int GetHashCode() =>
        HashCode.Combine( Latitude, Longitude, Heading, Scale, RequestedHeight, RequestedWidth );

    public bool IsValid( IProjection projection ) =>
        RequestedHeight > 0 && RequestedWidth > 0 && Scale >= projection.MinScale && Scale <= projection.MaxScale;

    public float Rotation => 360 - Heading;

    public Rectangle2D GetBoundingBox( IProjection projection )
    {
        var centerPoint = new StaticPoint( projection ) { Scale = Scale };
        centerPoint.SetLatLong( Latitude, Longitude );

        var retVal = new Rectangle2D( RequestedHeight,
                                      RequestedWidth,
                                      Rotation,
                                      new Vector3( centerPoint.X, centerPoint.Y, 0 ) );

        return projection is ITiledProjection ? retVal : retVal.BoundingBox;
    }
}
