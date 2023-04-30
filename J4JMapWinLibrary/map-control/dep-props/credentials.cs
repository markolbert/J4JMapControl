using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private readonly DispatcherQueue _dispatcherCredDlg = DispatcherQueue.GetForCurrentThread();

    private async Task<ContentDialog?> LaunchDialog( string projName )
    {
        var credType = MapControlViewModelLocator.Instance!
                                                 .CredentialsDialogFactory[ projName ];

        if( credType == null )
        {
            _logger?.LogWarning( "No credentials dialog for {projection}",
                                 projName );
            return null;
        }

        var credDialog = Activator.CreateInstance( credType ) as ContentDialog;
        if( credDialog == null )
        {
            _logger?.LogWarning( "Could not create credentials dialog for {projection}",
                                 projName );
            return null;
        }

        credDialog.XamlRoot = XamlRoot;

        return await credDialog.ShowAsync() == ContentDialogResult.Primary ? credDialog : null;
    }
}
