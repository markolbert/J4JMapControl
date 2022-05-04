using Microsoft.UI.Xaml;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapControl;
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
        public MainWindow()
        {
            this.InitializeComponent();

            ViewModel = J4JDeusEx.ServiceProvider.GetRequiredService<ViewModel>();
        }

        private ViewModel ViewModel { get; }

        private async void ButtonBase_OnClick( object sender, RoutedEventArgs e )
        {
            await TheMap.UpdateMap();
        }
    }
}
