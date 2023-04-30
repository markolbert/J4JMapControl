// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using J4JSoftware.J4JMapLibrary;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.J4JMapWinLibrary;

[CredentialsDialog(typeof(BingCredentials))]
public sealed partial class BingCredentialsDialog : ContentDialog, ICredentialsDialog
{
    private readonly BingCredentials _credentials;

    public BingCredentialsDialog()
    {
        if (MapControlViewModelLocator.Instance == null)
            throw new NullReferenceException($"{typeof(MapControlViewModelLocator)} was not initialized");

        var temp = MapControlViewModelLocator.Instance.CredentialsFactory[ typeof( BingCredentials ), false ];
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
