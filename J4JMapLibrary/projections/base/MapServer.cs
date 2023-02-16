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

using System.Reflection;
using System.Text;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class MapServer<TTile, TAuth> : IMapServer<TTile, TAuth>
    where TTile : class
    where TAuth : class
{
    public const int DefaultMaxRequestLatency = 500;

    private int _minScale;
    private int _maxScale;
    private MinMax<int>? _scaleRange;
    private float _minLat = -MapConstants.Wgs84MaxLatitude;
    private float _maxLat = MapConstants.Wgs84MaxLatitude;
    private float _minLong = -180;
    private float _maxLong = 180;

    protected MapServer()
    {
        Logger = J4JDeusEx.GetLogger()!;
        Logger.SetLoggedType( GetType() );

        LatitudeRange = new MinMax<float>( -90, 90 );
        LongitudeRange = new MinMax<float>( -180, 180 );

        var attr = GetType().GetCustomAttribute<MapServerAttribute>();
        SupportedProjection = attr?.ProjectionName ?? string.Empty;

        if( !string.IsNullOrEmpty( SupportedProjection ) )
            return;

        Logger.Error( "{0} is not decorated with a {1}, will not be accessible by projections",
                      GetType(),
                      typeof( MapServerAttribute ) );
    }

    protected IJ4JLogger Logger { get; }

    public string SupportedProjection { get; }
    public abstract bool Initialized { get; }

    public int MinScale
    {
        get => _minScale;

        internal set
        {
            _minScale = value;
            _scaleRange = null;
        }
    }

    public int MaxScale
    {
        get => _maxScale;

        internal set
        {
            _maxScale = value;
            _scaleRange = null;
        }
    }

    public MinMax<int> ScaleRange
    {
        get
        {
            if( _scaleRange == null )
                _scaleRange = new MinMax<int>( MinScale, MaxScale );

            return _scaleRange;
        }
    }

    public float MaxLatitude
    {
        get => _maxLat;

        internal set
        {
            _maxLat = value;
            LatitudeRange = new MinMax<float>( MinLatitude, MaxLatitude );
        }
    }

    public float MinLatitude
    {
        get => _minLat;

        internal set
        {
            _minLat = value;
            LatitudeRange = new MinMax<float>( MinLatitude, MaxLatitude );
        }
    }

    public MinMax<float> LatitudeRange { get; private set; }

    public float MaxLongitude
    {
        get => _maxLong;

        internal set
        {
            _maxLong = value;
            LongitudeRange = new MinMax<float>( MinLongitude, MaxLongitude );
        }
    }

    public float MinLongitude
    {
        get => _minLong;

        internal set
        {
            _minLong = value;
            LongitudeRange = new MinMax<float>( MinLongitude, MaxLongitude );
        }
    }

    public MinMax<float> LongitudeRange { get; private set; }

    public int MaxRequestLatency { get; set; } = DefaultMaxRequestLatency;
    public int TileHeightWidth { get; internal set; }
    public string ImageFileExtension { get; internal set; } = string.Empty;

    public string Copyright { get; internal set; } = string.Empty;
    public Uri? CopyrightUri { get; internal set; }

    public abstract HttpRequestMessage? CreateMessage( TTile tile, int scale );

    HttpRequestMessage? IMapServer.CreateMessage( object requestInfo, int scale )
    {
        if( requestInfo is TTile castInfo )
            return CreateMessage( castInfo, scale );

        Logger.Error( "Expected a {0} but was passed a {1}", typeof( TTile ), requestInfo.GetType() );
        return null;
    }
}
