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
