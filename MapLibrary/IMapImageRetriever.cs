using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.MapLibrary
{
    public interface IMapImageRetriever
    {
        MapRetrieverInfo MapRetrieverInfo { get; }

        Task<ImageRetrievalResult> GetImageSourceAsync(object tile);
    }
}
