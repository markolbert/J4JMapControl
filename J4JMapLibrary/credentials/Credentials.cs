using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection;

namespace J4JSoftware.J4JMapLibrary;

public abstract class Credentials : ICredentials
{
    public record CredentialProperty( string PropertyName, Type PropertyType, object? Value );

    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly List<PropertyInfo> _credProps = new();

    protected Credentials(
        Type projectionType
    )
    {
        ProjectionType = projectionType;

        var attribute = projectionType.GetCustomAttribute<ProjectionAttribute>();
        ProjectionName = attribute?.ProjectionName ?? string.Empty;

        foreach( var propInfo in this.GetType().GetProperties() )
        {
            var attr = propInfo.GetCustomAttribute<CredentialPropertyAttribute>();
            if( attr == null )
                continue;

            _credProps.Add( propInfo );
        }
    }

    [JsonIgnore]
    public Type ProjectionType { get; }

    [JsonIgnore]
    public string ProjectionName { get; }

    [JsonIgnore]
    public IEnumerable<CredentialProperty> CredentialProperties
    {
        get
        {
            foreach (var propInfo in _credProps)
            {
                yield return new CredentialProperty(propInfo.Name, propInfo.PropertyType, propInfo.GetValue(this));
            }
        }
    }

    public abstract ICredentials Encrypt( IDataProtector protector );
    public abstract ICredentials Decrypt( IDataProtector protector );

    protected virtual void OnPropertyChanged( [ CallerMemberName ] string? propertyName = null )
    {
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    protected bool SetField<T>( ref T field, T value, [ CallerMemberName ] string? propertyName = null )
    {
        if( EqualityComparer<T>.Default.Equals( field, value ) )
            return false;

        field = value;
        OnPropertyChanged( propertyName );
        return true;
    }
}
