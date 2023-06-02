// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.J4JMapWinLibrary;

[ CredentialsDialog( typeof( GoogleCredentials ) ) ]
public sealed partial class GoogleCredentialsDialog : ContentDialog, ICredentialsDialog
{
    public GoogleCredentialsDialog()
    {
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
