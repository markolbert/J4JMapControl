using J4JSoftware.VisualUtilities;
// ReSharper disable NonReadonlyMemberInGetHashCode
#pragma warning disable IDE0041

namespace J4JSoftware.J4JMapLibrary;

public class Region
{
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
            Latitude = center.Latitude,
            Longitude = center.Longitude,
            Heading = 0,
            Scale = scale,
            ShrinkStyle = ShrinkStyle.None,
            MapStyle = mapStyle
        };
    }

    public float Height { get; set; }
    public float Width { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public float Heading { get; set; }
    public float Rotation => (360 - Heading) % 360;
    public int Scale { get; set; }
    public ShrinkStyle ShrinkStyle { get; set; } = ShrinkStyle.None;
    public string? MapStyle { get; set; }

    public virtual bool Equals(Region? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Height.Equals(other.Height)
         && Width.Equals(other.Width)
         && Latitude.Equals(other.Latitude)
         && Longitude.Equals(other.Longitude)
         && Heading.Equals(other.Heading)
         && Scale == other.Scale
         && ShrinkStyle == other.ShrinkStyle
         && MapStyle == other.MapStyle;
    }

    public override int GetHashCode() =>
        HashCode.Combine(Height, Width, Latitude, Longitude, Heading, Scale, (int)ShrinkStyle);

}