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

namespace J4JMapLibrary;

public class StaticFragment : MapFragment, IStaticFragment
{
    public StaticFragment(
        IProjection projection,
        float latitude,
        float longitude,
        float height,
        float width,
        int scale
    )
        : base( projection )
    {
        Center = new LatLong( projection.MapServer );
        Center.SetLatLong( latitude, longitude );

        Height = height;
        Width = width;
        Scale = scale;
    }

    public override string FragmentId => $"{Center.ToText()}-{Scale}-{Height}-{Width}";

    public LatLong Center { get; }
    public float Height { get; }
    public float Width { get; }
    public int Scale { get; }
}
