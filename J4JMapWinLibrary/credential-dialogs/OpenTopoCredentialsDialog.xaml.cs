// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.J4JMapWinLibrary;

[CredentialsDialog(typeof(OpenTopoCredentials))]
public sealed partial class OpenTopoCredentialsDialog : ContentDialog, ICredentialsDialog
{
    public OpenTopoCredentialsDialog()
    {
        //if (MapControlViewModelLocator.Instance == null)
        //    throw new NullReferenceException($"{typeof(MapControlViewModelLocator)} was not initialized");

        //var temp = MapControlViewModelLocator.Instance.CredentialsFactory[ typeof( OpenTopoMapsProjection ), false ];
        //if( temp is not OpenTopoCredentials credentials )
        //    throw new NullReferenceException($"{typeof(OpenTopoCredentials)} could not be created");
        InitializeComponent();
    }

    public string UserAgent
    {
        get => ( (OpenTopoCredentials) Credentials ).UserAgent;
        set => ( (OpenTopoCredentials) Credentials ).UserAgent = value;
    }

    public ICredentials Credentials { get; } = new OpenTopoCredentials();

    public bool CancelOnFailure { get; set; }
}
