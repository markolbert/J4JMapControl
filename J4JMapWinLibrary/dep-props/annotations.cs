using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private Canvas? _annotationsCanvas;

    public static DependencyProperty AnnotationsProperty = DependencyProperty.Register( nameof( Annotations ),
        typeof( List<FrameworkElement> ),
        typeof( J4JMapControl ),
        new PropertyMetadata( new List<FrameworkElement>() ) );

    public List<FrameworkElement> Annotations
    {
        get => (List<FrameworkElement>) GetValue( AnnotationsProperty );
        set => SetValue( AnnotationsProperty, value );
    }

    private void ValidateAnnotations()
    {
        foreach( var uiElement in Annotations )
        {
            if( Location.TryParseCenter( uiElement, out _, out _ ) )
                continue;
        }
    }
}
