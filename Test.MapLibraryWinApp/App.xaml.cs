using Microsoft.UI.Xaml;
using J4JSoftware.DeusEx;
using Microsoft.UI;
using Microsoft.UI.Windowing;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Test.MapLibraryWinApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _mainWindow;
        private AppWindow? _appWindow;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            var deusEx = new DeusEx();
            if( !deusEx.Initialize() )
                throw new J4JDeusExException( "Failed to initialize J4JDeusEx, aborting" );
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched( Microsoft.UI.Xaml.LaunchActivatedEventArgs args )
        {
            _mainWindow = new MainWindow();
            _mainWindow.Activate();

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle( _mainWindow );
            var windowId = Win32Interop.GetWindowIdFromWindow( hWnd );
            _appWindow = AppWindow.GetFromWindowId( windowId );

            SetWindowSize( 700, 700 );
        }

        private void SetWindowSize(int width, int height)
        {
            var size = new Windows.Graphics.SizeInt32(width, height);
            _appWindow!.Resize(size);
        }

    }
}
