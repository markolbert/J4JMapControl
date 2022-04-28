using Microsoft.UI.Xaml;
using J4JSoftware.DeusEx;
using Microsoft.Extensions.DependencyInjection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Test.MapLibraryWinApp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window//, INotifyPropertyChanged
    {
        //private readonly IMapImageRetriever _retriever;
        //private readonly DispatcherQueue _dQueue;

        public MainWindow()
        {
            this.InitializeComponent();
            //_dQueue = DispatcherQueue.GetForCurrentThread();

            ViewModel = J4JDeusEx.ServiceProvider.GetRequiredService<ViewModel>();
            //var appInfo = new ApplicationInfo( "J4JMapControl-Test" );
            //_retriever = new OpenStreetMapsImageRetriever( appInfo, null );
        }

        private ViewModel ViewModel { get; }

        //private ImageSource? ImageSource { get; set; }
        //private string? ErrorMessage { get; set; }
        //private int XTile { get; set; }
        //private int YTile { get; set; }
        //private int Zoom { get; set; }
        //private int ImageWidth { get; set; }
        //private int ImageHeight { get; set; }

        //private void LoadMapButton_OnClick( object sender, RoutedEventArgs e )
        //{
        //    var tile = new MultiTileCoordinates( IntPoint.Empty,
        //                                         new IntPoint( XTile, YTile ),
        //                                         LatLong.Empty,
        //                                         new Zoom( Zoom ) );

        //    _retriever.GetImageSourceAsync( tile )
        //              .ContinueWith( ( t ) => _dQueue.TryEnqueue( () =>
        //               {
        //                   ImageSource = t.Result.ImageSource;
        //                   OnPropertyChanged( nameof( ImageSource ) );

        //                   ErrorMessage = t.Result.Message;
        //                   OnPropertyChanged( nameof( ErrorMessage ) );

        //                   if( t.Result.ImageSource is not BitmapSource bitmapSrc )
        //                       return;

        //                   ImageHeight = bitmapSrc.PixelHeight;
        //                   OnPropertyChanged( nameof( ImageHeight ) );

        //                   ImageWidth = bitmapSrc.PixelWidth;
        //                   OnPropertyChanged( nameof( ImageWidth ) );
        //               } ) );
        //}

        //public event PropertyChangedEventHandler? PropertyChanged;

        //private void OnPropertyChanged( [ CallerMemberName ] string? propertyName = null )
        //{
        //    PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        //}
    }
}
