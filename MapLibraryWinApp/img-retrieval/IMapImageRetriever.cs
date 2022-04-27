using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml.Media;

namespace J4JSoftware.J4JMapControl
{
    //public interface IMapImageRetriever
    //{
    //    string Description { get; }
    //    string CopyrightText { get; }
    //    Uri CopyrightUri { get; }

    //    Task<ImageRetrievalResult> GetImageSourceAsync( object tile );
    //}

    public interface IRetrieveMapImage<in TTile> : IMapImageRetriever
        where TTile : TileCoordinates
    {
        Task<ImageRetrievalResult> GetImageSourceAsync( TTile tile );
    }
}
