#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// MapPin.cs
//
// This file is part of JumpForJoy Software's J4JMapWinLibrary.
// 
// J4JMapWinLibrary is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JMapWinLibrary is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JMapWinLibrary. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Numerics;
using Windows.Foundation;
using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class MapPin : Control
{
    private Path? _pinPath;

    public MapPin()
    {
        DefaultStyleKey = typeof( MapPin );
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _pinPath = FindUIElement<Path>( "ForegroundPath" );

        InitializePin();
    }

    private T? FindUIElement<T>( string name, Action<T>? postProcessor = null )
        where T : UIElement
    {
        var retVal = GetTemplateChild( name ) as T;
        if( retVal != null )
            postProcessor?.Invoke( retVal );

        return retVal;
    }

    private void InitializePin()
    {
        if( _pinPath == null )
            return;

        var pathFigure = new PathFigure { IsClosed = true, IsFilled = true, StartPoint = new Point( 0, ArcRadius ) };

        pathFigure.Segments.Add( new ArcSegment
        {
            IsLargeArc = false,
            Point = new Point( 2 * ArcRadius, ArcRadius ),
            SweepDirection = SweepDirection.Clockwise,
            Size = new Size( ArcRadius, ArcRadius )
        } );

        pathFigure.Segments.Add( new LineSegment { Point = new Point( ArcRadius, ArcRadius + TailLength ) } );

        var geometry = new PathGeometry();
        geometry.Figures.Add( pathFigure );

        _pinPath.Data = geometry;
        _pinPath.Width = 2 * ArcRadius;
        _pinPath.Height = ArcRadius + TailLength;
    }

    protected override Size MeasureOverride( Size availableSize ) => new( 2 * ArcRadius, ArcRadius + TailLength );
}
