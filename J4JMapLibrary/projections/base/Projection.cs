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
using J4JSoftware.J4JMapLibrary.MapRegion;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

public abstract class Projection<TAuth> : IProjection
    where TAuth : class, new()
{
    public const int DefaultMaxRequestLatency = 500;
    
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

    protected TAuth? Credentials { get; private set; }

    public event EventHandler<bool>? LoadComplete;

    public string Name { get; } = string.Empty;
    public bool Initialized { get; protected set; }

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
        scale = ScaleRange.ConformValueToRange( scale, "GetNumTiles()" );
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

    public bool SupportsStyles { get; }
    public ReadOnlyCollection<string> MapStyles => _mapStyles.AsReadOnly();

    public string? MapStyle
    {
        get => _mapStyle;

        set
        {
            bool changed;

            if( value != null && IsStyleSupported( value ) )
                changed = !value.Equals( _mapStyle, StringComparison.OrdinalIgnoreCase );
            else changed = _mapStyle != null;

            _mapStyle = value;

            if( changed )
                OnMapStyleChanged();
        }
    }

    public bool IsStyleSupported( string style ) =>
        _mapStyles.Any( x => x.Equals( style, StringComparison.OrdinalIgnoreCase ) );

    protected virtual void OnMapStyleChanged()
    {
    }

    protected abstract HttpRequestMessage? CreateMessage( MapTile mapTile );

    public abstract Task<MapTile> GetMapTileWraparoundAsync(
        int x,
        int y,
        int scale,
        CancellationToken ctx = default
    );

    public abstract Task<MapTile> GetMapTileAbsoluteAsync(
        int x,
        int y,
        int scale,
        CancellationToken ctx = default
    );

    bool IProjection.SetCredentials( object credentials )
    {
        if( credentials is TAuth castCredentials )
            return SetCredentials( castCredentials );

        Logger?.LogError( "Expected a {credType} but received a {actualType}", typeof( TAuth ), credentials.GetType() );
        return false;
    }

    async Task<bool> IProjection.SetCredentialsAsync( object credentials, CancellationToken ctx )
    {
        if( credentials is TAuth castCredentials )
        {
            await SetCredentialsAsync( castCredentials, ctx );
            return true;
        }

        Logger?.LogError( "Expected a {credType} but received a {actualType}", typeof( TAuth ), credentials.GetType() );
        return false;
    }

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

        Logger?.LogError( "Attempting to authenticate before setting credentials" );
        return false;
    }

    public async Task<bool> LoadRegionAsync( MapRegion.MapRegion region, CancellationToken ctx = default )
    {
        if( !Initialized )
        {
            Logger?.LogError( "Projection not initialized" );
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

    //public byte[]? GetImage( MapTile mapTile )
    //{
    //    return Task.Run( async () => await GetImageAsync( mapTile ) ).Result;
    //}

    public async Task<byte[]?> GetImageAsync( MapTile mapTile, CancellationToken ctx = default )
    {
        if( !mapTile.InProjection )
            return null;

        Logger?.LogTrace( "Beginning image retrieval from web" );

        var request = CreateMessage( mapTile );
        if( request == null )
        {
            Logger?.LogError( "Could not create HttpRequestMessage for mapTile ({fragmentId})", mapTile.FragmentId );
            return null;
        }

        var uriText = request.RequestUri!.AbsoluteUri;
        var httpClient = new HttpClient();

        Logger?.LogTrace( "Querying {uriText}", uriText );

        HttpResponseMessage? response;

        try
        {
            response = MaxRequestLatency <= 0
                ? await httpClient.SendAsync( request, ctx )
                : await httpClient.SendAsync( request, ctx )
                                  .WaitAsync( TimeSpan.FromMilliseconds( MaxRequestLatency ),
                                              ctx );

            Logger?.LogTrace( "Got response from {uriText}", uriText );
        }
        catch( Exception ex )
        {
            Logger?.LogError( "Image request from {uri} failed, message was '{errorMesg}'",
                              request.RequestUri,
                              ex.Message );
            return null;
        }

        if( response.StatusCode != HttpStatusCode.OK )
        {
            Logger?.LogError( "Image request from {uri} failed with response code {respCode}, message was '{mesg}'",
                              uriText,
                              response.StatusCode,
                              await response.Content.ReadAsStringAsync( ctx ) );

            return null;
        }

        Logger?.LogTrace( "Reading response from {uri}", uriText );

        // extract image data from response
        try
        {
            await using var responseStream = MaxRequestLatency < 0
                ? await response.Content.ReadAsStreamAsync( ctx )
                : await response.Content.ReadAsStreamAsync( ctx )
                                .WaitAsync( TimeSpan.FromMilliseconds( MaxRequestLatency ),
                                            ctx );

            var memStream = new MemoryStream();
            await responseStream.CopyToAsync( memStream, ctx );

            return memStream.ToArray();
        }
        catch( Exception ex )
        {
            Logger?.LogError( "Could not retrieve bitmap image stream from {uri}, message was '{mesg}'",
                              response.RequestMessage!.RequestUri!,
                              ex.Message );

            return null;
        }
    }

    public virtual async Task<bool> LoadImageAsync(MapTile mapTile, CancellationToken ctx = default)
    {
        mapTile.ImageData = await GetImageAsync(mapTile, ctx);
        return mapTile.ImageData != null;
    }

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
