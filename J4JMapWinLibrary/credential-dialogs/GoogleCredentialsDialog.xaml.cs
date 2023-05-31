// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.J4JMapWinLibrary;

[CredentialsDialog(typeof(GoogleCredentials))]
public sealed partial class GoogleCredentialsDialog : ContentDialog, ICredentialsDialog
{
    public GoogleCredentialsDialog()
    {
        //if (MapControlViewModelLocator.Instance == null)
        //    throw new NullReferenceException($"{typeof(MapControlViewModelLocator)} was not initialized");

        //var temp = MapControlViewModelLocator.Instance.CredentialsFactory[ typeof( GoogleMapsProjection ), false ];
        //if( temp is not GoogleCredentials credentials )
        //    throw new NullReferenceException($"{typeof(GoogleCredentials)} could not be created");

        //_credentials = credentials;
        InitializeComponent();
    }

    public string ApiKey
    {
        get => ( (GoogleCredentials) Credentials ).ApiKey;
        set => ( (GoogleCredentials) Credentials ).ApiKey = value;
    }

    public string SignatureSecret
    {
        get => ( (GoogleCredentials) Credentials ).SignatureSecret;
        set => ( (GoogleCredentials) Credentials ).SignatureSecret = value;
    }

    public ICredentials Credentials { get; } = new GoogleCredentials();

    public bool CancelOnFailure { get; set; }
}
