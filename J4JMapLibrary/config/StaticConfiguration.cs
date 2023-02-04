using J4JSoftware.DeusEx;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class StaticConfiguration : SourceConfiguration, IStaticConfiguration
{
    private readonly IJ4JLogger? _logger;

    private int _minScale;
    private int _maxScale;
    private int _heightWidth;

    public StaticConfiguration()
    {
        _logger = J4JDeusEx.GetLogger<StaticConfiguration>();
    }

    public string RetrievalUrl { get; set; } = string.Empty;

    public int MinScale
    {
        get => _minScale;

        set
        {
            if( value < 0 )
            {
                _logger?.Error("Attempting to set minimum scale to < 0, defaulting to 0");
                value = 0;
            }

            _minScale = value;
        }
    }

    public int MaxScale
    {
        get => _maxScale;

        set
        {
            if (value < 0)
            {
                _logger?.Error("Attempting to set maximum scale to < 0, defaulting to 0");
                value = 0;
            }

            _maxScale = value;
        }
    }

    public int TileHeightWidth
    {
        get => _heightWidth;

        set
        {
            if (value < 0)
            {
                _logger?.Error("Attempting to set tile height/width to < 0, defaulting to 256");
                value = 256;
            }

            _heightWidth = value;
        }
    }
}