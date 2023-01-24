namespace J4JMapLibrary;

public class RectangleSize
{
    private int _width;
    private int _height;

    public int Width
    {
        get => _width; 
        set => _width = value < 0 ? 0 : value;
    }

    public int Height
    {
        get => _height;
        set => _height = value < 0 ? 0 : value;
    }
}
