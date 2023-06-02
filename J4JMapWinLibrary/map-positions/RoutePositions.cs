using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace J4JSoftware.J4JMapWinLibrary;

public class RoutePositions : MapPositions<MapRoute>
{
    private readonly Func<MapRoute, DataTemplate?> _templateFunc;

    public RoutePositions(
        string routeName,
        MapRoute route,
        Expression<Func<MapRoute, DataTemplate?>> templateBinder,
        int updateInterval = J4JMapControl.DefaultUpdateEventInterval
    )
        : base( route,
                routeName,
                updateInterval )
    {
        _templateFunc = templateBinder.Compile();

        DataSourceValidator.AddRule( nameof( MapRoute.LatLongField ),
                                     x => x.LatLongField,
                                     typeof( string ) );

        DataSourceValidator.AddRule( nameof( MapRoute.LatitudeField ),
                                     x => x.LatitudeField,
                                     typeof( string ) );

        DataSourceValidator.AddRule( nameof( MapRoute.LongitudeField ),
                                     x => x.LongitudeField,
                                     typeof( string ) );
    }

    protected override bool ItemIsValid( ValidationItem validationItem )
    {
        if( validationItem.ValidationResults[ nameof( MapRoute.LatitudeField ) ] == ValidationResult.Validated
        && validationItem.ValidationResults[ nameof( MapRoute.LongitudeField ) ] == ValidationResult.Validated )
            return true;

        return validationItem.ValidationResults[ nameof( MapRoute.LatLongField ) ]
         == ValidationResult.Validated;
    }

    protected override void CompleteInitialization()
    {
        base.CompleteInitialization();

        if( string.IsNullOrEmpty( PositionVisibilityProperty ) || !ProcessedItems.Any() )
            return;

        BindingSource.PointVisibilityPropertyInfo = ProcessedItems.FirstOrDefault()
                                                                 ?.DataItem?.GetType()
                                                                  .GetProperty( PositionVisibilityProperty );
    }

    internal override IPlacedItemInternal CreatePlacedItem( ValidationItem validationItem )
    {
        var isVisible = BindingSource.ShowPoints;

        if( BindingSource.PointVisibilityPropertyInfo != null )
        {
            var visibilityValue = BindingSource.PointVisibilityPropertyInfo.GetValue( validationItem.DataItem );
            isVisible &= visibilityValue == null || (bool) visibilityValue;
        }

        var template = _templateFunc( BindingSource );

        if( template == null )
        {
            return new PlacedElement( new Ellipse
            {
                Width = BindingSource.StrokeWidth,
                Height = BindingSource.StrokeWidth,
                Fill = new SolidColorBrush( BindingSource.StrokeColor ),
                Opacity = BindingSource.StrokeOpacity,
                Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed
            } );
        }

        var retVal = new PlacedTemplatedElement( template );

        if( retVal.VisualElement == null )
            return retVal;

        retVal.VisualElement.Opacity = BindingSource.StrokeOpacity;
        retVal.VisualElement.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;

        return retVal;
    }
}
