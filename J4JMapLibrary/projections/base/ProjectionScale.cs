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

public class ProjectionScale : IProjectionScale
{
    public event EventHandler? ScaleChanged;

    private int _scale;

    public ProjectionScale(
        IMapServer mapServer
    )
    {
        MapServer = mapServer;
    }

    protected ProjectionScale( ProjectionScale toCopy )
    {
        MapServer = toCopy.MapServer;
        Scale = toCopy.Scale;
    }

    public IMapServer MapServer { get; }

    public int Scale
    {
        get => _scale;

        set
        {
            var temp = MapServer.ScaleRange.ConformValueToRange( value, "Scale" );
            if( temp == _scale )
                return;

            _scale = temp;
            OnScaleChanged();
        }
    }

    protected virtual void OnScaleChanged()
    {
        ScaleChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool Equals( ProjectionScale? other )
    {
        if( ReferenceEquals( null, other ) ) return false;
        if( ReferenceEquals( this, other ) ) return true;

        return Scale == other.Scale;
    }

    public static ProjectionScale Copy( ProjectionScale toCopy ) => new( toCopy );

    public override bool Equals( object? obj )
    {
        if( ReferenceEquals( null, obj ) ) return false;
        if( ReferenceEquals( this, obj ) ) return true;

        return obj.GetType() == GetType() && Equals( (ProjectionScale) obj );
    }

    public override int GetHashCode() => HashCode.Combine( Scale );

    public static bool operator==( ProjectionScale? left, ProjectionScale? right ) => Equals( left, right );

    public static bool operator!=( ProjectionScale? left, ProjectionScale? right ) => !Equals( left, right );
}
