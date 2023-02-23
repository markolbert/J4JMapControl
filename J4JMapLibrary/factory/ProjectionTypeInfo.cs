using System.Reflection;

namespace J4JSoftware.J4JMapLibrary;

internal class ProjectionTypeInfo
{
    public ProjectionTypeInfo(
        Type projType
        )
    {
        var attr = projType.GetCustomAttribute<ProjectionAttribute>( false )
         ?? throw new NullReferenceException( $"{projType} is not decorated with a {typeof( ProjectionAttribute )}" );
        
        Name = attr.ProjectionName;
        ProjectionType = projType;
    }

    public string Name { get; }
    public Type ProjectionType { get; }
    public List<SupportingTypeInfo> SupportingTypes { get; } = new();
}
