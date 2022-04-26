using J4JSoftware.DeusEx;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.J4JMapControl;

public sealed class J4JMapControl : Panel
{
    public J4JMapControl()
    {
        MapTiles = J4JDeusEx.ServiceProvider.GetRequiredService<ITileCollection>();
    }

    public ITileCollection MapTiles { get; }
}