using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

namespace WinAppTest;

public class PointOfInterest : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _location = "0E, 0N";
    private string _text = string.Empty;
    private SolidColorBrush _brush = new SolidColorBrush( Colors.Black );

    public string Location
    {
        get => _location;
        set => SetField( ref _location, value );
    }

    public string Text
    {
        get => _text;
        set => SetField( ref _text, value );
    }

    public SolidColorBrush Brush
    {
        get => _brush;
        set => SetField( ref _brush, value );
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
