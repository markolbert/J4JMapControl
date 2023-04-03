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

using System.Reflection;
using J4JSoftware.J4JMapLibrary.MapRegion;
using Serilog;

#pragma warning disable CS8618

namespace J4JSoftware.J4JMapLibrary;

public abstract class Projection<TAuth> : IProjection
    where TAuth : class, new()
{
    public const int DefaultMaxRequestLatency = 500;

    public event EventHandler<bool>? LoadComplete;

    private int _minScale;
    private int _maxScale;
    private MinMax<int>? _scaleRange;
    private float _minLat = -MapConstants.Wgs84MaxLatitude;
    private float _maxLat = MapConstants.Wgs84MaxLatitude;
    private float _minLong = -180;
    private float _maxLong = 180;
    private string _mapStyle = string.Empty;

    protected Projection(
        ILogger logger
    )
    {
        Logger = logger;
        Logger.ForContext( GetType() );

        var attribute = GetType().GetCustomAttribute<ProjectionAttribute>();
        if( attribute == null )
            Logger.Error( "Map projection class is not decorated with ProjectionAttribute(s), cannot be used" );
        else Name = attribute.ProjectionName;

        Initialized = !string.IsNullOrEmpty( Name );

        LatitudeRange = new MinMax<float>( -90, 90 );
        LongitudeRange = new MinMax<float>( -180, 180 );
    }

    protected ILogger Logger { get; }

    public string Name { get; } = string.Empty;
    public bool Initialized { get; protected set; }

    #region Scale

    public int MinScale
    {
        get => _minScale;

        protected set
        {
            _minScale = value;
            _scaleRange = null;
        }
    }

    public int MaxScale
    {
        get => _maxScale;

        protected set
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

    #endregion

    #region LatLong

    public float MaxLatitude
    {
        get => _maxLat;

        protected set
        {
            _maxLat = value;
            LatitudeRange = new MinMax<float>( MinLatitude, MaxLatitude );
        }
    }

    public float MinLatitude
    {
        get => _minLat;

        protected set
        {
            _minLat = value;
            LatitudeRange = new MinMax<float>( MinLatitude, MaxLatitude );
        }
    }

    public MinMax<float> LatitudeRange { get; private set; }

    public float MaxLongitude
    {
        get => _maxLong;

        protected set
        {
            _maxLong = value;
            LongitudeRange = new MinMax<float>( MinLongitude, MaxLongitude );
        }
    }

    public float MinLongitude
    {
        get => _minLong;

        protected set
        {
            _minLong = value;
            LongitudeRange = new MinMax<float>( MinLongitude, MaxLongitude );
        }
    }

    public MinMax<float> LongitudeRange { get; private set; }

    #endregion

    public MinMax<float> GetXYRange( int scale )
    {
        scale = ScaleRange.ConformValueToRange( scale, "Scale" );

        var pow2 = InternalExtensions.Pow( 2, scale );
        return new MinMax<float>( 0, TileHeightWidth * pow2 - 1 );
    }

    public MinMax<int> GetTileRange( int scale )
    {
        scale = ScaleRange.ConformValueToRange( scale, "GetTileRange() Scale" );
        var pow = InternalExtensions.Pow( 2, scale );

        return new MinMax<int>( 0, pow - 1 );
    }

    public int GetNumTiles( int scale )
    {
        scale = ScaleRange.ConformValueToRange( scale, "GetNumTiles() Scale" );
        return InternalExtensions.Pow( 2, scale );
    }

    public int TileHeightWidth { get; protected set; }

    public int GetHeightWidth( int scale )
    {
        scale = ScaleRange.ConformValueToRange( scale, "Scale" );
        var pow2 = InternalExtensions.Pow( 2, scale );

        return TileHeightWidth * pow2;
    }

    public string ImageFileExtension { get; protected set; } = string.Empty;

    public int MaxRequestLatency { get; set; } = DefaultMaxRequestLatency;

    public string Copyright { get; protected set; } = string.Empty;
    public Uri? CopyrightUri { get; protected set; }

    public string MapStyle
    {
        get => _mapStyle;

        set
        {
            var changed = !value.Equals( _mapStyle, StringComparison.OrdinalIgnoreCase );

            _mapStyle = value;

            if( changed )
                OnMapStyleChanged( value );
        }
    }

    protected virtual void OnMapStyleChanged( string value )
    {
    }

    protected TAuth? Credentials { get; private set; }

    public bool SetCredentials( TAuth credentials )
    {
        Credentials = credentials;
        return Authenticate();
    }

    public async Task<bool> SetCredentialsAsync( TAuth credentials, CancellationToken ctx = default )
    {
        Credentials = credentials;
        return await AuthenticateAsync( ctx );
    }

    protected bool Authenticate() => Task.Run( async () => await AuthenticateAsync() ).Result;

#pragma warning disable CS1998
    protected virtual async Task<bool> AuthenticateAsync( CancellationToken ctx = default )
#pragma warning restore CS1998
    {
        if( Credentials != null )
            return true;

        Logger.Error( "Attempting to authenticate before setting credentials" );
        return false;
    }

    public abstract HttpRequestMessage? CreateMessage( MapTile mapTile );

    public abstract Task<MapTile> GetMapTileByProjectionCoordinatesAsync(
        int x,
        int y,
        int scale,
        CancellationToken ctx = default
    );

    public abstract Task<MapTile> GetMapTileByRegionCoordinatesAsync(
        int x,
        int y,
        int scale,
        CancellationToken ctx = default
    );

    public async Task<bool> LoadRegionAsync( MapRegion.MapRegion region, CancellationToken ctx = default )
    {
        if( !Initialized )
        {
            Logger.Error( "Projection not initialized" );
            return false;
        }

        // only reload if we have to
        var retVal = region.RegionChange != MapRegionChange.LoadRequired
         || await LoadRegionInternalAsync( region, ctx );

        LoadComplete?.Invoke( this, retVal );

        return retVal;
    }

    protected abstract Task<bool> LoadRegionInternalAsync(
        MapRegion.MapRegion region,
        CancellationToken ctx = default
    );

    bool IProjection.SetCredentials( object credentials )
    {
        if( credentials is TAuth castCredentials )
            return SetCredentials( castCredentials );

        Logger.Error( "Expected a {0} but received a {1}", typeof( TAuth ), credentials.GetType() );
        return false;
    }

    async Task<bool> IProjection.SetCredentialsAsync( object credentials, CancellationToken ctx )
    {
        if( credentials is TAuth castCredentials )
        {
            await SetCredentialsAsync( castCredentials, ctx );
            return true;
        }

        Logger.Error( "Expected a {0} but received a {1}", typeof( TAuth ), credentials.GetType() );
        return false;
    }

    #region IEquatable

    public static bool operator==( Projection<TAuth>? left, Projection<TAuth>? right ) => Equals( left, right );

    public static bool operator!=( Projection<TAuth>? left, Projection<TAuth>? right ) => !Equals( left, right );

    public bool Equals( IProjection? other ) =>
        other != null
     && MinScale == other.MinScale
     && MaxScale == other.MaxScale
     && Math.Abs( MinLatitude - other.MinLatitude ) < MapConstants.FloatTolerance
     && Math.Abs( MaxLatitude - other.MaxLatitude ) < MapConstants.FloatTolerance
     && Math.Abs( MinLongitude - other.MinLongitude ) < MapConstants.FloatTolerance
     && Math.Abs( MaxLongitude - other.MaxLongitude ) < MapConstants.FloatTolerance
     && Name.Equals( other.Name, StringComparison.OrdinalIgnoreCase );

    public override bool Equals( object? obj )
    {
        if( ReferenceEquals( null, obj ) )
            return false;
        if( ReferenceEquals( this, obj ) )
            return true;
        if( obj.GetType() != GetType() )
            return false;

        return Equals( (IProjection) obj );
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add( MinScale );
        hashCode.Add( MaxScale );
        hashCode.Add( MinLatitude );
        hashCode.Add( MaxLatitude );
        hashCode.Add( MinLongitude );
        hashCode.Add( MaxLongitude );
        hashCode.Add( Name );
        return hashCode.ToHashCode();
    }

    #endregion
}
