using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapControl;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using WinRT;

namespace J4JSoftware.MapLibrary
{
    public class TileImage : FrameworkElement
    {
        #region MapRetriever property

        // the IMapImageRetriever being used to display the map layer
        public static readonly DependencyProperty MapRetrieverProperty = DependencyProperty.Register(nameof(MapRetriever),
            typeof(IMapImageRetriever),
            typeof(TileImage),
            new PropertyMetadata(J4JDeusEx.ServiceProvider.GetRequiredService<OpenStreetMapsImageRetriever>(),
                                 OnMapImageRetrieverChanged));

        public IMapImageRetriever? MapRetriever
        {
            get => (IMapImageRetriever?)GetValue(MapRetrieverProperty);
            set => SetValue(MapRetrieverProperty, value);
        }

        private static async void OnMapImageRetrieverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TileImage tileImage
             || e.NewValue is not IMapImageRetriever retriever
             || retriever.MapRetrieverInfo == null
             || retriever.Zoom == null)
                return;

            await tileImage.LoadImageAsync();
        }

        #endregion

        #region Coordinates property

        // the Coordinates object defining this TileImage
        public static readonly DependencyProperty CoordinatesProperty = DependencyProperty.Register( 
            "", 
            typeof( Coordinates ), 
            typeof( TileImage ), 
            new PropertyMetadata(null,OnCoordinatesChangedStatic) );

        private static void OnCoordinatesChangedStatic( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            if( d is not TileImage tileImage )
                return;

            switch (e.NewValue)
            {
                case null:
                    tileImage.OnCoordinatesChanged( null );
                    break;

                case Coordinates coordinates:
                    tileImage.OnCoordinatesChanged(coordinates);
                    break;

                default:
                    tileImage._logger?.Error("{0} received a {1} instead of a {2}",
                                              nameof(OnCoordinatesChangedStatic),
                                              e.NewValue.GetType(),
                                              typeof(Coordinates));
                    break;
            }
        }

        public Coordinates? Coordinates
        {
            get => ( Coordinates? )GetValue( CoordinatesProperty );
            set => SetValue( CoordinatesProperty, value );
        }

        #endregion

        private readonly IJ4JLogger? _logger;

        public TileImage()
        {
            _logger = J4JDeusEx.ServiceProvider.GetService<IJ4JLogger>();
            _logger?.SetLoggedType( GetType() );
        }

        public BitmapSource? ImageSource { get; private set; }

        ///TODO: probably should generate a default image when value is null
        private async Task OnCoordinatesChanged(Coordinates? value)
        {
            if( value == null )
                return;

            if( MapRetriever != null )
                await LoadImageAsync();
        }

        private async Task LoadImageAsync()
        {
            if( MapRetriever == null
            || MapRetriever.MapRetrieverInfo == null
            || Coordinates == null )
                return;

            var temp = await MapRetriever.GetImageStreamAsync( Coordinates );
            if( temp.ReturnValue is not InMemoryRandomAccessStream memStream )
            {
                _logger?.Error( "{0} did not return a {1}",
                                nameof( MapRetriever.GetImageStreamAsync ),
                                typeof( InMemoryRandomAccessStream ) );
                return;
            }

            ImageSource = new WriteableBitmap( MapRetriever.MapRetrieverInfo.DefaultBitmapWidthHeight,
                                               MapRetriever.MapRetrieverInfo.DefaultBitmapWidthHeight );

            await ImageSource.SetSourceAsync( memStream );
        }
    }
}
