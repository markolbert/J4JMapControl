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
using System.Runtime.CompilerServices;
using J4JSoftware.Logging;
#pragma warning disable CS8618

namespace J4JSoftware.J4JMapLibrary;

public abstract class Projection<TAuth, TViewport, TFrag> : IProjection<TFrag>
    where TAuth : class, new()
    where TFrag : class, IMapFragment
    where TViewport : INormalizedViewport
{
    public const int DefaultMaxRequestLatency = 500;

    public event EventHandler? ScaleChanged;

    private int _scale = -1; // ensure the change handler gets called on first assignment (which might be 0)
    private int _minScale;
    private int _maxScale;
    private MinMax<int>? _scaleRange;
    private float _minLat = -MapConstants.Wgs84MaxLatitude;
    private float _maxLat = MapConstants.Wgs84MaxLatitude;
    private float _minLong = -180;
    private float _maxLong = 180;
    private MinMax<int>? _xRange;
    private MinMax<int>? _yRange;

    protected Projection(
        IJ4JLogger logger
    )
    {
        Logger = logger;
        Logger.SetLoggedType( GetType() );

        var attribute = GetType().GetCustomAttribute<ProjectionAttribute>();
        if( attribute == null )
            Logger.Error( "Map projection class is not decorated with ProjectionAttribute(s), cannot be used" );
        else Name = attribute.ProjectionName;

        Initialized = !string.IsNullOrEmpty( Name );

        LatitudeRange = new MinMax<float>(-90, 90);
        LongitudeRange = new MinMax<float>(-180, 180);
    }

    protected IJ4JLogger Logger { get; }

    public string Name { get; } = string.Empty;
    public bool Initialized { get; protected set; }

    #region Scale

    public int Scale
    {
        get => _scale;

        set
        {
            var temp = ScaleRange.ConformValueToRange(value, "Scale");
            if (temp == _scale)
                return;

            _scale = temp;
            OnScaleChanged();
        }
    }

    private void OnScaleChanged()
    {
        _xRange = null;
        _yRange = null;
        HeightWidth = TileHeightWidth * InternalExtensions.Pow(2, Scale);

        ScaleChanged?.Invoke(this, EventArgs.Empty);
    }

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
            if (_scaleRange == null)
                _scaleRange = new MinMax<int>(MinScale, MaxScale);

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
            LatitudeRange = new MinMax<float>(MinLatitude, MaxLatitude);
        }
    }

    public float MinLatitude
    {
        get => _minLat;

        protected set
        {
            _minLat = value;
            LatitudeRange = new MinMax<float>(MinLatitude, MaxLatitude);
        }
    }

    public MinMax<float> LatitudeRange { get; private set; }

    public float MaxLongitude
    {
        get => _maxLong;

        protected set
        {
            _maxLong = value;
            LongitudeRange = new MinMax<float>(MinLongitude, MaxLongitude);
        }
    }

    public float MinLongitude
    {
        get => _minLong;

        protected set
        {
            _minLong = value;
            LongitudeRange = new MinMax<float>(MinLongitude, MaxLongitude);
        }
    }

    public MinMax<float> LongitudeRange { get; private set; }

    #endregion

    #region Cartesian X,Y

    public MinMax<int> XRange
    {
        get
        {
            if (_xRange != null)
                return _xRange;

            var pow2 = InternalExtensions.Pow(2, Scale);
            _xRange = new MinMax<int>(0, TileHeightWidth * pow2 - 1);

            return _xRange;
        }
    }

    public MinMax<int> YRange
    {
        get
        {
            if (_yRange != null)
                return _yRange;

            var pow2 = InternalExtensions.Pow(2, Scale);
            _yRange = new MinMax<int>(0, TileHeightWidth * pow2 - 1);

            return _yRange;
        }
    }

    #endregion

    public int TileHeightWidth { get; protected set; }
    public int HeightWidth { get; protected set; }
    public string ImageFileExtension { get; protected set; } = string.Empty;

    public int MaxRequestLatency { get; set; } = DefaultMaxRequestLatency;

    public string Copyright { get; protected set; } = string.Empty;
    public Uri? CopyrightUri { get; protected set; }

    public bool Authenticate( TAuth credentials ) =>
        Task.Run( async () => await AuthenticateAsync( credentials ) ).Result;

    public abstract Task<bool> AuthenticateAsync( TAuth credentials, CancellationToken ctx = default );

    public abstract IAsyncEnumerable<TFrag> GetExtractAsync(
        TViewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );

    async Task<bool> IProjection.AuthenticateAsync( object credentials, CancellationToken ctx )
    {
        switch( credentials )
        {
            case TAuth castCredentials:
                return await AuthenticateAsync( castCredentials, ctx );

            default:
                Logger.Error( "Expected a {0} but received a {1}", typeof( TAuth ), credentials.GetType() );
                return false;
        }
    }

    bool IProjection.Authenticate(object credentials)
    {
        switch (credentials)
        {
            case TAuth castCredentials:
                return Authenticate(castCredentials);

            default:
                Logger.Error("Expected a {0} but received a {1}", typeof(TAuth), credentials.GetType());
                return false;
        }
    }

    public abstract HttpRequestMessage? CreateMessage(TFrag fragment, int scale);

    HttpRequestMessage? IProjection.CreateMessage(object requestInfo, int scale)
    {
        if (requestInfo is TFrag castInfo)
            return CreateMessage(castInfo, scale);

        Logger.Error("Expected a {0} but was passed a {1}", typeof(TFrag), requestInfo.GetType());
        return null;
    }

    async IAsyncEnumerable<IMapFragment> IProjection.GetViewportAsync(
        INormalizedViewport viewportData,
        bool deferImageLoad,
        [ EnumeratorCancellation ] CancellationToken ctx
    )
    {
        if( viewportData.GetType().IsAssignableTo( typeof( TViewport ) ) )
        {
            await foreach( var fragment in GetExtractAsync( (TViewport) viewportData, deferImageLoad, ctx ) )
            {
                yield return fragment;
            }
        }
        else
            Logger.Error( "Expected viewport data to be an {0}, got a {1} instead",
                          typeof( TViewport ),
                          viewportData.GetType() );
    }

    public static bool operator==( Projection<TAuth, TViewport, TFrag>? left, Projection<TAuth, TViewport, TFrag>? right ) => Equals( left, right );

    public static bool operator!=( Projection<TAuth, TViewport, TFrag>? left, Projection<TAuth, TViewport, TFrag>? right ) => !Equals( left, right );

    public bool Equals( IProjection? other ) =>
        other != null
     && Scale == other.Scale
     && MinScale == other.MinScale
     && MaxScale == other.MaxScale
     && Math.Abs( MinLatitude - other.MinLatitude ) < MapConstants.FloatTolerance
     && Math.Abs( MaxLatitude - other.MaxLatitude ) < MapConstants.FloatTolerance
     && Math.Abs(MinLongitude - other.MinLongitude) < MapConstants.FloatTolerance
     && Math.Abs(MaxLongitude - other.MaxLongitude) < MapConstants.FloatTolerance
     && Equals( _xRange, other.XRange )
     && Equals( _yRange, other.YRange )
     && Name.Equals( other.Name, StringComparison.OrdinalIgnoreCase );

    public override bool Equals( object? obj )
    {
        if( ReferenceEquals( null, obj ) )
            return false;
        if( ReferenceEquals( this, obj ) )
            return true;
        if( obj.GetType() != this.GetType() )
            return false;

        return Equals( (IProjection) obj );
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add( Scale );
        hashCode.Add( MinScale );
        hashCode.Add( MaxScale );
        hashCode.Add( MinLatitude );
        hashCode.Add( MaxLatitude );
        hashCode.Add( MinLongitude );
        hashCode.Add( MaxLongitude );
        hashCode.Add( XRange );
        hashCode.Add( YRange );
        hashCode.Add( Name );
        return hashCode.ToHashCode();
    }
}
