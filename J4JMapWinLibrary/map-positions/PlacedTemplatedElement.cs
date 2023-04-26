using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Shapes;

namespace J4JSoftware.J4JMapWinLibrary;

public class PlacedTemplatedElement : PlacedItem, IPlacedElement
{
    private readonly DataTemplate _template;

    public PlacedTemplatedElement( 
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