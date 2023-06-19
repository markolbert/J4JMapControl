using System.Collections.ObjectModel;

namespace J4JSoftware.J4JMapLibrary;

public abstract partial class Projection
{
    public bool SupportsStyles { get; }

    public string? MapStyle
    {
        get => _mapStyle;

        set
        {
            bool changed;

            if (value != null && IsStyleSupported(value))
                changed = !value.Equals(_mapStyle, StringComparison.OrdinalIgnoreCase);
            else changed = _mapStyle != null;

            _mapStyle = value;

            if (changed)
                OnMapStyleChanged();
        }
    }

    public ReadOnlyCollection<string> MapStyles => _mapStyles.AsReadOnly();

    public bool IsStyleSupported( string style ) =>
        _mapStyles.Any( x => x.Equals( style, StringComparison.OrdinalIgnoreCase ) );

    protected virtual void OnMapStyleChanged()
    {
    }
}
