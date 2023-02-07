namespace J4JMapLibrary;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited=false)]
public class MessageCreatorAttribute : Attribute
{
    public MessageCreatorAttribute(
        string name
    )
    {
        SupportedProjection = name;
    }

    public string SupportedProjection { get; }
}