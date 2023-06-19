#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// Projection.cs
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

using System.Collections.ObjectModel;
using System.Net;
using System.Reflection;
using J4JSoftware.VisualUtilities;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

public abstract partial class Projection : IProjection
{
    public const int DefaultMaxRequestLatency = 500;

    public event EventHandler<bool>? RegionProcessed;

    private readonly List<string> _mapStyles;

    private float _maxLat = MapConstants.Wgs84MaxLatitude;
    private float _maxLong = 180;
    private float _minLat = -MapConstants.Wgs84MaxLatitude;
    private float _minLong = -180;
    private int _minScale;
    private int _maxScale;
    private MinMax<int>? _scaleRange;
    private string? _mapStyle;

    protected Projection(
        IEnumerable<string>? mapStyles = null,
        ILoggerFactory? loggerFactory = null
    )
    {
        _mapStyles = mapStyles?.ToList() ?? new List<string>();
        SupportsStyles = _mapStyles.Any();

        LoggerFactory = loggerFactory;
        Logger = loggerFactory?.CreateLogger( GetType() );

        var attribute = GetType().GetCustomAttribute<ProjectionAttribute>();
        if( attribute == null )
            Logger?.LogError( "Map projection class is not decorated with ProjectionAttribute(s), cannot be used" );
        else Name = attribute.ProjectionName;

        Initialized = !string.IsNullOrEmpty( Name );

        LatitudeRange = new MinMax<float>( -90, 90 );
        LongitudeRange = new MinMax<float>( -180, 180 );
    }

    protected ILogger? Logger { get; }
    protected ILoggerFactory? LoggerFactory { get; }
}
