using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract partial class MapProjection
{
    public class LatLong
    {
        public event EventHandler? Changed;

        static LatLong()
        {
            CreateLatLongInternal = (proj, logger) => new LatLong(proj, logger);
        }

        private readonly IJ4JLogger _logger;

        private double _latitude;
        private double _longitude;

        private LatLong(
            IMapProjection projection,
            IJ4JLogger logger
        )
        {
            Projection = projection;

            _logger = logger;
            _logger.SetLoggedType( GetType() );
        }

        public IMapProjection Projection { get; }

        public double Latitude
        {
            get => _latitude;

            set
            {
                _latitude = MapExtensions.ConformValueToRange( value,
                                                               Projection.MinLatitude,
                                                               Projection.MaxLatitude,
                                                               "Latitude",
                                                               _logger );

                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public double Longitude
        {
            get=> _longitude;

            set
            {
                _longitude =
                    MapExtensions.ConformValueToRange( value,
                                                       Projection.MinLongitude,
                                                       Projection.MaxLongitude,
                                                       "Longitude",
                                                       _logger );

                Changed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}