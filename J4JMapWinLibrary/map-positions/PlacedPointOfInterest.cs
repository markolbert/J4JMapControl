using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public class PlacedPointOfInterest : PlacedItem
{
    private readonly DataTemplate _template;

    public PlacedPointOfInterest( 
        DataTemplate template
    )
    {
        _template = template;
    }

    public FrameworkElement? VisualElement { get; private set; }

    protected override void Initialize( object data )
    {
        base.Initialize( data );

        VisualElement = _template.LoadContent() as FrameworkElement;

        if( VisualElement != null )
            VisualElement.DataContext = data;
    }
}
