using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using J4JSoftware.DeusEx;
using Microsoft.Extensions.DependencyInjection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Test.MapLibraryWinApp;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
// ReSharper disable once RedundantExtendsListEntry
public sealed partial class MainWindow : Window, INotifyPropertyChanged
{
    public MainWindow()
    {
        this.InitializeComponent();

        ViewModel = J4JDeusEx.ServiceProvider.GetRequiredService<ViewModel>();
    }

    private ViewModel ViewModel { get; }
    private Size MapSize { get; set; }

    private void TheMap_OnSizeChanged( object sender, SizeChangedEventArgs e )
    {
        MapSize = e.NewSize;
        OnPropertyChanged(nameof(MapSize));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged( [ CallerMemberName ] string? propertyName = null )
    {
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }
}