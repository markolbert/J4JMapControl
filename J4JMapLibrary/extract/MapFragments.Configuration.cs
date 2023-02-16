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

public partial class MapFragments<TFrag>
{
    private record Configuration(
        float Latitude,
        float Longitude,
        float Heading,
        int Scale,
        float Height,
        float Width,
        Buffer Buffer
    )
    {
        public virtual bool Equals( Configuration? other )
        {
            if( ReferenceEquals( null, other ) )
                return false;
            if( ReferenceEquals( this, other ) )
                return true;

            return Latitude.Equals( other.Latitude )
             && Longitude.Equals( other.Longitude )
             && Heading.Equals( other.Heading )
             && Scale == other.Scale
             && Height.Equals( other.Height )
             && Width.Equals( other.Width )
             && Buffer.Equals( other.Buffer );
        }

        public override int GetHashCode() =>
            HashCode.Combine( Latitude, Longitude, Heading, Scale, Height, Width, Buffer );

        public bool IsValid( IProjection projection ) =>
            Height > 0 && Width > 0 && Scale >= projection.MapServer.MinScale && Scale <= projection.MapServer.MaxScale;

        public float Rotation => 360 - Heading;
    }
}
