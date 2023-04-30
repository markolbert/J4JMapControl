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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.J4JMapWinLibrary;
public sealed partial class MessageBox : ContentDialog, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string? _text;

    public MessageBox()
    {
        this.InitializeComponent();
    }

    public string? Text
    {
        get => _text;
        set => SetField( ref _text, value );
    }

    private bool SetField<T>( ref T field, T value, [ CallerMemberName ] string? propertyName = null )
    {
        if( EqualityComparer<T>.Default.Equals( field, value ) )
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        return true;
    }
}
