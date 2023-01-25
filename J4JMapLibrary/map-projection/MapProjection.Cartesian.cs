using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract partial class MapProjection
{
    public class Cartesian
    {
        public event EventHandler? Changed;

        static Cartesian()
        {
            CreateCartesianInternal = (proj, logger) => new Cartesian(proj, logger);
        }

        private readonly IJ4JLogger _logger;

        private int _x;
        private int _y;

        private Cartesian(
            IMapProjection projection,
            IJ4JLogger logger
        )
        {
            Projection = projection;

            _logger = logger;
            _logger.SetLoggedType( GetType() );
        }

        public IMapProjection Projection { get; }

        public int X
        {
            get => _x;

            set
            {
                _x = MapExtensions.ConformValueToRange( value,
                                                        Projection.MinX,
                                                        Projection.MaxX,
                                                        "X",
                                                        _logger );

                Changed?.Invoke( this, EventArgs.Empty );
            }
        }

        public int Y
        {
            get=> _y;

            set
            {
                _y =
                    MapExtensions.ConformValueToRange( value,
                                                       Projection.MinY,
                                                       Projection.MaxY,
                                                       "Y",
                                                       _logger );

                Changed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}