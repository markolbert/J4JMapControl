// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.Logging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl : Panel
{
    private readonly IJ4JLogger _logger;

    private ImageFragments? _fragments;

    public J4JMapControl()
    {
        _logger = J4JDeusEx.GetLogger()!;
        _logger.SetLoggedType( GetType() );
    }

    public IProjection MapProjection
    {
        get => (IProjection) GetValue( MapProjectionProperty );
        set => SetValue( MapProjectionProperty, value );
    }

    public float Latitude
    {
        get => (float) GetValue( LatitudeProperty );
        set => SetValue( LatitudeProperty, value );
    }

    public float Longitude
    {
        get => (float) GetValue( LongitudeProperty );
        set => SetValue( LongitudeProperty, value );
    }

    public int MapScale
    {
        get => (int) GetValue( MapScaleProperty );
        set => SetValue( MapScaleProperty, value );
    }

    public float Heading
    {
        get => (float) GetValue( HeadingProperty );
        set => SetValue( HeadingProperty, value );
    }

    protected override Size MeasureOverride( Size availableSize )
    {
        return base.MeasureOverride( availableSize );
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
        return base.ArrangeOverride( finalSize );
    }
}
