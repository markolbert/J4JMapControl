// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using J4JSoftware.J4JMapLibrary;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.J4JMapWinLibrary;

[CredentialsDialog(typeof(BingCredentials))]
public sealed partial class BingCredentialsDialog : ICredentialsDialog
{
    private readonly BingCredentials _credentials;

    public BingCredentialsDialog()
    {
        if (MapControlViewModelLocator.Instance == null)
            throw new NullReferenceException($"{typeof(MapControlViewModelLocator)} was not initialized");

        var temp = MapControlViewModelLocator.Instance.CredentialsFactory[ typeof( BingMapsProjection ), false ];
        if( temp is not BingCredentials bingCred )
            throw new NullReferenceException($"{typeof(BingCredentials)} could not be created");

        _credentials = bingCred;

        this.InitializeComponent();
    }

    public string ApiKey
    {
        get => _credentials.ApiKey;
        set => _credentials.ApiKey = value;
    }

    public ICredentials Credentials => _credentials;

    public bool CancelOnFailure { get; set; }
}
