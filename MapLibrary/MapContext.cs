using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.DeusEx;

#pragma warning disable CS8618

namespace J4JSoftware.MapLibrary;

public class MapContext : IMapContext
{
    public event EventHandler? MapImageRetrieverChanged;
    public event EventHandler? ZoomChanged;
    public event EventHandler? ViewRectangleChanged;

    private IMapImageRetriever? _mapImageRetriever;
    private IZoom? _zoom;
    private MapRect? _viewRect;

    public IMapImageRetriever MapImageRetriever
    {
        get
        {
            if( _mapImageRetriever != null )
                return _mapImageRetriever;

            var msg = $"Trying to access an unconfigured {typeof( MapContext )}";

            J4JDeusEx.OutputFatalMessage( msg, null );
            throw new J4JDeusExException( msg );
        }

        set
        {
            _mapImageRetriever = value;
            MapImageRetrieverChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public IZoom Zoom
    {
        get
        {
            if (_zoom != null)
                return _zoom;

            var msg = $"Trying to access an unconfigured {typeof(MapContext)}";

            J4JDeusEx.OutputFatalMessage(msg, null);
            throw new J4JDeusExException(msg);
        }

        set
        {
            _zoom = value;
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public MapRect? ViewRect
    {
        get => _viewRect;

        set
        {
            _viewRect = value;
            ViewRectangleChanged?.Invoke( this, EventArgs.Empty );
        }
    }
}