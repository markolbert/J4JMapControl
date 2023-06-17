namespace J4JSoftware.J4JMapLibrary;

public interface ILoadedRegion
{
    bool Succeeded { get; }
    float? Zoom { get; }

    int FirstRow { get; }
    int LastRow { get; }

    int FirstColumn { get; }
    int LastColumn { get; }

    MapBlock? GetBlock(int row, int column);
}
