#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// MapRegion.cs
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

using System.Collections;
using System.ComponentModel;
using System.Numerics;
using J4JSoftware.VisualUtilities;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary.MapRegion;

public class MapRegion : IEnumerable<PositionedMapBlock>
{
    public event EventHandler? ConfigurationChanged;
    public event EventHandler<MapRegionChange>? BuildUpdated;

    private readonly MinMax<float> _heightWidthRange = new( 0F, float.MaxValue );

    private float _latitude;
    private float _longitude;
    private float _requestedHeight;
    private float _requestedWidth;
    private int _scale;
    private int _oldScale;
    private float _heading;
    private string? _mapStyle;
    private string? _oldMapStyle;
    private float _xOffset;
    private float _yOffset;

#pragma warning disable CS8618
    public MapRegion(
#pragma warning restore CS8618
        IProjection projection,
        ILoggerFactory? loggerFactory = null
    )
    {
        LoggerFactory = loggerFactory;
        Logger = loggerFactory?.CreateLogger<MapRegion>();

        Projection = projection;
        ProjectionType = Projection.GetProjectionType();
        _mapStyle = projection.MapStyle;

        Scale = projection.MinScale;
        MaximumTiles = projection.GetNumTiles( Scale );

        UpdateEmpty();
    }

    protected ILogger? Logger { get; }
    internal ILoggerFactory? LoggerFactory { get; }

    public IProjection Projection { get; }
    public ProjectionType ProjectionType { get; }

    public bool Changed { get; private set; }

    public float CenterLatitude
    {
        get => _latitude;

        internal set =>
            SetField( ref _latitude, Projection.LatitudeRange.ConformValueToRange( value, "MapRegion Latitude" ) );
    }

    public float CenterLongitude
    {
        get => _longitude;

        internal set =>
            SetField( ref _longitude, Projection.LongitudeRange.ConformValueToRange( value, "MapRegion Longitude" ) );
    }

    public float CenterXOffset
    {
        get => _xOffset;
        internal set => SetField( ref _xOffset, value );
    }

    public float CenterYOffset
    {
        get => _yOffset;
        internal set => SetField( ref _yOffset, value );
    }

    public MapPoint Center { get; private set; }

    // The ViewpointOffset is the vector that describes how the origin
    // of the display elements -- which defaults to 0,0 -- needs to be
    // offset/translated so that the requested center point is, in fact,
    // in the center of the display elements. This doesn't happen
    // automatically for tiled projections because the images come
    // in fixed sizes, so the upper left image may well include data
    // that should be outside the display area (which is centered on the
    // requested center point)
    public Vector3 ViewpointOffset { get; private set; }

    public float RequestedHeight
    {
        get => _requestedHeight;

        internal set =>
            SetField( ref _requestedHeight,
                      _heightWidthRange.ConformValueToRange( value, "MapRegion Requested Height" ) );
    }

    public float RequestedWidth
    {
        get => _requestedWidth;

        internal set =>
            SetField( ref _requestedWidth,
                      _heightWidthRange.ConformValueToRange( value, "MapRegion Requested Width" ) );
    }

    public int Scale
    {
        get => _scale;
        internal set => SetField( ref _scale, Projection.ScaleRange.ConformValueToRange( value, "MapRegion Scale" ) );
    }

    // in degrees; north is 0/360; stored as mod 360
    public float Heading
    {
        get => _heading;
        internal set => SetField( ref _heading, value % 360 );
    }

    public float Rotation => ( 360 - Heading ) % 360;

    public string? MapStyle
    {
        get => _mapStyle;

        internal set
        {
            if( value != null && !Projection.IsStyleSupported( value ) )
                value = null;

            Projection.MapStyle = value;

            SetField( ref _mapStyle, value );
        }
    }

    public Rectangle2D BoundingBox { get; private set; }
    public int MaximumTiles { get; private set; }
    public TileCoordinates UpperLeft { get; private set; }
    public int TilesWide { get; private set; }
    public int TilesHigh { get; private set; }

    public float TileWidth =>
        ProjectionType switch
        {
            ProjectionType.Static => RequestedWidth,
            ProjectionType.Tiled => ( (ITiledProjection) Projection ).TileHeightWidth,
            _ => throw new InvalidEnumArgumentException( $"Unsupported {typeof( ProjectionType )} '{ProjectionType}'" )
        };

    public float TileHeight =>
        ProjectionType switch
        {
            ProjectionType.Static => RequestedHeight,
            ProjectionType.Tiled => ( (ITiledProjection) Projection ).TileHeightWidth,
            _ => throw new InvalidEnumArgumentException( $"Unsupported {typeof( ProjectionType )} '{ProjectionType}'" )
        };

    public List<PositionedMapBlock> MapBlocks { get; } = new();

    public MapBlock? this[ int column, int row ] =>
        MapBlocks.FirstOrDefault( b => b.Row == row && b.Column == column )?.MapBlock;

    public bool IsDefined { get; private set; }
    public MapRegionChange RegionChange { get; private set; }

    public IEnumerator<PositionedMapBlock> GetEnumerator()
    {
        if( !IsDefined )
            yield break;

        foreach( var positionedBlock in MapBlocks.OrderBy( b => b.Row ).ThenBy( b => b.Column ) )
        {
            yield return positionedBlock;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void SetField( ref int field, int newValue )
    {
        if( newValue == field )
            return;

        field = newValue;
        Changed = true;
        ConfigurationChanged?.Invoke( this, EventArgs.Empty );
    }

    private void SetField( ref float field, float newValue )
    {
        if( Math.Abs( newValue - field ) < MapConstants.FloatTolerance )
            return;

        field = newValue;
        Changed = true;
        ConfigurationChanged?.Invoke( this, EventArgs.Empty );
    }

    private void SetField( ref string? field, string? newValue )
    {
        if( field == null && newValue == null )
            return;

        if( field != null && newValue != null && field.Equals( newValue, StringComparison.OrdinalIgnoreCase ) )
            return;

        field = newValue;
        Changed = true;
        ConfigurationChanged?.Invoke( this, EventArgs.Empty );
    }

    public int WrapXOffsetWithinProjection( int offsetX )
    {
        var retVal = UpperLeft.X + offsetX;

        if( retVal < 0 )
            retVal += MaximumTiles;

        if( retVal < 0 )
            retVal = -1;

        if( retVal >= MaximumTiles )
            retVal = -1;

        return retVal;
    }

    public MapRegion Update()
    {
        RegionChange = MapRegionChange.NoChange;
        IsDefined = false;

        MaximumTiles = Projection.GetNumTiles( Scale );

        var oldBoundingBox = BoundingBox.Copy();
        var oldUpperLeft = UpperLeft;
        var oldWide = TilesWide;
        var oldHigh = TilesHigh;

        Center = new MapPoint( this );
        Center.SetLatLong( CenterLatitude, CenterLongitude );

        // apply the x/y offsets, if any, and adjust properties accordingly
        if( CenterXOffset != 0 || CenterYOffset != 0 )
        {
            var adjX = CenterXOffset;
            var adjY = CenterYOffset;

            if( Heading != 0 )
            {
                var offset = new Vector3( CenterXOffset, CenterYOffset, 0 );
                var transform = Matrix4x4.CreateRotationZ( -Rotation * MapConstants.RadiansPerDegree );
                var rotated = Vector3.Transform( offset, transform );

                adjX = rotated.X;
                adjY = rotated.Y;
            }

            var xOffset = adjX == 0 ? (float?) null : adjX;
            var yOffset = adjY == 0 ? (float?) null : adjY;
            Center.OffsetCartesian( xOffset, yOffset );

            CenterLatitude = Center.Latitude;
            CenterLongitude = Center.Longitude;

            CenterXOffset = 0;
            CenterYOffset = 0;
        }

        // if we're not fully defined just invoke and return
        if( RequestedHeight <= 0 || RequestedWidth <= 0 )
        {
            UpdateEmpty();
            RegionChange = MapRegionChange.Empty;

            BuildUpdated?.Invoke( this, MapRegionChange.Empty );
            return this;
        }

        var box = new Rectangle2D( RequestedHeight,
                                   RequestedWidth,
                                   Rotation,
                                   new Vector3( Center.X, Center.Y, 0 ) );

        BoundingBox = box.BoundingBox;

        if( Projection is ITiledProjection )
            UpdateTiledRegion( oldUpperLeft, oldWide, oldHigh );
        else UpdateStaticRegion( oldBoundingBox );

        IsDefined = true;

        Changed = false;
        _oldScale = Scale;
        _oldMapStyle = MapStyle;

        BuildUpdated?.Invoke( this, RegionChange );

        return this;
    }

    private void UpdateEmpty()
    {
        UpperLeft = new TileCoordinates(this, int.MinValue, int.MinValue );
        TilesWide = 0;
        TilesHigh = 0;
        BoundingBox = Rectangle2D.Empty;
    }

    private void UpdateStaticRegion( Rectangle2D oldBoundingBox )
    {
        UpperLeft = new TileCoordinates(this,0, 0 );
        TilesWide = 1;
        TilesHigh = 1;

        ViewpointOffset = new Vector3( -( BoundingBox.Width - RequestedWidth ) / 2,
                                       -( BoundingBox.Height - RequestedHeight ) / 2,
                                       0 );

        MapBlocks.Clear();

        var block = StaticBlock.CreateBlock( this );
        if( block == null )
            Logger?.LogError("Could not create {type}", typeof(StaticBlock));

        MapBlocks.Add( new PositionedMapBlock( 0, 0, block ) );

        RegionChange = BoundingBox.Equals( oldBoundingBox ) && !MapStyleChanged()
            ? MapRegionChange.OffsetChanged
            : MapRegionChange.LoadRequired;
    }

    private void UpdateTiledRegion( TileCoordinates oldUpperLeft, int oldWide, int oldHigh )
    {
        var minXTile = RoundTile( BoundingBox.Min( c => c.X ) );
        var maxXTile = RoundTile( BoundingBox.Max( c => c.X ) - 1 );
        var minYTile = RoundTile( BoundingBox.Min( c => c.Y ) );
        var maxYTile = RoundTile( BoundingBox.Max( c => c.Y ) - 1 );

        var rawColumns = maxXTile - minXTile + 1;
        var rawRows = maxYTile - minYTile + 1;

        var rawBlocks = new MapBlock?[rawColumns, rawRows];

        for (var xTile = minXTile; xTile <= maxXTile; xTile++)
        {
            for (var yTile = minYTile; yTile <= maxYTile; yTile++)
            {
                rawBlocks[ xTile - minXTile, yTile - minYTile ] = TileBlock.CreateBlock( this, xTile, yTile );
            }
        }

        // identify rows which have at least some tiles in the projection
        var rowsToExclude = new List<int>();

        for (var row = 0; row < rawRows; row++)
        {
            var inProjection = false;

            for (var column = 0; column < rawColumns; column++)
            {
                inProjection |= rawBlocks[column, row] != null;
            }

            if (!inProjection)
                rowsToExclude.Add(row);
        }

        TilesHigh = rawRows - rowsToExclude.Count;

        // identify columns which have at least some tiles in the projection
        var columnsToExclude = new List<int>();

        for (var column = 0; column < rawColumns; column++)
        {
            var inProjection = false;

            for (var row = 0; row < rawRows; row++)
            {
                inProjection |= rawBlocks[column, row] != null;
            }

            if (!inProjection)
                columnsToExclude.Add(column);
        }

        TilesWide = rawColumns - columnsToExclude.Count;

        MapBlocks.Clear();

        var finalRow = -1;

        for (var row = 0; row < rawRows; row++)
        {
            if (rowsToExclude.Contains(row))
                continue;

            finalRow++;
            var finalColumn = -1;

            for (var column = 0; column < rawColumns; column++)
            {
                if (columnsToExclude.Contains(column))
                    continue;

                finalColumn++;

                MapBlocks.Add( new PositionedMapBlock( finalRow, finalColumn, rawBlocks[ column, row ] ) );
            }
        }

        UpperLeft = new TileCoordinates(this, minXTile, minYTile );

        var tileHeightWidth = ( (ITiledProjection) Projection ).TileHeightWidth;

        var xOffset = Center.X - RequestedWidth / 2 - UpperLeft.X * tileHeightWidth;
        var yOffset = Center.Y - RequestedHeight / 2 - UpperLeft.Y * tileHeightWidth;

        ViewpointOffset = new Vector3( -xOffset, -yOffset, 0 );

        RegionChange = UpperLeft == oldUpperLeft
         && Scale == _oldScale
         && TilesWide == oldWide
         && TilesHigh == oldHigh
         && !MapStyleChanged()
                ? MapRegionChange.OffsetChanged
                : MapRegionChange.LoadRequired;
    }

    private bool MapStyleChanged()
    {
        if( _mapStyle == null && _oldMapStyle == null )
            return false;

        if( _mapStyle != null && _oldMapStyle != null )
            return Projection.IsStyleSupported( _mapStyle );

        return true;
    }

    private int RoundTile( float value )
    {
        var tile = Math.Round( value ) / Projection.TileHeightWidth;
        return (int) Math.Floor( tile );
    }
}
