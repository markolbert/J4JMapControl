// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using J4JSoftware.J4JMapLibrary;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.J4JMapWinLibrary;

[CredentialsDialog(typeof(OpenTopoCredentials))]
public sealed partial class OpenTopoCredentialsDialog : ICredentialsDialog
{
    private readonly OpenTopoCredentials _credentials;

    public OpenTopoCredentialsDialog()
    {
        if (MapControlViewModelLocator.Instance == null)
            throw new NullReferenceException($"{typeof(MapControlViewModelLocator)} was not initialized");

        var temp = MapControlViewModelLocator.Instance.CredentialsFactory[ typeof( OpenTopoMapsProjection ), false ];
        if( temp is not OpenTopoCredentials credentials )
            throw new NullReferenceException($"{typeof(OpenTopoCredentials)} could not be created");

        _credentials = credentials;

        this.InitializeComponent();
    }

    public string UserAgent
    {
        get => _credentials.UserAgent;
        set => _credentials.UserAgent = value;
    }

    public ICredentials Credentials => _credentials;

    public bool CancelOnFailure { get; set; }
}
