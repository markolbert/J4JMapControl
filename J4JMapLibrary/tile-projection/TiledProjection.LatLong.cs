using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract partial class TiledProjection
{
    public class LatLong
    {
        static LatLong()
        {
            CreateLatLongInternal = (proj, logger) => new LatLong(proj, logger);
        }

        private readonly IJ4JLogger _logger;

        private double _latitude;
        private double _longitude;

        private LatLong(
            ITiledProjection projection,
            IJ4JLogger logger
        )
        {
            Projection = projection;
            Scale = projection.Scale;

            _logger = logger;
            _logger.SetLoggedType( GetType() );
        }

        public ITiledProjection Projection { get; }
        public int Scale { get; }
        public bool ReflectsProjection => Scale == Projection.Scale;

        public double Latitude
        {
            get => _latitude;

            set =>
                _latitude = InternalExtensions.ConformValueToRange( value,
                                                                    Projection.MinLatitude,
                                                                    Projection.MaxLatitude,
                                                                    "Latitude",
                                                                    _logger );
        }

        public double Longitude
        {
            get=> _longitude;

            set =>
                _longitude =
                    InternalExtensions.ConformValueToRange( value,
                                                            Projection.MinLongitude,
                                                            Projection.MaxLongitude,
                                                            "Longitude",
                                                            _logger );
        }
    }
}