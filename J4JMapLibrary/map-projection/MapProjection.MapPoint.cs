using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract partial class MapProjection
{
    public class MapPoint
    {
        static MapPoint()
        {
            CreateMapPointInternal = ( proj, logger ) => new MapPoint( proj, logger );
        }

        private MapPoint(
            IMapProjection projection,
            IJ4JLogger logger
        )
        {
            Projection = projection;

            LatLong = CreateLatLongInternal( projection, logger );
            LatLong.Changed += ( _, _ ) => UpdateCartesian();

            Cartesian = CreateCartesianInternal( projection, logger );
            Cartesian.Changed += ( _, _ ) => UpdateLatLong();
        }

        public IMapProjection Projection { get; }

        public LatLong LatLong { get; }
        public Cartesian Cartesian { get; }

        // X & Y are presumed to be valid if this method is called
        protected internal void UpdateLatLong()
        {
            var latLong = Projection.CartesianToLatLong( Cartesian.X, Cartesian.Y );

            LatLong.Latitude = latLong.Latitude; 
            LatLong.Longitude = latLong.Longitude;
        }

        // Latitude & Longitude are presumed to be valid if this method is called
        protected internal void UpdateCartesian()
        {
            var cartesian = Projection.LatLongToCartesian( LatLong.Latitude, LatLong.Longitude );

            Cartesian.X = cartesian.X; 
            Cartesian.Y = cartesian.Y;
        }
    }
}
