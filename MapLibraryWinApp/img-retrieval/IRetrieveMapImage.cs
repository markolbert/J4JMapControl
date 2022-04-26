using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml.Media;

namespace J4JSoftware.J4JMapControl
{
    public interface IRetrieveMapImage
    {
        Task<ImageSource?> GetImageSourceAsync( object tile );
    }

    public interface IRetrieveMapImage<in TTile> : IRetrieveMapImage
        where TTile : TileCoordinates
    {
        Task<ImageSource?> GetImageSourceAsync( TTile tile );
    }
}
