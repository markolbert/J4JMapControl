using System.Numerics;

namespace J4JSoftware.MapLibrary;

public record AffineVector
{
    public AffineVector( Vector<double> vector )
        : this( new double[] { vector[ 0 ], vector[ 1 ] } )
    {
    }

    public AffineVector( double[] vector )
    {
        Values = new double[ 3 ];

        for( var idx = 0; idx < vector.Length && idx < 3; idx++ )
        {
            Values[idx] = vector[idx];

            switch ( idx )
            {
                case 0:
                    X = vector[idx];
                    break;

                case 1:
                    Y = vector[idx];
                    break;
            }
        }
    }

    public AffineVector(
        double x,
        double y,
        double lastElement = 1.0
    )
    {
        X = x;
        Y = y;
        Values = new[] { x, y, lastElement };
    }

    public double X { get; }
    public double Y { get; }
    public double[] Values { get; }
}
