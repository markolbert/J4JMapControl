using System.Numerics;
using J4JSoftware.VisualUtilities;
// ReSharper disable NonReadonlyMemberInGetHashCode
#pragma warning disable IDE0041

namespace J4JSoftware.J4JMapLibrary;

public class Region
{
    protected bool Equals( Region other ) =>
        Height.Equals( other.Height )
     && Width.Equals( other.Width )
     && Equals( CenterPoint, other.CenterPoint )
     && Heading.Equals( other.Heading )
     && Scale == other.Scale
     && ShrinkStyle == other.ShrinkStyle
     && MapStyle == other.MapStyle;

    public override bool Equals( object? obj )
    {
        if( ReferenceEquals( null, obj ) )
            return false;
        if( ReferenceEquals( this, obj ) )
            return true;
        if( obj.GetType() != this.GetType() )
            return false;

        return Equals( (Region) obj );
    }

    public override int GetHashCode() =>
        HashCode.Combine( Height, Width, CenterPoint, Heading, Scale, (int) ShrinkStyle, MapStyle );

    public static bool operator==( Region? left, Region? right ) => Equals( left, right );

    public static bool operator!=( Region? left, Region? right ) => !Equals( left, right );

    public static Region FromTileCoordinates(
        IProjection projection,
        int xTile,
        int yTile,
        int scale,
        string? mapStyle = null
    )
    {
        var center = new MapPoint( projection, scale );
        center.SetCartesian( ( xTile + 0.5f ) * projection.TileHeightWidth,
                             ( yTile + 0.5f ) * projection.TileHeightWidth );

        return new Region
        {
            Height = projection.TileHeightWidth,
            Width = projection.TileHeightWidth,
            CenterPoint = center,
            Heading = 0,
            Scale = scale,
            ShrinkStyle = ShrinkStyle.None,
            MapStyle = mapStyle
        };
    }

    public float Height { get; set; }
    public float Width { get; set; }
    public MapPoint? CenterPoint { get; set; }
    public float Heading { get; set; }
    public float Rotation => ( 360 - Heading ) % 360;
    public int Scale { get; set; }
    public ShrinkStyle ShrinkStyle { get; set; } = ShrinkStyle.None;
    public string? MapStyle { get; set; }

    public Rectangle2D? Area =>
        CenterPoint == null
            ? null
            : new( Height,
                   Width,
                   Rotation,
                   new Vector3( CenterPoint.X,
                                CenterPoint.Y,
                                0 ),
                   CoordinateSystem2D.Display );
}