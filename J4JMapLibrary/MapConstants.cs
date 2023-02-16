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

public static class MapConstants
{
    public const float TwoPi = (float) Math.PI * 2;
    public const float HalfPi = (float) Math.PI / 2;
    public const float QuarterPi = (float) Math.PI / 4;
    public const float RadiansPerDegree = (float) Math.PI / 180;
    public const float DegreesPerRadian = (float) ( 180 / Math.PI );
    public const float EarthCircumferenceMeters = 6378137;
    public const float MetersPerInch = 0.0254F;
    public static MinMax<float> ZeroFloat = new( 0, 0 );
    public static MinMax<int> ZeroInt = new( 0, 0 );
    public static MinMax<float> AllFloat = new( float.MinValue, float.MaxValue );
    public static MinMax<int> AllInt = new( int.MinValue, int.MaxValue );
    public static MinMax<int> ViewportInt = new( 0, int.MaxValue );
    public static float Wgs84MaxLatitude = Convert.ToSingle( Math.Atan( Math.Sinh( Math.PI ) ) * 180 / Math.PI );
    public static MinMax<float> LongitudeRange = new( -180, 180 );
    public static MinMax<float> Wgs84LatitudeRange = new( -Wgs84MaxLatitude, Wgs84MaxLatitude );
}
