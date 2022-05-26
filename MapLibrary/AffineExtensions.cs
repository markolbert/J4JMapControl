namespace J4JSoftware.MapLibrary;

public static class AffineExtensions
{
    public static double[] ExpandToAffine( this double[] source )
    {
        if( source.Length < 2 )
            throw new ArgumentException(
                $"{nameof( ExpandToAffine )}: source vector length ({source.Length}) must be >= 2" );

        return new[] { source[ 0 ], source[ 1 ], 1.0 };
    }

    public static double[] CreateAffineVector( double x, double y ) => new[] { x, y, 1.0 };

    public static double[] ToVector( this AffineVector affineVector ) =>
        new[] { affineVector.X, affineVector.Y };

    public static double[] DotProduct( this double[ , ] matrix, double[] vector )
    {
        if( matrix.Rank != 2 )
            throw new ArgumentException( $"{nameof(DotProduct)}: matrix is not two dimensional" );

        var numCols = matrix.GetLength( 1 );

        if ( vector.Length != numCols )
            throw new ArgumentException(
                $"{nameof( DotProduct )}: vector row count ({vector.Length}) does not equal matrix column count ({numCols})" );

        var retVal = new double[vector.Length];

        for (var row = 0; row < vector.Length; row++)
        {
            for (var col = 0; col < numCols; col++)
            {
                retVal[row] += vector[row] * matrix[row, col];
            }
        }

        return retVal;
    }

    public static double[] Add( this double[] vector1, double[] vector2 )
    {
        if( vector1.Length != vector2.Length )
            throw new ArgumentException(
                $"{nameof( Add )}: size of first vector ({vector1.Length}) != size of second vector ({vector2.Length})" );

        var retVal = new double[ vector1.Length ];

        for( var idx = 0; idx < vector1.Length; idx++ )
        {
            retVal[idx] = vector1[idx] + vector2[idx];
        }

        return retVal;
    }
}
