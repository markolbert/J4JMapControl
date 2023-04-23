using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace J4JSoftware.J4JMapWinLibrary;

public class RouteInfo : DependencyObject
{
    public static readonly DependencyProperty LineProperty =
        DependencyProperty.Register( nameof( Line ),
                                     typeof( Line ),
                                     typeof( J4JMapControl ),
                                     new PropertyMetadata( new Line
                                     {
                                         Stroke = new SolidColorBrush( Colors.Black ),
                                         Width = 2
                                     } ) );

    public Line? Line
    {
        get => (Line?) GetValue( LineProperty );
        set => SetValue( LineProperty, value );
    }

    public static readonly DependencyProperty MarkerProperty =
        DependencyProperty.Register(nameof(Marker),
                                    typeof(Shape),
                                    typeof(J4JMapControl),
                                    new PropertyMetadata(new Ellipse
                                    {
                                        Stroke = new SolidColorBrush(Colors.Black),
                                        Fill = new SolidColorBrush(Colors.DarkGray),
                                        Width = 3,
                                        Height = 3
                                    }));

    public Shape? Marker
    {
        get => (Shape?)GetValue(MarkerProperty);
        set => SetValue(MarkerProperty, value);
    }

    public static readonly DependencyProperty RouteNumberProperty =
        DependencyProperty.Register( nameof( RouteNumber ),
                                     typeof( int ),
                                     typeof( J4JMapControl ),
                                     new PropertyMetadata( 0 ) );

    public int RouteNumber
    {
        get => (int) GetValue( RouteNumberProperty );
        set => SetValue( RouteNumberProperty, value );
    }
}
