// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.J4JMapWinLibrary;

[CredentialsDialog(typeof(OpenStreetCredentials))]
public sealed partial class OpenStreetCredentialsDialog : ContentDialog, ICredentialsDialog
{
    public OpenStreetCredentialsDialog()
    {
        //if (MapControlViewModelLocator.Instance == null)
        //    throw new NullReferenceException($"{typeof(MapControlViewModelLocator)} was not initialized");

        //var temp = MapControlViewModelLocator.Instance.CredentialsFactory[ typeof( OpenStreetMapsProjection ), false ];
        //if( temp is not OpenStreetCredentials credentials )
        //    throw new NullReferenceException($"{typeof(OpenStreetCredentials)} could not be created");
        InitializeComponent();
    }

    public string UserAgent
    {
        get => ( (OpenStreetCredentials) Credentials ).UserAgent;
        set => ( (OpenStreetCredentials) Credentials ).UserAgent = value;
    }

    public ICredentials Credentials { get; } = new OpenStreetCredentials();

    public bool CancelOnFailure { get; set; }
}
