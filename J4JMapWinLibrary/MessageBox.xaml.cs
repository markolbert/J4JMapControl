// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class MessageBox : ContentDialog, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _titleText = "Message";
    private string? _text;

    public MessageBox()
    {
        InitializeComponent();
    }

    public string TitleText
    {
        get => _titleText;
        set => SetField( ref _titleText, value );
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
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );

        return true;
    }
}
