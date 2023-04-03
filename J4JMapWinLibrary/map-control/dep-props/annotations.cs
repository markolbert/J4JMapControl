using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public static DependencyProperty AnnotationsProperty = DependencyProperty.Register( nameof( Annotations ),
        typeof( List<FrameworkElement> ),
        typeof( J4JMapControl ),
        new PropertyMetadata( new List<FrameworkElement>() ) );

    private Canvas? _annotationsCanvas;

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
