using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace J4JSoftware.J4JMapLibrary;

public class Credentials : ICredentials
{
    public record CredentialProperty( string PropertyName, Type PropertyType, object? Value );

    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly List<PropertyInfo> _credProps = new();

    private bool _cancelOnFailure;

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

    public Type ProjectionType { get; }
    public string ProjectionName { get; }

    public bool CancelOnFailure
    {
        get => _cancelOnFailure;
        set => SetField( ref _cancelOnFailure, value );
    }

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

    public IEnumerator<CredentialProperty> GetEnumerator()
    {
        foreach( var propInfo in _credProps )
        {
            yield return new CredentialProperty( propInfo.Name, propInfo.PropertyType, propInfo.GetValue( this ) );
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
