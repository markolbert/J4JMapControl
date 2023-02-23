using System.Reflection;

namespace J4JSoftware.J4JMapLibrary;

internal class SupportingTypeInfo
{
    protected SupportingTypeInfo(
        Type supportingType,
        SupportingEntity entity
    )
    {
        var attr = supportingType.GetCustomAttribute<MapServerAttribute>( false )
         ?? throw new NullReferenceException(
                $"{supportingType} is not decorated with a {typeof( MapServerAttribute )}" );

        Name = attr.ServerName;
        ProjectionType = attr.ProjectionType;
        SupportingType = supportingType;
        Entity = entity;
    }

    public string Name { get; }
    public Type SupportingType { get; }
    public Type ProjectionType { get; }
    public SupportingEntity Entity { get; }
}
