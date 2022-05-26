using System;
using J4JSoftware.Logging;

namespace J4JSoftware.MapLibrary;

public class AffineMatrix
{
    public const double RadiansPerDegree = Math.PI / 180;

    private readonly IJ4JLogger? _logger;

    private double _rotationRadians;
    private double _scaleX = 1.0;
    private double _scaleY = 1.0;
    private double _offsetX;
    private double _offsetY;
    private double[ , ]? _matrix;

    public AffineMatrix(
        IJ4JLogger? logger
    )
    {
        _logger = logger;
        _logger?.SetLoggedType( GetType() );
    }

    public double GetRotation() => _rotationRadians / RadiansPerDegree;

    public void SetRotation( double value )
    {
        _rotationRadians = value * RadiansPerDegree;

        _matrix = null;
    }

    public double ScaleX
    {
        get => _scaleX;

        set
        {
            _scaleX = value;
            _matrix = null;
        }
    }

    public double ScaleY
    {
        get => _scaleY;

        set
        {
            _scaleY = value;
            _matrix = null;
        }
    }

    public void ScaleUniform( double scale )
    {
        ScaleX = scale;
        ScaleY = scale;
    }

    public double OffsetX
    {
        get => _offsetX;

        set
        {
            _offsetX = value;
            _matrix = null;
        }
    }

    public double OffsetY
    {
        get => _offsetY;

        set
        {
            _offsetY = value;
            _matrix = null;
        }
    }

    public void Offset( double xOffset, double yOffset )
    {
        OffsetX = xOffset;
        OffsetY = yOffset;
    }

    public double[,] Matrix
    {
        get
        {
            if( _matrix != null )
                return _matrix;

            _matrix = new double[ , ]
            {
                { _scaleX * Math.Cos( _rotationRadians ), -Math.Sin( _rotationRadians ), _offsetX },
                { Math.Sin( _rotationRadians ), _scaleY * Math.Cos( _rotationRadians ), _offsetY },
                { 0.0, 0.0, 1.0 }
            };

            return _matrix;
        }
    }

    //public AffineVector Transform( AffineVector vector )
    //{
    //    var retVal = new double[ 3 ];

    //    for( var row = 0; row < 3; row++ )
    //    {
    //        for( var col = 0; col < 3; col++ )
    //        {
    //            retVal[row] += vector.Values[row] * Matrix[row, col];
    //        }
    //    }

    //    return new AffineVector( retVal );
    //}
}