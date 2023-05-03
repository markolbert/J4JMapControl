// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.J4JMapWinLibrary;

[CredentialsDialog(typeof(GoogleCredentials))]
public sealed partial class GoogleCredentialsDialog : ICredentialsDialog
{
    private readonly GoogleCredentials _credentials;

    public GoogleCredentialsDialog()
    {
        if (MapControlViewModelLocator.Instance == null)
            throw new NullReferenceException($"{typeof(MapControlViewModelLocator)} was not initialized");

        var temp = MapControlViewModelLocator.Instance.CredentialsFactory[ typeof( GoogleMapsProjection ), false ];
        if( temp is not GoogleCredentials credentials )
            throw new NullReferenceException($"{typeof(GoogleCredentials)} could not be created");

        _credentials = credentials;

        this.InitializeComponent();
    }

    public string ApiKey
    {
        get => _credentials.ApiKey;
        set => _credentials.ApiKey = value;
    }

    public string SignatureSecret
    {
        get => _credentials.SignatureSecret;
        set => _credentials.SignatureSecret = value;
    }

    public ICredentials Credentials => _credentials;

    public bool CancelOnFailure { get; set; }
}
