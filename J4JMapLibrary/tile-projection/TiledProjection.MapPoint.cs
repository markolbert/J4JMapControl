using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract partial class TiledProjection
{
    public class MapPoint
    {
        static MapPoint()
        {
            CreateMapPointInternal = ( proj, logger ) => new MapPoint( proj, logger );
        }

        private MapPoint(
            ITiledProjection projection,
            IJ4JLogger logger
        )
        {
            Projection = projection;
            Scale = projection.Scale;

            LatLong = CreateLatLongInternal( projection, logger );
            Cartesian = CreateCartesianInternal( projection, logger );
        }

        public ITiledProjection Projection { get; }

        public LatLong LatLong { get; }
        public Cartesian Cartesian { get; }
        public int Scale { get; }
        public bool ReflectsProjection => Scale == Projection.Scale;

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
