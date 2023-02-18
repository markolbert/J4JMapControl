// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// 
// This file is part of ConsoleUtilities.
//
// ConsoleUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// ConsoleUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with ConsoleUtilities. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Threading.Tasks;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.J4JMapWinLibrary;

public record MapImage( Image Image, int X, int Y ) : IImagedFragment
{
    public float ActualHeight => (float) Image.ActualHeight;
    public float ActualWidth => (float) Image.ActualWidth;
}

public class ImageFragments : MapFragments<MapImage>
{
    private static async Task<MapImage?> DefaultFactory(IMapFragment fragment)
    {
        var imageBytes = await fragment.GetImageAsync();
        if( imageBytes == null )
            return null;

        var memStream = new MemoryStream(imageBytes);
        var bitmapImage = new BitmapImage();
        await bitmapImage.SetSourceAsync( memStream.AsRandomAccessStream() );

        var image = new Image { Source = bitmapImage };
        return new MapImage( image, fragment.X, fragment.Y );
    }

    public ImageFragments( 
        IProjection projection, 
        IJ4JLogger logger )
        : base( projection, DefaultFactory, logger )
    {
    }

    protected override void OnConfigurationChanged()
    {
        base.OnConfigurationChanged();
    }
}
