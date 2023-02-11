namespace J4JMapLibrary;

public class ProjectionScale : IProjectionScale
{
    private int _scale;

    public ProjectionScale(
        IMapServer mapServer
        )
    {
        MapServer = mapServer;
    }

    protected ProjectionScale(ProjectionScale toCopy)
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
            UpdateScaleRelated();
        }
    }

    protected virtual void UpdateScaleRelated()
    {
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
