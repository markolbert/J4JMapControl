using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace J4JSoftware.J4JMapLibrary;

public class Credentials : ICredentials
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _cancelOnFailure;

    protected Credentials(
        Type projectionType
    )
    {
        ProjectionType = projectionType;

        var attribute = projectionType.GetCustomAttribute<ProjectionAttribute>();
        ProjectionName = attribute?.ProjectionName ?? string.Empty;
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
}
