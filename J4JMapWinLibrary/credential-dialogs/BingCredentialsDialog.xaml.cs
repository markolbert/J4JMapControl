// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.J4JMapWinLibrary;

[CredentialsDialog(typeof(BingCredentials))]
public sealed partial class BingCredentialsDialog : ContentDialog, ICredentialsDialog
{
    public BingCredentialsDialog()
    {
        //if (MapControlViewModelLocator.Instance == null)
        //    throw new NullReferenceException($"{typeof(MapControlViewModelLocator)} was not initialized");

        //var temp = MapControlViewModelLocator.Instance.CredentialsFactory[ typeof( BingMapsProjection ), false ];
        //if( temp is not BingCredentials bingCred )
        //    throw new NullReferenceException($"{typeof(BingCredentials)} could not be created");

        InitializeComponent();
    }

    public string ApiKey
    {
        get => ( (BingCredentials) Credentials ).ApiKey;
        set => ( (BingCredentials) Credentials ).ApiKey = value;
    }

    public ICredentials Credentials { get; } = new BingCredentials();

    public bool CancelOnFailure { get; set; }
}
