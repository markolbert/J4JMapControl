using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public DependencyProperty MapProjectionProperty = DependencyProperty.Register( nameof( MapProjection ),
        typeof( IProjection ),
        typeof( J4JMapControl ),
        new PropertyMetadata( null, OnMapProjectionChanged ) );

    public DependencyProperty LatitudeProperty = DependencyProperty.Register( nameof( Latitude ),
                                                                              typeof( float ),
                                                                              typeof( J4JMapControl ),
                                                                              new PropertyMetadata( 0F,
                                                                                  OnViewportChanged ) );

    public DependencyProperty LongitudeProperty = DependencyProperty.Register( nameof( Longitude ),
                                                                               typeof( float ),
                                                                               typeof( J4JMapControl ),
                                                                               new PropertyMetadata( 0F,
                                                                                   OnViewportChanged ) );

    public DependencyProperty MapScaleProperty = DependencyProperty.Register( nameof( Scale ),
                                                                              typeof( int ),
                                                                              typeof( J4JMapControl ),
                                                                              new PropertyMetadata( 0F,
                                                                                  OnViewportChanged ) );

    public DependencyProperty HeadingProperty = DependencyProperty.Register( nameof( Heading ),
                                                                             typeof( float ),
                                                                             typeof( J4JMapControl ),
                                                                             new PropertyMetadata( 0F,
                                                                                 OnViewportChanged ) );
}
